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

using EZAppMaker.Attributes;
using EZAppMaker.Defaults;
using EZAppMaker.Interfaces;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public class EZRating : ContentView, IEZComponent
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZRating), null);
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(EZSlider), null);
        public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(nameof(IsRequired), typeof(bool), typeof(EZSlider), false);
        public static readonly BindableProperty RatingProperty = BindableProperty.Create(nameof(Rating), typeof(int), typeof(EZRating), 0, BindingMode.TwoWay);
        public static readonly BindableProperty StrokeThicknessProperty = BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(EZRating), 1D);

        public static readonly BindableProperty FillProperty = BindableProperty.Create(nameof(Fill), typeof(Brush), typeof(EZRating), defaultValueCreator: bindable => Default.Brush("ezrating_fill"));
        public static readonly BindableProperty StrokeProperty = BindableProperty.Create(nameof(Stroke), typeof(Brush), typeof(EZRating), defaultValueCreator: bindable => Default.Brush("ezrating_stroke"));
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EZSlider), defaultValueCreator: bindable => Default.Color("ezslider_border"));
        public static readonly BindableProperty LabelColorProperty = BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(EZSlider), defaultValueCreator: bindable => Default.Color("ezslider_label"));

        public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(EZSlider), defaultValueCreator: bindable => Default.Color("ezslider_background"));

        private int initial;
        private int state;

        private readonly Grid Container;

        public delegate void OnTapHandler(EZRating rating);
        public event OnTapHandler OnStarTap;

        public delegate void OnChange(EZRating rating);
        public event OnChange OnChanged;

        public ICommand StarTap { get; private set; }

        public EZRating()
        {
            StarTap = new Command(Handle_StarTap);

            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZRatingTemplate"];

            Container = (Grid)GetTemplateChild("Container");

            OnPropertyChanged(nameof(Rating));
        }

        public void ThemeChanged()
        {
            LabelColor = Default.Color("ezrating_label");
            BorderColor = Default.Color("ezrating_border");
            BackgroundColor = Default.Color("ezrating_background");
            Fill = Default.Brush("ezrating_fill");
            Stroke = Default.Brush("ezrating_stroke");
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            initial = Rating;

            FillStars();
        }

        public void Clear()
        {
            Rating = 0;
        }

        public bool Modified()
        {
            return Rating != initial;
        }

        public object ToDatabaseValue(object target)
        {
            object result = null;

            Type type = target?.GetType();

            if (type == typeof(string))
            {
                result = Rating.ToString();
            }

            else

            if (type == typeof(int))
            {
                result = Rating;
            }

            else

            if (type == typeof(long))
            {
                result = Convert.ToInt64(Rating);
            }

            else

            if (type == typeof(double))
            {
                result = Convert.ToDouble(Rating);
            }

            return result;
        }

        public void StateManager(StateFormAction action)
        {
            switch (action)
            {
                case StateFormAction.Save:

                    state = Rating;
                    break;

                case StateFormAction.Restore:

                    if (Rating != state)
                    {
                        Rating = state;
                    }
                    break;
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case "Label": OnPropertyChanged(nameof(HasLabel)); break;

                case "Rating":

                    FillStars();

                    OnChanged?.Invoke(this);

                    break;
            }
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

        public int Rating
        {
            get => (int)GetValue(RatingProperty);
            set => SetValue(RatingProperty, value);
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

        public bool HasLabel
        {
            get => !string.IsNullOrEmpty(Label);
        }

        private void FillStars()
        {
            foreach (Element child in EZXamarin.GetChildren<Element>(Container))
            {
                if (Grid.GetColumn(child) > (Rating - 1))
                {
                    ((Microsoft.Maui.Controls.Shapes.Path)child).Fill = new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    ((Microsoft.Maui.Controls.Shapes.Path)child).Fill = Fill;
                }
            }
        }

        [ComponentEventHandler]
        private void Handle_StarTap(object parameter)
        {
            // .NET 8 IsEnabled property is broken. It does not propagate
            // the status to layout children. So... we have to workaround.

            if (!IsEnabled && EZWorkarounds.IsEnabledPropagation) return; /* WORKAROUND */

            Rating = int.Parse((string)parameter);

            EZApp.Container.HideKeyboard();

            OnStarTap?.Invoke(this);
        }
    }
}