/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Runtime.CompilerServices;
using System.Windows.Input;

using Microsoft.Maui.Controls.Shapes;

using EZAppMaker.Attributes;
using EZAppMaker.Support;
using EZAppMaker.Defaults;

namespace EZAppMaker.Components
{
    public enum EZButtonType
    {
        primary,
        secondary,
        success,
        danger,
        warning,
        info,
        light,
        dark,
        link
    }

    public class EZButton : ContentView
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZButton), null);
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(EZButton), null);
        public static readonly BindableProperty TypeProperty = BindableProperty.Create(nameof(Type), typeof(EZButtonType), typeof(EZButton), EZButtonType.secondary);
        public static readonly BindableProperty PathDataProperty = BindableProperty.Create(nameof(PathData), typeof(GeometryGroup), typeof(EZButton), null);
        public static readonly BindableProperty AutoDisableProperty = BindableProperty.Create(nameof(AutoDisable), typeof(bool), typeof(EZButton), false);

        public delegate void OnTapHandler(EZButton button);
        public event OnTapHandler OnTap;

        private int tapping;
        private int tick;

        public ICommand OnButtonTap { get; private set; }

        public EZButton()
        {
            OnButtonTap = new Command(Handle_ButtonTap);

            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZButtonTemplate"];
        }

        public void ThemeChanged()
        {
            OnPropertyChanged(nameof(ButtonColor));
            OnPropertyChanged(nameof(TappedColor));
            OnPropertyChanged(nameof(LabelColor));
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case "IsEnabled":
                    {
                        Opacity = IsEnabled ? 1.0D : 0.5D;
                        break;
                    }
                case "Type":
                    {
                        OnPropertyChanged(nameof(ButtonColor));
                        OnPropertyChanged(nameof(LabelColor));
                        OnPropertyChanged(nameof(TappedColor));
                        break;
                    }
                case "PathData":
                    {
                        OnPropertyChanged(nameof(HasPath));
                        break;
                    }
            }
        }

        public string ItemId
        {
            get => (string)GetValue(ItemIdProperty);
            set => SetValue(ItemIdProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public EZButtonType Type
        {
            get => (EZButtonType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public GeometryGroup PathData
        {
            get => (GeometryGroup)GetValue(PathDataProperty);
            set => SetValue(PathDataProperty, value);
        }

        public bool AutoDisable
        {
            get => (bool)GetValue(AutoDisableProperty);
            set => SetValue(AutoDisableProperty, value);
        }

        public Brush PathFill
        {
            get
            {
                return new SolidColorBrush(LabelColor);
            }
        }

        public bool HasPath
        {
            get => (GetValue(PathDataProperty) != null);
        }

        public Color ButtonColor
        {
            get
            {
                Color c = Colors.Gray;

                switch (Type)
                {
                    case EZButtonType.primary: { c = Default.Color("ezbutton_primary"); break; }
                    case EZButtonType.secondary: { c = Default.Color("ezbutton_secondary"); break; }
                    case EZButtonType.success: { c = Default.Color("ezbutton_success"); break; }
                    case EZButtonType.danger: { c = Default.Color("ezbutton_danger"); break; }
                    case EZButtonType.warning: { c = Default.Color("ezbutton_warning"); break; }
                    case EZButtonType.info: { c = Default.Color("ezbutton_info"); break; }
                    case EZButtonType.light: { c = Default.Color("ezbutton_light"); break; }
                    case EZButtonType.dark: { c = Default.Color("ezbutton_dark"); break; }
                    case EZButtonType.link: { c = Default.Color("ezbutton_link"); break; }
                }

                return c;
            }
        }

        public Color TappedColor
        {
            get
            {
                Color c = Colors.Gray;

                switch (Type)
                {
                    case EZButtonType.primary: { c = Default.Color("ezbutton_primary_tapped"); break; }
                    case EZButtonType.secondary: { c = Default.Color("ezbutton_secondary_tapped"); break; }
                    case EZButtonType.success: { c = Default.Color("ezbutton_success_tapped"); break; }
                    case EZButtonType.danger: { c = Default.Color("ezbutton_danger_tapped"); break; }
                    case EZButtonType.warning: { c = Default.Color("ezbutton_warning_tapped"); break; }
                    case EZButtonType.info: { c = Default.Color("ezbutton_info_tapped"); break; }
                    case EZButtonType.light: { c = Default.Color("ezbutton_light_tapped"); break; }
                    case EZButtonType.dark: { c = Default.Color("ezbutton_dark_tapped"); break; }
                    case EZButtonType.link: { c = Default.Color("ezbutton_link_tapped"); break; }
                }

                return c;
            }
        }

        public Color LabelColor
        {
            get
            {
                EZButtonType t = (EZButtonType)GetValue(TypeProperty);

                Color c = Colors.White;

                switch (t)
                {
                    case EZButtonType.primary: { c = Default.Color("ezbutton_primary_label"); break; }
                    case EZButtonType.secondary: { c = Default.Color("ezbutton_secondary_label"); break; }
                    case EZButtonType.success: { c = Default.Color("ezbutton_success_label"); break; }
                    case EZButtonType.danger: { c = Default.Color("ezbutton_danger_label"); break; }
                    case EZButtonType.warning: { c = Default.Color("ezbutton_warning_label"); break; }
                    case EZButtonType.info: { c = Default.Color("ezbutton_info_label"); break; }
                    case EZButtonType.light: { c = Default.Color("ezbutton_light_label"); break; }
                    case EZButtonType.dark: { c = Default.Color("ezbutton_dark_label"); break; }
                    case EZButtonType.link: { c = Default.Color("ezbutton_link_label"); break; }
                }

                return c;
            }
        }

        [ComponentEventHandler, AsyncVoidOnPurpose]
        private void Handle_ButtonTap()
        {
            // .NET 8 IsEnabled property is broken. It does not propagate
            // the status to layout children. So... we have to workaround.

            if (!IsEnabled && EZWorkarounds.IsEnabledPropagation) return; /* WORKAROUND */

            // Protect against multiple taps (per tap interval):

            if ((Environment.TickCount - tick) < EZSettings.MinTapInterval) return;

            // Protect against multiple taps (while previous call is not finished):

            if (0 == Interlocked.Exchange(ref tapping, 1))
            {
                if (AutoDisable) IsEnabled = false;

                var ti = GetTemplateChild("TapIndicator");

                if (ti != null)
                {
                    BoxView box = (BoxView)ti;
                    var animation = new Animation(v => box.Opacity = v, 0, 1);
                    animation.Commit(this, "TapAnimation", 10, 200, Easing.Linear, (v, c) => box.Opacity = 0, () => false);
                }

                EZApp.Container.HideKeyboard();

                OnTap?.Invoke(this);

                tick = Environment.TickCount;

                Interlocked.Exchange(ref tapping, 0);
            }            
        }
    }
}