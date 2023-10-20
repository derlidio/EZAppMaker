/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Windows.Input;
using System.Runtime.CompilerServices;

using Microsoft.Maui.Controls.Shapes;

using EZAppMaker.Attributes;
using EZAppMaker.Defaults;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{

    public class EZPathButton : ContentView
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZPathButton), null);
        public static readonly BindableProperty PathDataProperty = BindableProperty.Create(nameof(PathData), typeof(GeometryGroup), typeof(EZPathButton), null);
        public static readonly BindableProperty AutoDisableProperty = BindableProperty.Create(nameof(AutoDisable), typeof(bool), typeof(EZPathButton), false);
        public static readonly BindableProperty StrokeThicknessProperty = BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(EZPathButton), 0D);

        public static readonly BindableProperty FillProperty = BindableProperty.Create(nameof(Fill), typeof(Brush), typeof(EZPathButton), defaultValueCreator: bindable => Default.Brush("ezpathbutton_fill"));
        public static readonly BindableProperty StrokeProperty = BindableProperty.Create(nameof(Stroke), typeof(Brush), typeof(EZPathButton), defaultValueCreator: bindable => Default.Brush("ezpathbutton_stroke"));

        public delegate void OnTapHandler(EZPathButton button);
        public event OnTapHandler OnTap;

        private int tapping;
        private int tick;

        public ICommand OnButtonTap { get; private set; }

        public EZPathButton()
        {
            OnButtonTap = new Command(Handle_ButtonTap);

            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZPathButtonTemplate"];
        }

        public void ThemeChanged()
        {
            Fill = Default.Brush("ezpathbutton_fill");
            Stroke =  Default.Brush("ezpathbutton_stroke");
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
            }
        }

        public double ButtonOpacity { get; private set; }

        public string ItemId
        {
            get => (string)GetValue(ItemIdProperty);
            set => SetValue(ItemIdProperty, value);
        }

        public bool AutoDisable
        {
            get => (bool)GetValue(AutoDisableProperty);
            set => SetValue(AutoDisableProperty, value);
        }

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public GeometryGroup PathData
        {
            get => (GeometryGroup)GetValue(PathDataProperty);
            set => SetValue(PathDataProperty, value);
        }

        [ComponentEventHandler]
        private void Handle_ButtonTap()
        {
            // .NET 8 IsEnabled property is broken. It does not propagate
            // the status to layout children. So... we have to workaround.

            if (!IsEnabled && EZWorkarounds.IsEnabledPropagation) return; /* WORKAROUND */

            // This event must run async. If it runs sync it may interfere with
            // the Soft Keyboard behavior on Android. We also need to protect it
            // against multiple consecutive calls (it will not run if the previous
            // call has not yet finished).

            if ((Environment.TickCount - tick) < EZSettings.MinTapInterval) return;

            if (0 == Interlocked.Exchange(ref tapping, 1))
            {
                if (AutoDisable) IsEnabled = false;

                EZApp.Container.HideKeyboard();

                OnTap?.Invoke(this);

                tick = Environment.TickCount;

                Interlocked.Exchange(ref tapping, 0);
            }            
        }
    }
}