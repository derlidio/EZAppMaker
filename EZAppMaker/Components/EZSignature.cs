/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using Microsoft.Maui.Controls.Shapes;

using EZAppMaker.Attributes;
using EZAppMaker.Defaults;
using EZAppMaker.Effects;
using EZAppMaker.Support;
using EZAppMaker.Interfaces;

namespace EZAppMaker.Components
{
    public class EZSignature : ContentView, IEZComponent
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZSignature), null);
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(EZSignature), null);
        public static readonly BindableProperty DataProperty = BindableProperty.Create(nameof(Data), typeof(string), typeof(EZSignature), null, BindingMode.TwoWay);
        public static readonly BindableProperty StrokeThicknessProperty = BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(EZSignature), 2D);

        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EZSignature), defaultValueCreator: bindable => Default.Color("ezsignature_border"));
        public static readonly BindableProperty LabelColorProperty = BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(EZSignature), defaultValueCreator: bindable => Default.Color("ezsignature_label"));
        public static readonly BindableProperty StrokeProperty = BindableProperty.Create(nameof(Stroke), typeof(Brush), typeof(EZSignature), defaultValueCreator: bindable => Default.Brush("ezsignature_stroke"));

        public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(EZSignature), defaultValueCreator: bindable => Default.Color("ezsignature_background"));

        public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(nameof(IsRequired), typeof(bool), typeof(EZSignature), false);
        public static readonly BindableProperty RequiredAlertProperty = BindableProperty.Create(nameof(RequiredAlert), typeof(string), typeof(EZSignature), null);

        private string initial = null;
        private string state = null;

        private readonly ContentView canvas;
        private readonly Microsoft.Maui.Controls.Shapes.Path path;

        private readonly EZPathButton penOn;
        private readonly EZPathButton penOff;

        public delegate void OnChange(EZSignature signature);
        public event OnChange OnChanged;

        public delegate bool OnValidateHandler(EZSignature signature);
        public event OnValidateHandler OnValidate;

        public EZSignature()
        {
            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZSignatureTemplate"];

            TouchRoutingEffect effect = new TouchRoutingEffect() { Capture = true };
            effect.TouchAction += OnTouchEffectAction;

            canvas = (ContentView)GetTemplateChild("EZSignatureCanvas");
            canvas.Effects.Add(effect);

            EZPathButton button = (EZPathButton)GetTemplateChild("ClearButton");
            button.OnTap += OnClearButtonTap;

            penOn = (EZPathButton)GetTemplateChild("PenButtonOn");
            penOn.OnTap += OnEditButtonTap;

            penOff = (EZPathButton)GetTemplateChild("PenButtonOff");
            penOff.OnTap += OnEditButtonTap;

            path = (Microsoft.Maui.Controls.Shapes.Path)GetTemplateChild("EZSignaturePath");
        }

        public void ThemeChanged()
        {
            LabelColor = Default.Color("ezsignature_label");
            BorderColor = Default.Color("ezsignature_border");
            BackgroundColor = Default.Color("ezsignature_background");
            Stroke = Default.Brush("ezsignature_stroke");

            EZPathButton button;

            button = (EZPathButton)GetTemplateChild("PenButtonOn"); button.ThemeChanged();
            button = (EZPathButton)GetTemplateChild("PenButtonOff"); button.ThemeChanged();
            button = (EZPathButton)GetTemplateChild("ClearButton"); button.ThemeChanged();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            try
            {
                // Try to convert Data property to a Geometry object.
                // If successful, pass it to the pen. If not, discard.
                Geometry geometry = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(Data);
                path.Data = geometry;
            }
            catch
            {
                Data = null;
                System.Diagnostics.Debug.WriteLine($"Invalid Path Data for Signature: {ItemId}");
            }

            initial = Data;
        }

        public void StateManager(StateFormAction action)
        {
            switch (action)
            {
                case StateFormAction.Save:

                    state = Data;
                    break;

                case StateFormAction.Restore:

                    if (Data != state)
                    {
                        Data = state;
                    }
                    break;
            }
        }

        public void Refresh()
        {
            try
            {
                path.Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(Data);
            }
            catch { /* Dismiss */ }
        }

        public void Clear()
        {
            Data = null;
            path.Data = null;            
            OnChanged?.Invoke(this);
        }

        public bool Modified()
        {
            return Data != initial;
        }

        public object ToDatabaseValue(object target)
        {
            return Data;
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

        public string Data
        {
            get => (string)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
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

        public bool IsRequired
        {
            get => (bool)GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

        public string RequiredAlert
        {
            get => (string)GetValue(RequiredAlertProperty);
            set => SetValue(RequiredAlertProperty, value);
        }

        public bool Validate()
        {
            bool valid = true;

            if (OnValidate != null)
            {
                valid = OnValidate.Invoke(this);
            }

            return valid;
        }

        [ComponentEventHandler]
        private void OnClearButtonTap(EZPathButton button)
        {
            Clear();
        }

        [ComponentEventHandler]
        private void OnEditButtonTap(EZPathButton button)
        {
            switch (button.ItemId)
            {
                case "PenOn":
                    
                    canvas.IsEnabled = true;
                    penOn.IsVisible = false;
                    penOff.IsVisible = true;
                    break;

                case "PenOff":

                    canvas.IsEnabled = false;
                    penOn.IsVisible = true;
                    penOff.IsVisible = false;
                    break;
            }
        }

        private void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            if (!canvas.IsEnabled) return;

            int x = args.Location.X;
            int y = args.Location.Y;

            switch (args.Type)
            {
                case TouchActionType.Entered:
                    break;

                case TouchActionType.Exited:
                    break;

                case TouchActionType.Pressed:
                    EZApp.Container.DisableScrolling();
                    Data = $"{Data}M{x},{y}";
                    break;

                case TouchActionType.Moved:
                    Data = $"{Data}L{x},{y}";
                    break;

                case TouchActionType.Released:
                    EZApp.Container.EnableScrolling();
                    OnChanged?.Invoke(this);
                    GC.Collect();
                    break;

                case TouchActionType.Cancelled:
                    EZApp.Container.EnableScrolling();
                    OnChanged?.Invoke(this);
                    GC.Collect();
                    break;
            }

            path.Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(Data);
        }
    }
}