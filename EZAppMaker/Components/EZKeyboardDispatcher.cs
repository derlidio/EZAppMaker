/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using EZAppMaker.Attributes;
using EZAppMaker.Defaults;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public class EZKeyboardDispatcher : ContentView
    {
        private bool initialized = false;
        private VisualElement pointed = null;
        private readonly EZPathButton SearchButton;

        public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(EZKeyboardDispatcher), defaultValueCreator: bindable => Default.Color("ezkeyboarddispatcher_background"));
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Brush), typeof(EZKeyboardDispatcher), defaultValueCreator: bindable => Default.Brush("ezkeyboarddispatcher_border"));
        public static readonly BindableProperty ShadowColorProperty = BindableProperty.Create(nameof(ShadowColor), typeof(Brush), typeof(EZKeyboardDispatcher), defaultValueCreator: bindable => Default.Brush("ezkeyboarddispatcher_shadow"));
        public static readonly BindableProperty IconFillProperty = BindableProperty.Create(nameof(IconFill), typeof(Brush), typeof(EZKeyboardDispatcher), defaultValueCreator: bindable => Default.Brush("ezkeyboarddispatcher_icon"));

        private int showing = 0;
        private int leave = 0;

        public EZKeyboardDispatcher()
        {
            BindingContext = this;

            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZKeyboardDispatcherTemplate"];

            SearchButton = (EZPathButton)GetTemplateChild("SearchButton");

            SearchButton.OnTap += Handle_SearchTap;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (!initialized && (width > 0) && (height > 0))
            {
                initialized = true;
                IsVisible = false;
                Opacity = 1.0D;
            }
        }

        public void ThemeChanged()
        {
            BackgroundColor = Default.Color("ezkeyboarddispatcher_background");
            ShadowColor = Default.Brush("ezkeyboarddispatcher_shadow");
            BorderColor = Default.Color("ezkeyboarddispatcher_border");
            IconFill = Default.Color("ezkeyboarddispatcher_icon");
        }

        public new Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public Brush BorderColor
        {
            get => (Brush)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public Brush ShadowColor
        {
            get => (Brush)GetValue(ShadowColorProperty);
            set => SetValue(ShadowColorProperty, value);
        }

        public Brush IconFill
        {
            get => (Brush)GetValue(IconFillProperty);
            set => SetValue(IconFillProperty, value);
        }

        public async void Show(VisualElement element, bool search)
        {
            if (element == null)
            {
                System.Diagnostics.Debug.WriteLine("Null VisualElement passed to EZKeyboardDispatcher!");
                return;
            }
            
            pointed = element;
            SearchButton.IsVisible = search;

            if (0 != Interlocked.Exchange(ref showing, 1))
            {
                System.Diagnostics.Debug.WriteLine("EZKeyboardDispatcher: already visible!");
                return;
            }

            double xx = 0, yy = 0; uint length = 0U;

            while (leave == 0)
            {
                EZApp.Container.GetContainerPosition(pointed, out double x, out double y);

                y += pointed.Height;
                x += pointed.Width - Width / 2;

                if (x + Width > EZApp.Container.Width)
                {
                    x = EZApp.Container.Width - Width;
                }

                if (x != xx || y != yy)
                {
                    _ = await this.TranslateTo(x, y, length);

                    xx = x; yy = y; length = 250U;

                    IsVisible = true;
                }

                await Task.Delay(50);
            }

            IsVisible = false;

            Interlocked.Exchange(ref leave, 0);
            Interlocked.Exchange(ref showing, 0);
        }

        public void Hide()
        {
            if (showing == 1)
            {
                Interlocked.Exchange(ref leave, 1);
            }
        }

        [ComponentEventHandler]
        private void Handle_SearchTap(EZPathButton button)
        {
            ((EZEntry)pointed)?.OnSearchTapped?.Invoke((EZEntry)pointed);
        }
    }
}