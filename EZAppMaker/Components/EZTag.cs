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
    public class EZTag : ContentView
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZTag), null);
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(EZTag), null);
        public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(EZTag), TextAlignment.Start);

        public static readonly BindableProperty TagHeightProperty = BindableProperty.Create(nameof(TagHeight), typeof(double), typeof(EZTag), 25D);
        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(EZTag), 5D);
        public static readonly BindableProperty HasShadowProperty = BindableProperty.Create(nameof(HasShadow), typeof(bool), typeof(EZTag), true);

        public static readonly BindableProperty FillProperty = BindableProperty.Create(nameof(Fill), typeof(Brush), typeof(EZTag), defaultValueCreator: bindable => Default.Brush("eztag_fill"));
        public static readonly BindableProperty ShadowColorProperty = BindableProperty.Create(nameof(ShadowColor), typeof(Brush), typeof(EZTag), defaultValueCreator: bindable => Default.Brush("eztag_shadow"));
        public static readonly BindableProperty LabelColorProperty = BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(EZTag), defaultValueCreator: bindable => Default.Color("eztag_label"));

        public EZTag()
        {
            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZTagTemplate"];
        }

        public void ThemeChanged()
        {
            Fill = Default.Brush("eztag_fill");
            ShadowColor = Default.Brush("eztag_shadow");
            LabelColor = Default.Color("eztag_label");
        }

        public string ItemId
        {
            get => (string)GetValue(ItemIdProperty);
            set => SetValue(ItemIdProperty, value);
        }

        public double TagHeight
        {
            get => (double)GetValue(TagHeightProperty);
            set => SetValue(TagHeightProperty, value);
        }

        public double TotalHeight
        {
            get
            {
                return TagHeight + (HasShadow ? 5 : 0);
            }
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public Brush ShadowColor
        {
            get => (Brush)GetValue(ShadowColorProperty);
            set => SetValue(ShadowColorProperty, value);
        }

        public Color LabelColor
        {
            get => (Color)GetValue(LabelColorProperty);
            set => SetValue(LabelColorProperty, value);
        }

        public TextAlignment HorizontalTextAlignment
        {
            get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
            set => SetValue(HorizontalTextAlignmentProperty, value);
        }

        public double CornerRadius
        {
            get => (double)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public bool HasShadow
        {
            get => (bool)GetValue(HasShadowProperty);
            set => SetValue(HasShadowProperty, value);
        }
    }
}