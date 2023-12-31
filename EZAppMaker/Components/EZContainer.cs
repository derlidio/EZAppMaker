﻿/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Windows.Input;

using EZAppMaker.Attributes;
using EZAppMaker.Support;
using EZAppMaker.Bridges;
using EZAppMaker.Interfaces;

namespace EZAppMaker.Components
{
    public enum EZRaiseResult
    {
        Canceled, Success, NotFound
    }

    public class EZContainer : ContentView
    {
        private readonly Grid floater;
        private readonly EZScrollBridge scroller;
        private readonly ContentView target;
        private readonly EZBalloon balloon;
        private readonly EZKeyboardDispatcher keyboard_dispatcher;
        private readonly EZMenu menu;
        private readonly EZSpinner indicator;
        private readonly Grid blocker;

        private readonly List<EZContentView> ContentViewStack;

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim scrolling = new SemaphoreSlim(1, 1);

        public readonly SemaphoreSlim Resizing = new SemaphoreSlim(1, 1);

        private VisualElement focused;

        public delegate void OnReadyHandler();
        public event OnReadyHandler OnReady;

        public ICommand OnMenuBlockerTap { get; private set; }

        public EZContainer()
        {
            SizeChanged += OnSizeChanged;

            OnMenuBlockerTap = new Command(Handle_TopMenuBlockerTap);

            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZContainerTemplate"];

            floater = (Grid)GetTemplateChild("EZFloater");
            scroller = (EZScrollBridge)GetTemplateChild("EZScroller");
            target = (ContentView)GetTemplateChild("EZContentTarget");
            balloon = (EZBalloon)GetTemplateChild("EZBalloonAlert");
            keyboard_dispatcher = (EZKeyboardDispatcher)GetTemplateChild("EZBalloonKeyboard");
            menu = (EZMenu)GetTemplateChild("EZAppMenu");
            indicator = (EZSpinner)GetTemplateChild("EZTimeConsumingActivity");
            blocker = (Grid)GetTemplateChild("EZBlocker");

            ContentViewStack = new List<EZContentView>();

            if (EZWorkarounds.ScrollViewContentSize) /* WORKAROUND */
            {
                // On iOS, whenever the ContentSize of a ScrollView changes, it will (may)
                // shift that content down, changing the value of it's Y property. This is
                // a problem for EZAppMaker's Combo! To the date of this writing, there are
                // many bug reports related to MAUI ScrollView. This is just another one.
                // This workaround has been made for MAUI + .NET 8 RC2.

                target.PropertyChanged += Target_PropertyChanged;
            }            
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Y")
            {
                var view = (ContentView)sender;
                view.TranslationY = -view.Y;
            }
        }

        //  ___      _ _   _      _ _         _   _          
        // |_ _|_ _ (_) |_(_)__ _| (_)_____ _| |_(_)___ _ _  
        //  | || ' \| |  _| / _` | | |_ / _` |  _| / _ \ ' \ 
        // |___|_||_|_|\__|_\__,_|_|_/__\__,_|\__|_\___/_||_|

        private void OnSizeChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"EZContainer.OnSizeChanged(): [{Math.Floor(Width)}x{Math.Floor(Height)}]");

            if (Initialized)
            {
                return;
            }

            Initialized = (Width > 0) && (Height > 0);

