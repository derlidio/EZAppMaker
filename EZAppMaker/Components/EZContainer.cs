/*__ ____  _             
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
        private readonly VerticalStackLayout target;
        private readonly EZBalloon balloon;
        private readonly EZKeyboardDispatcher keyboard_dispatcher;
        private readonly EZMenu menu;
        private readonly ActivityIndicator indicator;
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
            target = (VerticalStackLayout)GetTemplateChild("EZContentTarget");
            balloon = (EZBalloon)GetTemplateChild("EZBalloonAlert");
            keyboard_dispatcher = (EZKeyboardDispatcher)GetTemplateChild("EZBalloonKeyboard");
            menu = (EZMenu)GetTemplateChild("EZAppMenu");
            indicator = (ActivityIndicator)GetTemplateChild("EZTimeConsumingActivity");
            blocker = (Grid)GetTemplateChild("EZBlocker");

            ContentViewStack = new List<EZContentView>();
        }

        //  ___      _ _   _      _ _         _   _          
        // |_ _|_ _ (_) |_(_)__ _| (_)_____ _| |_(_)___ _ _  
        //  | || ' \| |  _| / _` | | |_ / _` |  _| / _ \ ' \ 
        // |___|_||_|_|\__|_\__,_|_|_/__\__,_|\__|_\___/_||_|

        private void OnSizeChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"EZContainer.OnSizeChanged(): [{Math.Floor(Width)}x{Math.Floor(Height)}]");

            if (Initialized) return;

            Initialized = (Width > 0) && (Height > 0);

            if (Initialized)
            {
                OnReady?.Invoke();

                System.Diagnostics.Debug.WriteLine($"EZContainer.OnReady(): [{Math.Floor(Width)}x{Math.Floor(Height)}]");
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

        public async Task TriggerLayout() /* WORKAROUND */
        {
            // On iOS our main ScrollView will not adjust it's scrollable
            // area size when it's content resizes. We need to force it by
            // invoking a Layout Pass, and this is made by changing it's
            // HeightRequest property (momentarily).

            if (!EZWorkarounds.ScrollViewContentSize) return;

            await Resizing.WaitAsync();
            {
                double height = scroller.Height;

                scroller.HeightRequest = height - 1;
                scroller.HeightRequest = height;

                await Task.Delay(100);

                Resizing.Release();
            }

            System.Diagnostics.Debug.WriteLine("Scroller Layout Pass triggered!");
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
            indicator.IsRunning = true;
            indicator.IsVisible = true;
        }

        public void HideActivityIndicator()
        {
            indicator.IsRunning = false;
            indicator.IsVisible = false;
        }

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
                    if (top != -1) // <- This may seem unnecessary, but it is! Believe me :)
                    {
                        hiding = ContentViewStack[top];
                        hiding.ScrollY = scroller.ScrollY;

                        if (EZSettings.SmoothTransitions)
                        {
                            await scroller.FadeTo(0, 250);
                        }
                        else
                        {
                            scroller.Opacity = 0D;
                        }
                        
                        await scroller.ScrollToAsync(target, ScrollToPosition.Start, false);
                    }

                    view.Container = this;

                    ContentViewStack.Add(view);

                    target.Clear();
                    target.Add(view);
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
                        }
                        else
                        {
                            scroller.Opacity = 0D;
                        }

                        await scroller.ScrollToAsync(target, ScrollToPosition.Start, false);

                        target.Clear();

                        ContentViewStack.Remove(leaving);

                        if (top > 0)
                        {
                            top--;

                            raising = ContentViewStack[top];
                            target.Add(raising);
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

                if (EZSettings.ContentReposition && (raising.ScrollY != 0))
                {
                    await scroller.ScrollToAsync(0, raising.ScrollY, true);
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
                                }
                                else
                                {
                                    scroller.Opacity = 0D;
                                }

                                await scroller.ScrollToAsync(target, ScrollToPosition.Start, false);

                                MoveToStackTop(i);

                                target.Clear();
                                target.Add(ContentViewStack[top]);

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

                if (EZSettings.ContentReposition && (raising.ScrollY != 0))
                {
                    await scroller.ScrollToAsync(0, raising.ScrollY, true);
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

                        if (target.Contains(view))
                        {
                            target.Clear();
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

                    if (target.Contains(leaving))
                    {
                        target.Clear();
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
                    if (target.Contains(cv))
                    {
                        target.Clear();
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

            if (receiver != null) receiver.ProcessMessage(MessageId, MessageData, Sender);
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
            
            System.Diagnostics.Debug.WriteLine("Scrolling...");

            if (!animated)
            {
                _ = scroller.ScrollToAsync(0, y, false);
                return;
            }

            double start = scroller.ScrollY;

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
            // to make the element visible.

            GetContainerPosition(element, out double x, out double y);

            y += element.Height;
            double target = scroller.ScrollY + y - Height / 2;
            if (target < 0) target = 0;

            Scroll(target, animated).Wait();

            if (focused && (element is IEZFocusable))
            {
                ShowKeyboardDispatcher(((IEZFocusable)element).FocusedElement);
            }
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

        [AsyncVoidOnPurpose]
        public async void HandleFocus(VisualElement element, bool state)
        {
            if (element == null) return;

            System.Diagnostics.Debug.WriteLine($"HandleFocus: {element.GetType()} -> {(state ? "Gain" : "Lost")}");

            switch (state)
            {
                case false:

                    if (focused == element)
                    {
                        HideKeyboardDispatcher();
                        CurrentView?.Contract();
                        Entry e = (Entry)((IEZFocusable)focused).FocusedElement;
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

                        await Resizing.WaitAsync();
                        MakeVisible(element, true, true);
                        Resizing.Release();

                        // If a scrolling animation is in progress, it will
                        // break the behavior of the soft input entrance and
                        // the entire screen may shift down, leaving a black
                        // area at the top and messing up with some touchable
                        // spots! This happens only on iOS (and made me crazy
                        // for some time, until I finally found the cause).

                        await scrolling.WaitAsync(); /* WORKAROUND */
                        Entry e = (Entry)((IEZFocusable)focused).FocusedElement;
                        e.Focus();
                        scrolling.Release();
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