/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using EZAppMaker.Defaults;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public class EZFrame : ContentView
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZFrame), null);
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(EZFrame), null);
        public static readonly BindableProperty ContentPaddingProperty = BindableProperty.Create(nameof(ContentPadding), typeof(Thickness), typeof(EZFrame), new Thickness(10, 20, 10, 10));
        public static readonly BindableProperty FrameOpacityProperty = BindableProperty.Create(nameof(FrameOpacity), typeof(double), typeof(EZFrame), 1D);
        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float), typeof(EZFrame), 5F);
        public static readonly BindableProperty TagCornerRadiusProperty = BindableProperty.Create(nameof(TagCornerRadius), typeof(double), typeof(EZFrame), 5D);
        public static readonly BindableProperty TagHorizontalTextAlignmentProperty = BindableProperty.Create(nameof(TagHorizontalTextAlignment), typeof(TextAlignment), typeof(EZFrame), TextAlignment.Start);

        public static readonly BindableProperty HasShadowProperty = BindableProperty.Create(nameof(HasShadow), typeof(bool), typeof(EZFrame), false);
        public static readonly BindableProperty HasTagShadowProperty = BindableProperty.Create(nameof(HasTagShadow), typeof(bool), typeof(EZFrame), true);

        public static readonly BindableProperty FillProperty = BindableProperty.Create(nameof(Fill), typeof(Brush), typeof(EZFrame), defaultValueCreator: bindable => Default.Brush("ezframe_fill"));
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Brush), typeof(EZFrame), defaultValueCreator: bindable => Default.Brush("ezframe_border"));
        public static readonly BindableProperty TagFillProperty = BindableProperty.Create(nameof(TagFill), typeof(Brush), typeof(EZFrame), defaultValueCreator: bindable => Default.Brush("ezframe_tag_fill"));
        public static readonly BindableProperty TagShadowProperty = BindableProperty.Create(nameof(TagShadow), typeof(Brush), typeof(EZFrame), defaultValueCreator: bindable => Default.Brush("ezframe_tag_shadow"));
        public static readonly BindableProperty TagLabelColorProperty = BindableProperty.Create(nameof(TagLabelColor), typeof(Color), typeof(EZFrame), defaultValueCreator: bindable => Default.Color("ezframe_label"));

        public EZFrame()
        {
            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZFrameTemplate"];
        }

        public void ThemeChanged()
        {
            Fill = Default.Brush("ezframe_fill");
            BorderColor = Default.Color("ezframe_border");
            TagFill = Default.Brush("ezframe_tag_fill");
            TagShadow = Default.Brush("ezframe_tag_shadow");
            TagLabelColor =  Default.Color("ezframe_label");
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

        public bool HasShadow
        {
            get => (bool)GetValue(HasShadowProperty);
            set => SetValue(HasShadowProperty, value);
        }

        public bool HasTagShadow
        {
            get => (bool)GetValue(HasTagShadowProperty);
            set => SetValue(HasTagShadowProperty, value);
        }

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public Brush TagFill
        {
            get => (Brush)GetValue(TagFillProperty);
            set => SetValue(TagFillProperty, value);
        }

        public Brush BorderColor
        {
            get => (Brush)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public Brush TagShadow
        {
            get => (Brush)GetValue(TagShadowProperty);
            set => SetValue(TagShadowProperty, value);
        }

        public Color TagLabelColor
        {
            get => (Color)GetValue(TagLabelColorProperty);
            set => SetValue(TagLabelColorProperty, value);
        }

        public Thickness ContentPadding
        {
            get => (Thickness)GetValue(ContentPaddingProperty);
            set => SetValue(ContentPaddingProperty, value);
        }

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public double TagCornerRadius
        {
            get => (double)GetValue(TagCornerRadiusProperty);
            set => SetValue(TagCornerRadiusProperty, value);
        }

        public double FrameOpacity
        {
            get => (double)GetValue(FrameOpacityProperty);
            set => SetValue(FrameOpacityProperty, value);
        }

        public TextAlignment TagHorizontalTextAlignment
        {
            get => (TextAlignment)GetValue(TagHorizontalTextAlignmentProperty);
            set => SetValue(TagHorizontalTextAlignmentProperty, value);
        }
    }
}