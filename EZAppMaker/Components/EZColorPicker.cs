/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Runtime.CompilerServices;

using EZAppMaker.Defaults;
using EZAppMaker.Interfaces;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public class EZColorPicker : ContentView, IEZComponent
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZColorPicker), null);
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(EZColorPicker), null);
        public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(nameof(IsRequired), typeof(bool), typeof(EZColorPicker), false);

        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EZColorPicker), defaultValueCreator: bindable => Default.Color("ezcolorpicker_border"));
        public static readonly BindableProperty LabelColorProperty = BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(EZColorPicker), defaultValueCreator: bindable => Default.Color("ezcolorpicker_label"));
        public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(nameof(SelectedColor), typeof(Color), typeof(EZColorPicker), Colors.White);
        public static readonly BindableProperty ColorValueProperty = BindableProperty.Create(nameof(ColorValue), typeof(string), typeof(EZColorPicker), "#FFFFFFFF");

        public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(EZColorPicker), defaultValueCreator: bindable => Default.Color("ezcolorpicker_background"));

        private readonly Slider RSlider;
        private readonly Slider GSlider;
        private readonly Slider BSlider;
        private readonly Slider ASlider;

        private readonly Grid RGrid;
        private readonly Grid GGrid;
        private readonly Grid BGrid;
        private readonly Grid AGrid;

        private readonly Image RPointer;
        private readonly Image GPointer;
        private readonly Image BPointer;
        private readonly Image APointer;

        private readonly Border RStrip;
        private readonly Border GStrip;
        private readonly Border BStrip;
        private readonly Border AStrip;

        private readonly EZEntry ColorEntry;

        public delegate void OnChange(EZColorPicker picker);
        public event OnChange OnChanged;

        private Color initial;
        private Color state;
        private bool ignore;

        public EZColorPicker()
        {
            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZColorPickerTemplate"];

            RGrid = (Grid)GetTemplateChild("RPointerGrid");
            GGrid = (Grid)GetTemplateChild("GPointerGrid");
            BGrid = (Grid)GetTemplateChild("BPointerGrid");
            AGrid = (Grid)GetTemplateChild("APointerGrid");

            RSlider = (Slider)GetTemplateChild("RSlider");
            GSlider = (Slider)GetTemplateChild("GSlider");
            BSlider = (Slider)GetTemplateChild("BSlider");
            ASlider = (Slider)GetTemplateChild("ASlider");

            RPointer = (Image)GetTemplateChild("RPointer");
            GPointer = (Image)GetTemplateChild("GPointer");
            BPointer = (Image)GetTemplateChild("BPointer");
            APointer = (Image)GetTemplateChild("APointer");

            RStrip = (Border)GetTemplateChild("RStrip");
            GStrip = (Border)GetTemplateChild("GStrip");
            BStrip = (Border)GetTemplateChild("BStrip");
            AStrip = (Border)GetTemplateChild("AStrip");

            RStrip.Background = Gradient(Colors.Cyan, Colors.White);
            GStrip.Background = Gradient(Colors.Magenta, Colors.White);
            BStrip.Background = Gradient(Colors.Yellow, Colors.White);
            AStrip.Background = Gradient(new Color(1,1,1,0), new Color(1,1,1,1));

            RSlider.ValueChanged += Slider_ValueChanged;
            GSlider.ValueChanged += Slider_ValueChanged;
            BSlider.ValueChanged += Slider_ValueChanged;
            ASlider.ValueChanged += Slider_ValueChanged;

            RSlider.DragStarted += RSlider_DragStarted;
            GSlider.DragStarted += RSlider_DragStarted;
            BSlider.DragStarted += RSlider_DragStarted;
            ASlider.DragStarted += RSlider_DragStarted;

            RGrid.SizeChanged += GridSizeChanged;
            GGrid.SizeChanged += GridSizeChanged;
            BGrid.SizeChanged += GridSizeChanged;
            AGrid.SizeChanged += GridSizeChanged;

            ColorEntry = (EZEntry)GetTemplateChild("ColorEntry");

            SetStripsGradient();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            initial = SelectedColor;
        }

        private void GridSizeChanged(object sender, EventArgs e)
        {
            if ((RGrid.Width > 0) && (GGrid.Width > 0) && (BGrid.Width > 0) && (AGrid.Width > 0))
            {
                Slider_ValueChanged(null, null);
            }
        }

        public void ThemeChanged()
        {
            LabelColor = Default.Color("ezcolorpicker_label");
            BorderColor = Default.Color("ezcolorpicker_border");
            BackgroundColor = Default.Color("ezcolorpicker_background");

            ColorEntry.ThemeChanged();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch(propertyName)
            {
                case "Label": OnPropertyChanged(nameof(HasLabel)); break;

                case "SelectedColor":
                    {
                        Color color = (SelectedColor == null) ? Colors.White : SelectedColor;

                        ignore = true;

                        RSlider.Value = color.Red;
                        GSlider.Value = color.Green;
                        BSlider.Value = color.Blue;
                        ASlider.Value = color.Alpha;

                        ignore = false;

                        Slider_ValueChanged(null, null);

                        OnChanged?.Invoke(this);

                        break;
                    }

                case "ColorValue":
                    {
                        Color color = Colors.White;

                        try
                        {
                            color = Color.FromArgb(ColorValue);
                        }
                        catch { /* Dismiss */ }

                        SelectedColor = color;

                        break;
                    }
            }
        }

        public void StateManager(StateFormAction action)
        {
            switch (action)
            {
                case StateFormAction.Save:

                    state = SelectedColor;
                    break;

                case StateFormAction.Restore:

                    if (SelectedColor != state)
                    {
                        SelectedColor = state;
                    }
                    break;
            }
        }

        public void Clear()
        {
            SelectedColor = Colors.White;
        }

        public bool Modified()
        {
            return SelectedColor != initial;
        }

        public object ToDatabaseValue(object target)
        {
            return SelectedColor.ToArgbHex();
        }

        public bool Detached { get; set; }

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

        public bool IsRequired
        {
            get => (bool)GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

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

        public Color LabelColor
        {
            get => (Color)GetValue(LabelColorProperty);
            set => SetValue(LabelColorProperty, value);
        }

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public string ColorValue
        {
            get => (string)GetValue(ColorValueProperty);
            set => SetValue(ColorValueProperty, value);
        }

        public bool HasLabel
        {
            get => !string.IsNullOrEmpty(Label);
        }

        private void SetStripsGradient()
        {
            LinearGradientBrush lgb;

            float r = (float)RSlider.Value;
            float g = (float)GSlider.Value;
            float b = (float)BSlider.Value;

            lgb = (LinearGradientBrush)RStrip.Background;
            lgb.GradientStops[0].Color = new Color(0, g, b);
            lgb.GradientStops[1].Color = new Color(1, g, b);

            lgb = (LinearGradientBrush)GStrip.Background;
            lgb.GradientStops[0].Color = new Color(r, 0, b);
            lgb.GradientStops[1].Color = new Color(r, 1, b);

            lgb = (LinearGradientBrush)BStrip.Background;
            lgb.GradientStops[0].Color = new Color(r, g, 0);
            lgb.GradientStops[1].Color = new Color(r, g, 1);

            lgb = (LinearGradientBrush)AStrip.Background;
            lgb.GradientStops[0].Color = new Color(r, g, b, 0);
            lgb.GradientStops[1].Color = new Color(r, g, b, 1);
        }

        private LinearGradientBrush Gradient(Color start, Color end)
        {
            LinearGradientBrush lgb = new LinearGradientBrush();

            lgb.StartPoint = new Point() { X = 0D, Y = 0.5D };
            lgb.EndPoint = new Point() { X = 1D, Y = 0.5D };

            lgb.GradientStops.Add(new GradientStop() { Color = start, Offset = 0F });
            lgb.GradientStops.Add(new GradientStop() { Color = end, Offset = 1F });

            return lgb;
        }

        private void RSlider_DragStarted(object sender, EventArgs e)
        {
            EZApp.Container.HideKeyboard();
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (ignore) return;

            float r = (float)RSlider.Value;
            float g = (float)GSlider.Value;
            float b = (float)BSlider.Value;
            float a = (float)ASlider.Value;

            double length = RGrid.Width - RPointer.WidthRequest;

            RPointer.TranslationX = length * r;
            GPointer.TranslationX = length * g;
            BPointer.TranslationX = length * b;
            APointer.TranslationX = length * a;

            SetStripsGradient();

            ColorEntry.Text = SelectedColor.ToArgbHex();

            SelectedColor = new Color(r, g, b, a);
        }
    }
}