using System.Runtime.CompilerServices;

using EZAppMaker.Interfaces;
using EZAppMaker.Defaults;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public enum EZSliderPointer
    {
        round, square
    }

    public class EZSlider : ContentView, IEZComponent
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZSlider), null);
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(EZSlider), null);
        public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(nameof(IsRequired), typeof(bool), typeof(EZSlider), false);
        public static readonly BindableProperty MinProperty = BindableProperty.Create(nameof(Min), typeof(double), typeof(EZSlider), 0D);
        public static readonly BindableProperty MaxProperty = BindableProperty.Create(nameof(Max), typeof(double), typeof(EZSlider), 1D);
        public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(double), typeof(EZSlider), 0D, BindingMode.TwoWay);
        public static readonly BindableProperty DecimalsProperty = BindableProperty.Create(nameof(Decimals), typeof(int), typeof(EZSlider), 0);

        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EZSlider), defaultValueCreator: bindable => Default.Color("ezslider_border"));
        public static readonly BindableProperty LabelColorProperty = BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(EZSlider), defaultValueCreator: bindable => Default.Color("ezslider_label"));
        public static readonly BindableProperty MinimumTrackColorProperty = BindableProperty.Create(nameof(MinimumTrackColor), typeof(Color), typeof(EZSlider), defaultValueCreator: bindable => Default.Color("ezslider_min_track"));
        public static readonly BindableProperty MaximumTrackColorProperty = BindableProperty.Create(nameof(MaximumTrackColor), typeof(Color), typeof(EZSlider), defaultValueCreator: bindable => Default.Color("ezslider_max_track"));
        public static readonly BindableProperty PointerShapeProperty = BindableProperty.Create(nameof(PointerShape), typeof(EZSliderPointer), typeof(EZSlider), EZSliderPointer.round);

        public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(EZSlider), defaultValueCreator: bindable => Default.Color("ezslider_background"));

        private readonly Slider slider;
        private readonly EZEntry entry;
        private readonly Grid grid;
        private readonly BoxView track_min;
        private readonly BoxView track_max;
        private readonly Image pointer;

        private double initial;
        private double state;

        public delegate void OnChange(EZSlider slider);
        public event OnChange OnChanged;

        public delegate void OnDragComplete(EZSlider slider);
        public event OnDragComplete OnDragCompleted;

        public EZSlider()
        {
            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZSliderTemplate"];

            slider = (Slider)GetTemplateChild("InternalSlider");
            entry = (EZEntry)GetTemplateChild("SliderEntry");
            grid = (Grid)GetTemplateChild("PointerGrid");
            track_min = (BoxView)GetTemplateChild("SliderTrackMin");
            track_max = (BoxView)GetTemplateChild("SliderTrackMax");
            pointer = (Image)GetTemplateChild("SliderPointer");

            slider.ValueChanged += Slider_ValueChanged;
            slider.DragCompleted += Slider_DragCompleted;
            slider.DragStarted += Slider_DragStarted;
            entry.Unfocused += Entry_Unfocused;

            track_min.SizeChanged += Track_SizeChanged;
        }

        private void Track_SizeChanged(object sender, EventArgs e)
        {
            track_max.WidthRequest = track_min.Width * slider.Value + 1; // + 1 -> iOS WORKAROUND
            pointer.TranslationX = (grid.Width - pointer.WidthRequest) * slider.Value;
        }

        public void ThemeChanged()
        {
            LabelColor = Default.Color("ezslider_label");
            BorderColor = Default.Color("ezslider_border");
            BackgroundColor = Default.Color("ezslider_background");

            entry.ThemeChanged();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            initial = Value;
            
            SetSliderValue();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch(propertyName)
            {
                case "Label": OnPropertyChanged(nameof(HasLabel)); break;

                case "PointerShape": OnPropertyChanged(nameof(PointerImage)); break;

                case "Value":
                    
                    SetSliderValue();
                    OnChanged?.Invoke(this);
                    break;
            }
        }

        public void StateManager(StateFormAction action)
        {
            switch (action)
            {
                case StateFormAction.Save:

                    state = Value;
                    break;

                case StateFormAction.Restore:

                    if (Value != state)
                    {
                        Value = state;
                    }
                    break;
            }
        }

        public void Clear()
        {
            Value = 0D;
        }

        public bool Modified()
        {
            return Value != initial;
        }

        public object ToDatabaseValue(object target)
        {
            object result = null;

            Type type = target?.GetType();

            if (type == typeof(string))
            {
                if (!string.IsNullOrWhiteSpace(entry.Text))
                {
                    result = entry.Text;
                }
            }

            else

            if (type == typeof(int))
            {
                result = int.TryParse(entry.Text, out int int_value) ? int_value : target;
            }

            else

            if (type == typeof(long))
            {
                result = long.TryParse(entry.Text, out long long_value) ? long_value : target;
            }

            else

            if (type == typeof(double))
            {
                result = double.TryParse(entry.Text, out double double_value) ? double_value : target;
            }

            return result;
        }

        public bool Detached { get; set; }

        public ImageSource PointerImage
        {
            get
            {
                string asset = $"slider_{(PointerShape == EZSliderPointer.round ? "a" : "b")}.png";

                return EZEmbedded.GetImage($"EZAppMaker.Assets.Images.{asset}");
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

        public bool IsRequired
        {
            get => (bool)GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

        public double Min
        {
            get => (double)GetValue(MinProperty);
            set => SetValue(MinProperty, value);
        }

        public double Max
        {
            get => (double)GetValue(MaxProperty);
            set => SetValue(MaxProperty, value);
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int Decimals
        {
            get => (int)GetValue(DecimalsProperty);
            set => SetValue(DecimalsProperty, value);
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

        public Color MinimumTrackColor
        {
            get => (Color)GetValue(MinimumTrackColorProperty);
            set => SetValue(MinimumTrackColorProperty, value);
        }

        public Color MaximumTrackColor
        {
            get => (Color)GetValue(MaximumTrackColorProperty);
            set => SetValue(MaximumTrackColorProperty, value);
        }

        public EZSliderPointer PointerShape
        {
            get => (EZSliderPointer)GetValue(PointerShapeProperty);
            set => SetValue(PointerShapeProperty, value);
        }

        public bool HasLabel
        {
            get => !string.IsNullOrEmpty(Label);
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            double range = Max - Min;

            Value = Min + range * slider.Value;

            // We need to add 1 to the track_max BoxView WidthRequest,
            // otherwise the line will not render on iOS. It "seems"
            // that when we set WidthRequest to zero, the BoxView
            // is somehow removed from the visual tree.

            track_max.WidthRequest = track_min.Width * slider.Value + 1; // + 1 -> iOS WORKAROUND
            pointer.TranslationX = (grid.Width - pointer.Width) * slider.Value;

            entry.Text = Value.ToString($"F{Decimals}");
        }

        private void Slider_DragStarted(object sender, EventArgs e)
        {
            EZApp.Container.HideKeyboard();
        }

        private void Slider_DragCompleted(object sender, EventArgs e)
        {
            OnDragCompleted?.Invoke(this);
        }

        private void Entry_Unfocused(EZEntry entry)
        {
            string text = Value.ToString($"F{Decimals}");

            if (entry.Text != text)
            {
                double slider_value = 0;

                if (double.TryParse(entry.Text, out double entry_value))
                {
                    if (entry_value < Min) entry_value = Min;
                    else
                    if (entry_value > Max) entry_value = Max;
                    
                    double range = Max - Min;
                    double value = entry_value - Min;

                    if (range != 0)
                    {
                        slider_value = value / range;
                    }
                }

                slider.Value = slider_value;
            }
        }

        private void SetSliderValue()
        {
            double range = Math.Abs(Max - Min);
            double value = Value;

            if (Min <= Max)
            {
                if (value < Min) value = Min;
                else
                if (value > Max) value = Max;
            }
            else
            {
                if (value < Max) value = Max;
                else
                if (value > Min) value = Min;
            }

            value -= Min;

            if (range != 0)
            {
                slider.Value = Math.Abs(value / range);
            }
        }
    }
}