using EZAppMaker.Defaults;

namespace EZAppMaker.Components
{
	public class EZHoverMessage : ContentView
	{
        public static readonly BindableProperty BalloonColorProperty = BindableProperty.Create(nameof(BackColor), typeof(Color), typeof(EZHoverMessage), defaultValueCreator: bindable => Default.Color("ezhovermsg_background"));
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Brush), typeof(EZHoverMessage), defaultValueCreator: bindable => Default.Brush("ezhovermsg_border"));
        public static readonly BindableProperty ShadowColorProperty = BindableProperty.Create(nameof(ShadowColor), typeof(Brush), typeof(EZHoverMessage), defaultValueCreator: bindable => Default.Brush("ezhovermsg_shadow"));
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(EZHoverMessage), defaultValueCreator: bindable => Default.Color("ezhovermsg_text"));
        public static readonly BindableProperty MessageProperty = BindableProperty.Create(nameof(Message), typeof(string), typeof(EZHoverMessage), string.Empty);
        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(EZHoverMessage), 0D);

        public EZHoverMessage()
		{
            BindingContext = this;
		}

        public Color BackColor
        {
            get => (Color)GetValue(BalloonColorProperty);
            set => SetValue(BalloonColorProperty, value);
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

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public double CornerRadius
        {
            get => (double)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}