            if (Initialized)
            {
                System.Diagnostics.Debug.WriteLine($"EZContainer.OnReady(): [{Math.Floor(Width)}x{Math.Floor(Height)}]");

                OnReady?.Invoke();
            }
        }

        //  ___                       _   _        
        // | _ \_ _ ___ _ __  ___ _ _| |_(_)___ ___
        // |  _/ '_/ _ \ '_ \/ -_) '_|  _| / -_|_-<
        // |_| |_| \___/ .__/\___|_|  \__|_\___/__/
        //             |_|

        public bool Initialized { get; private set; } = false;
        public double TopOffset { get; set; } = 5;

        public bool IsMenuVisible
        {
            get
            {
                return menu.IsOpen;
            }
        }

        public EZContentView CurrentView
        {
            get
            {
                int top = ContentViewStack.Count - 1;

                if (top == -1) return null;

                return ContentViewStack[top];
            }
        }

        public int StackCount
        {
            get
            {
                return ContentViewStack.Count;
            }
        }

        public void BuildMenu()
        {
            System.Diagnostics.Debug.WriteLine("EZContainer.BuildMenu()");

            EZApp.Builder.BuildMenu(menu);

            menu?.SetBlocker((Grid)GetTemplateChild("EZMenuBlocker"));
        }

        public async void ThemeChanged()
        {
            menu?.ThemeChanged();
            balloon?.ThemeChanged();
            keyboard_dispatcher?.ThemeChanged();

            await semaphore.WaitAsync();

            foreach (EZContentView cv in ContentViewStack)
            {
                cv.ThemeChanged();
            }

            semaphore.Release();
        }

        public void ShowMenu(bool animated = true)
        {
            menu?.Show(animated);
        }

        public void HideMenu(bool animated = true)
        {
            menu?.Hide(animated);
        }

        public void DisableScrolling()
        {
            scroller.ScrollingEnabled = false;
        }

        public void EnableScrolling()
        {
            scroller.ScrollingEnabled = true;
        }

        public void ShowBalloon(VisualElement element, string text)
        {
            balloon.Show(element, text);
        }

        public void HideBalloon()
        {
            balloon.Hide();
        }

        public void ShowKeyboardDispatcher(VisualElement element, bool search = false)
        {
            keyboard_dispatcher.Show(element, search);
        }

        public void HideKeyboard()
        {
            if (focused != null)
            {
                ((IEZFocusable)focused).Unfocus();
            }
        }

        public void HideKeyboardDispatcher()
        {
            keyboard_dispatcher.Hide();
        }

        public void ShowActivityIndicator()
        {
            indicator.IsVisible = true;
            indicator.IsSpinning = true;
        }

        public void HideActivityIndicator()
        {
            indicator.IsSpinning = false;
            indicator.IsVisible = false;
        }

        public async Task NavigateTo(string item)
        {
            EZRaiseResult raised = await EZApp.Container.RaiseContentView(item);

            if (raised == EZRaiseResult.NotFound)
            {
                EZContentView view = EZApp.Builder.BuildContentView(item);

                if (view != null)
                {
                    await EZApp.Container.PushContentView(view);
                }
            }
        }

        //  _  _          _           _   _ 
        // | \| |__ ___ _(_)__ _ __ _| |_(_)___ _ _ 
        // | .` / _` \ V / / _` / _` |  _| / _ \ ' \
        // |_|\_\__,_|\_/|_\__, \__,_|\__|_\___/_||_|
        //                 |___/                                              

        public async Task PushContentView(EZContentView view)
        {
            if (view == null) return;

            bool can_leave = true;

            EZContentView hiding = null;

            await semaphore.WaitAsync();
            {
                blocker.IsVisible = true;

                HideBalloon();

                int top = ContentViewStack.Count - 1;

                if (top != -1)
                {
                    can_leave = ContentViewStack[top].OnBeforeHiding();
                }

                if (can_leave)
                {
                    if (view.ShowSpinner)
                    {
                        EZApp.Indicator.Show();
                        await Task.Delay(250);
                    }

                    if (top != -1) // <- This may seem unnecessary, but it is! Believe me :)
                    {
                        hiding = ContentViewStack[top];
                        hiding.ScrollY = scroller.ScrollY;

                        if (EZSettings.SmoothTransitions)
                        {
                            await scroller.FadeTo(0, 250);
                            await Task.Delay(50);
                        }
                        else
                        {
                            scroller.Opacity = 0D;
                        }
                        
                        await scroller.ScrollToAsync(0D, 0D, false);
                    }

                    view.Container = this;
                    ContentViewStack.Add(view);
                    target.Content = view;
                }

                blocker.IsVisible = false;
            }
            semaphore.Release();

            hiding?.OnHidden();

            if (EZSettings.SmoothTransitions)
            {
                await scroller.FadeTo(1, 250);
            }
            else
            {
                scroller.Opacity = 1D;
            }

            if (view.ShowSpinner)
            {
                EZApp.Indicator.Hide();
            }

            view.OnAppearing();
        }

        public async Task PopContentView()
        {
            EZContentView leaving = null;
            EZContentView raising = null;

            await semaphore.WaitAsync();
            {
                if (ContentViewStack.Count != 0)
                {
                    HideBalloon();

                    blocker.IsVisible = true;

                    int top = ContentViewStack.Count - 1;

                    bool can_leave = ContentViewStack[top].OnBeforeLeaving();

                    if (can_leave)
                    {
                        leaving = ContentViewStack[top];

                        if (EZSettings.SmoothTransitions)
                        {
                            await scroller.FadeTo(0, 250, Easing.Linear);
                            await Task.Delay(50);
                        }
                        else
                        {
                            scroller.Opacity = 0D;
                        }

                        await scroller.ScrollToAsync(0D, 0D, false);

                        target.Content = null;

                        ContentViewStack.Remove(leaving);

                        if (top > 0)
                        {
                            top--;

                            raising = ContentViewStack[top];
                            target.Content = raising;

                            await Task.Delay(250); // Give some time for MAUI to compose the page.
                        }
                        else
                        {
                            EZApp.Builder.CurrentViewChanged(null);
                        }
                    }

                    blocker.IsVisible = false;
                }
            }
            semaphore.Release();

            leaving?.OnLeaving();

            if (raising != null)
            {
                if (EZSettings.SmoothTransitions)
                {
                    await scroller.FadeTo(1, 250);
                }
                else
                {
                    scroller.Opacity = 1D;
                }

                raising.OnRaised();
            }

            GC.Collect();
        }

        public async Task<EZRaiseResult> RaiseContentView(string ItemId)
        {
            EZRaiseResult result = EZRaiseResult.NotFound;

            EZContentView raising = null;
            EZContentView hiding = null;

            await semaphore.WaitAsync();
            {
                HideBalloon();

                blocker.IsVisible = true;

                int top = ContentViewStack.Count - 1;

                for (int i = 0; i < ContentViewStack.Count; i++)
                {
                    if (ContentViewStack[i].ItemId == ItemId)
                    {
                        result = EZRaiseResult.Canceled;

                        if (i < top)
                        {
                            bool can_leave = ContentViewStack[top].OnBeforeHiding();

                            if (can_leave)
                            {
                                hiding = ContentViewStack[top];
                                hiding.ScrollY = scroller.ScrollY;

                                if (EZSettings.SmoothTransitions)
                                {
                                    await scroller.FadeTo(0, 250);
                                    await Task.Delay(50);
                                }
                                else
                                {
                                    scroller.Opacity = 0D;
                                }

                                await scroller.ScrollToAsync(0D, 0D, false);

                                MoveToStackTop(i);

                                target.Content = ContentViewStack[top];

                                await Task.Delay(250); // Give some time for MAUI to compose the page.
                      
                                // Notifies the page about it being rised to the top of the stack:

                                raising = ContentViewStack[top];

                                result = EZRaiseResult.Success;
                            }
                        }
                        break;
                    }
                }

                blocker.IsVisible = false;
            }
            semaphore.Release();

            hiding?.OnHidden();

            if (raising != null)
            {
                if (EZSettings.SmoothTransitions)
                {
                    await scroller.FadeTo(1, 250);
                }
                else
                {
                    scroller.Opacity = 1D;
                }

                raising.OnRaised();
            }

            return result;
        }

        public async Task RemoveContentView(string ItemId)
        {
            EZContentView leaving = null;

            await semaphore.WaitAsync();
            {
                foreach (EZContentView view in ContentViewStack)
                {
                    if (view.ItemId == ItemId)
                    {
                        leaving = view;

                        if (target.Content == view)
                        {
                            target.Content = null;
                        }

                        ContentViewStack.Remove(leaving);

                        break;
                    }
                }
            }
            semaphore.Release();

            leaving?.OnLeaving();
        }

        public async Task RemoveContentView(EZContentView view)
        {
            if (view == null) return;

            EZContentView leaving = null;

            await semaphore.WaitAsync();
            {
                if (ContentViewStack.Contains(view))
                {
                    leaving = view;

                    if (target.Content == view)
                    {
                        target.Content = null;
                    }

                    ContentViewStack.Remove(leaving);
                }
            }
            semaphore.Release();

            leaving?.OnLeaving();
        }

        public async Task RemoveContentGroup(string group)
        {
            if (string.IsNullOrWhiteSpace(group)) return;

            List<EZContentView> leaving = new List<EZContentView>();

            await semaphore.WaitAsync();
            {
                for (int i = ContentViewStack.Count - 1; i >= 0; i--)
                {
                    EZContentView cv = ContentViewStack[i];

                    if (cv.Group == group)
                    {
                        ContentViewStack.Remove(cv);

                        leaving.Add(cv);
                    }
                }

                foreach (EZContentView cv in leaving)
                {
                    if (target.Content == cv)
                    {
                        target.Content = null;
                        break;
                    }
                }
            }
            semaphore.Release();

            foreach (EZContentView view in leaving)
            {
                view.OnLeaving();
            }
        }

        private void MoveToStackTop(int index)
        {
            int last = ContentViewStack.Count - 1;

            string group = ContentViewStack[index].Group;

            if (!string.IsNullOrWhiteSpace(group))
            {
                List<EZContentView> Group = new List<EZContentView>();

                for (int j = last; j >= index; j--)
                {
                    EZContentView cv = ContentViewStack[j];

                    if (cv.Group == group)
                    {
                        ContentViewStack.Remove(cv);
                        Group.Add(cv);
                    }
                }

                for (int j = Group.Count - 1; j >= 0; j--)
                {
                    ContentViewStack.Add(Group[j]);
                }
            }
            else
            {
                EZContentView cv = ContentViewStack[index];
                ContentViewStack.Remove(cv); // Get the item from it's current position
                ContentViewStack.Add(cv);    // and put it on the top of the satack.
            }
        }

        public async Task ClearContentViewStack()
        {
            await semaphore.WaitAsync();

            ContentViewStack.Clear();

            semaphore.Release();
        }

        //   _  _     _                  
        //  | || |___| |_ __  ___ _ _ ___
        //  | __ / -_) | '_ \/ -_) '_(_-<
        //  |_||_\___|_| .__/\___|_| /__/
        //             |_|

        public async Task<EZContentView> GetTopPage()
        {
            EZContentView cv = null;

            await semaphore.WaitAsync();

            if (ContentViewStack.Count > 0)
            {
                cv = ContentViewStack[ContentViewStack.Count - 1];
            }

            semaphore.Release();

            return cv;
        }

        public async Task<EZContentView> GetPage(Type type)
        {
            EZContentView cv = null;

            await semaphore.WaitAsync();

            foreach (EZContentView c in ContentViewStack)
            {
                if (c.GetType() == type)
                {
                    cv = c;
                    break;
                }
            }

            semaphore.Release();

            return cv;
        }

        public EZContentView GetPage(string ItemId)
        {
            EZContentView cv = null;

            semaphore.WaitAsync();

            if (!string.IsNullOrWhiteSpace(ItemId))
            {
                foreach (EZContentView c in ContentViewStack)
                {
                    if (c.ItemId == ItemId)
                    {
                        cv = c;
                        break;
                    }
                }
            }

            semaphore.Release();

            return cv;
        }

        public async void SendMessage(string ItemId, string MessageId, object MessageData, object Sender = null)
        {
            if (string.IsNullOrWhiteSpace(ItemId)) return;

            EZContentView receiver = null;

            await semaphore.WaitAsync();

            foreach (EZContentView cv in ContentViewStack)
            {
                if (cv.ItemId == ItemId)
                {
                    receiver = cv;
                    break;
                }
            }

            semaphore.Release();

            receiver?.ProcessMessage(MessageId, MessageData, Sender);
        }

        public async Task Scroll(VisualElement element, ScrollToPosition position, bool animated = true)
        {
            if (!IsScrollerChild(element)) return;

            PositionWithinScroller(element, out double x, out double y);

            double target = y;

            switch (position)
            {
                case ScrollToPosition.Start: target = y; break;
                case ScrollToPosition.End: target = y - Height - element.Height; break;
                case ScrollToPosition.Center: target = y - Height / 2 - element.Height / 2; break;
            }

            await Scroll(target, animated);
        }

        public async Task Scroll(double y, bool animated = true)
        {
            await scrolling.WaitAsync();
            
            double start = scroller.ScrollY;

            if (start != y)
            {
                System.Diagnostics.Debug.WriteLine("Scrolling...");

                if (!animated)
                {
                    _ = scroller.ScrollToAsync(0, y, false);
                    return;
                }

                Animation animation = new Animation
                (
                    callback: (ny) => { scroller.ScrollToAsync(0, ny, animated: false); },
                    start: start,
                    end: y
                );
                
                animation.Commit
                (
                    owner: this,
                    name: "Scroll",
                    length: 250,
                    easing: Easing.CubicInOut,
                    finished: (d, b) => { scrolling.Release(); System.Diagnostics.Debug.WriteLine("Positioned!"); }
                );
            }
            else
            {
                scrolling.Release();

                System.Diagnostics.Debug.WriteLine("Scrolling... [ no need ]");
            }
        }

        public void MakeVisible(VisualElement element, bool animated = true, bool focused = false)
        {
            System.Diagnostics.Debug.WriteLine("MakeVisible");

            if (element == null)
            {
                System.Diagnostics.Debug.WriteLine("Null element can't be spotted!");
                return;
            }

            if (!IsScrollerChild(element))
            {
                string id = (string)element.GetType().GetProperty("ItemId")?.GetValue(element);
                System.Diagnostics.Debug.WriteLine($"Can't make element visible through scrolling: {element.GetType()} [{id}]");
                return;
            }

            // Calculate the phisical position of the element's bottom on the screen.
            // If it is below the middle of the screen, it'll probably be covered by the
            // soft keyboard (on small devices). If this is the case, roll the scroller
            // to make the element visible (if possible).

            GetContainerPosition(element, out double x, out double y);

            y += element.Height;
            double desired = scroller.ScrollY + y - Height / 2;
            if (desired < 0) desired = 0;

            System.Diagnostics.Debug.WriteLine($"Desired offset: {Math.Floor(desired)}");

            Scroll(desired, animated).Wait();
        }

        public void GetContainerPosition(VisualElement element, out double x, out double y)
        {
            if ((element == null) || (element == target))
            {
                x = 0;
                y = 0;

                return;
            }

            x = element.X + scroller.X - scroller.ScrollX;
            y = element.Y + scroller.Y - scroller.ScrollY;

            VisualElement parent = (VisualElement)element.Parent;

            while ((parent != null) && (parent.Parent != scroller))
            {
                y += parent.Y;
                x += parent.X;

                parent = (VisualElement)parent.Parent;
            }
        }

        public void PositionWithinScroller(VisualElement element, out double x, out double y)
        {
            if ((element == null) || (element == target))
            {
                x = 0;
                y = 0;

                return;
            }

            x = element.X;
            y = element.Y;

            VisualElement parent = (VisualElement)element.Parent;

            while ((parent != null) && (parent.Parent != scroller))
            {
                y += parent.Y;
                x += parent.X;

                parent = (VisualElement)parent.Parent;
            }
        }

        public bool IsScrollerChild(Element element)
        {
            bool child = false;

            if (element != null)
            {
                Element parent = element.Parent;

                while (parent != null)
                {
                    if (parent == scroller)
                    {
                        child = true;
                        break;
                    }
                    parent = parent.Parent;
                }
            }

            return child;
        }

        public async Task HandleFocus(VisualElement element, bool state)
        {
            if (element == null) return;

            System.Diagnostics.Debug.WriteLine($"HandleFocus: {element.GetType()} -> {(state ? "Gain" : "Lost")}");

            switch (state)
            {
                case false:

                    if (focused == element)
                    {
                        HideKeyboardDispatcher();
                        await CurrentView?.Contract();

                        Entry e = (Entry)((IEZFocusable)focused).FocusedElement;
                        _ = await e.HideSoftInputAsync(new CancellationToken());
                        e.Unfocus();

                        focused = null;
                    }
                    break;

                case true:

                    if (focused != element)
                    {
                        IEZFocusable previous = (IEZFocusable)focused;
                        focused = element;
                        previous?.Unfocus();

                        await CurrentView?.Expand();

                        MakeVisible(element, true, true);

                        if (EZWorkarounds.ScrollViewContentSize)
                        {
                            // It seems that, if the main scroll is animating (scrolling or resizing)
                            // and the soft input enters, the entire screen will shift down. This will
                            // happen only on iOS. THE ENTIRE SCREEN WILL SHIFT DOWN! So, despite the
                            // fact that I hate using delays, it's needed here... :o(

                            await Task.Delay(300);
                        }

                        Entry e = (Entry)((IEZFocusable)focused).FocusedElement;
                        _ = await e.ShowSoftInputAsync(new CancellationToken());
                        e.Focus();

                        ShowKeyboardDispatcher(e);
                    }
                    break;
            }
        }

        public void AddFloater(View element)
        {
            if (element != null)
            {
                floater.Children.Add(element);
            }
        }

        public void RemoveFloater(View element)
        {
            if (element != null)
            {
                if (floater.Children.Contains(element))
                {
                    floater.Children.Remove(element);
                }
            }
        }

        [ComponentEventHandler]
        private void Handle_TopMenuBlockerTap()
        {
            HideMenu();
        }
    }
}