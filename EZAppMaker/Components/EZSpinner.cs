using System.Runtime.CompilerServices;

using EZAppMaker.Defaults;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
	public class EZSpinner : ContentView
	{
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZSpinner), null);
        public static readonly BindableProperty SizeProperty = BindableProperty.Create(nameof(Size), typeof(int), typeof(EZSpinner), 32);
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Brush), typeof(EZSpinner), defaultValueCreator: bindable => Default.Brush("ezspinner"));
        public static readonly BindableProperty IsSpinningProperty = BindableProperty.Create(nameof(IsSpinning), typeof(bool), typeof(EZSpinner), false);

        private bool animating = false;

        private readonly Microsoft.Maui.Controls.Shapes.Path spinner;

        public EZSpinner()
		{
            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZSpinnerTemplate"];

            spinner = (Microsoft.Maui.Controls.Shapes.Path)GetTemplateChild("Spinner");
        }

        public string ItemId
        {
            get => (string)GetValue(ItemIdProperty);
            set => SetValue(ItemIdProperty, value);
        }

        public int Size
        {
            get => (int)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public Brush Color
        {
            get => (Brush)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public bool IsSpinning
        {
            get => (bool)GetValue(IsSpinningProperty);
            
            set
            {
                SetValue(IsSpinningProperty, value);

                if (value)
                {
                    StartAnimation();
                }
            }
        }

        //private void StartAnimation()
        //{
        //    if ((spinner == null) || animating) return;

        //    System.Diagnostics.Debug.WriteLine("EZSpinner: starging animation");

        //    animating = true;

        //    var animation = new Animation(v => spinner.Rotation = v, 360, 0);

        //    animation.Commit
        //    (
        //        this,
        //        "Spin",
        //        40,
        //        1000,
        //        Easing.Linear,
        //        (v, c) =>
        //        {
        //            spinner.Rotation = 360;

        //            if (!IsSpinning)
        //            {
        //                animating = false;
        //                System.Diagnostics.Debug.WriteLine("EZSpinner: ending animation");
        //            }
        //        },
        //        () => IsSpinning
        //    );
        //}

        private void StartAnimation()
        {
            if ((spinner == null) || animating) return;

            System.Diagnostics.Debug.WriteLine("EZSpinner: starging animation");

            animating = true;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var animation = new Animation(v => spinner.Rotation = v, 360, 0);

                animation.Commit(
                    this,
                    "Spin",
                    40,
                    1000,
                    Easing.Linear,
                    (v, c) =>
                    {
                        spinner.Rotation = 360;
                        if (!IsSpinning)
                        {
                            animating = false;
                            System.Diagnostics.Debug.WriteLine("EZSpinner: ending animation");
                        }
                    },
                    () => IsSpinning
                );
            });
        }
    }
}