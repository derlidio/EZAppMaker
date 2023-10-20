/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Windows.Input;

using EZAppMaker.Defaults;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public class EZBalloon : ContentView
    {
        private bool initialized = false;

        public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(EZBalloon), defaultValueCreator: bindable => Default.Color("ezballoon_background"));
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EZBalloon), defaultValueCreator: bindable => Default.Color("ezballoon_border"));
        public static readonly BindableProperty ShadowColorProperty = BindableProperty.Create(nameof(ShadowColor), typeof(Brush), typeof(EZBalloon), defaultValueCreator: bindable => Default.Brush("ezballoon_shadow"));
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(EZBalloon), defaultValueCreator: bindable => Default.Color("ezballoon_text"));

        private Frame pointer;
        private int showing = 0;
        private int leave = 0;

        public ICommand OnBalloonTap { get; private set; }

        public EZBalloon()
        {
            BindingContext = this;

            OnBalloonTap = new Command(Handle_BalloonTap);

            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZBalloonTemplate"];

            pointer = (Frame)GetTemplateChild("EZBalloonPointer");
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
            BackgroundColor = Default.Color("ezballoon_background");
            ShadowColor = Default.Brush("ezballoon_shadow");
            BorderColor = Default.Color("ezballoon_border");
            TextColor = Default.Color("ezballoon_text");
        }

        public string Text { get; private set; }

        public new Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        public Color BorderColor
        {
            get => (Color)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public Brush ShadowColor
        {
            get => (Brush)GetValue(ShadowColorProperty);
            set => SetValue(ShadowColorProperty, value);
        }

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public async void Show(VisualElement element, string text)
        {
            if (element == null) return;

            double xx = 0, yy = 0, pp = 0; uint length = 0;
            
            if (showing != 0)
            {
                Interlocked.Exchange(ref leave, 1);

                while (showing != 0)
                {
                    await Task.Delay(50);
                }
            }

            Interlocked.Exchange(ref showing, 1);
            {
                Text = text;

                OnPropertyChanged(nameof(Text));

                IsVisible = true;

                int tick = Environment.TickCount;

                while ((Environment.TickCount - tick < EZSettings.BallonTime) && (leave == 0))
                {
                    EZApp.Container.GetContainerPosition(element, out double x, out double y);

                    y += element.Height;
                    x += (element.Width / 2) - (Width / 2);

                    double offset = (x < 0) ? x : (x + Width) > EZApp.Container.Width ? (x + Width) - EZApp.Container.Width : 0;

                    x -= offset;

                    double p = (Width / 2) - (pointer.Width / 2) + offset;

                    if (x != xx || y != yy || p != pp)
                    {
                        _ = pointer.TranslateTo(p, 0, length);
                        _ = this.TranslateTo(x, y, length);

                        xx = x; yy = y; pp = p; length = 250U;
                    }
                    await Task.Delay(50);
                }

                IsVisible = false;

                Interlocked.Exchange(ref leave, 0);
            }
            Interlocked.Exchange(ref showing, 0);
        }

        public void Hide()
        {
            if (showing != 0)
            {
                Interlocked.Exchange(ref leave, 1);
            }
        }

        private void Handle_BalloonTap()
        {
            Hide();
        }
    }
}