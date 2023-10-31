/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Windows.Input;

using Microsoft.Maui.Controls.Shapes;

using EZAppMaker.Attributes;
using EZAppMaker.Defaults;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public enum EZMenuAlignment
    {
        Top,
        Center,
        Bottom
    }

    public enum EZMenuSlide
    {
        Top,
        Bottom,
        Left,
        Right
    }

    public class EZMenuItem
    {
        public GeometryGroup Icon { get; set; } = null;
        public string ItemId { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool Visible { get; set; } = true;
    }

    public class EZMenu : ContentView
    {
        public static new readonly BindableProperty MarginProperty = BindableProperty.Create(nameof(Margin), typeof(Thickness), typeof(EZMenu), new Thickness(0D));
        public static new readonly BindableProperty PaddingProperty = BindableProperty.Create(nameof(Padding), typeof(Thickness), typeof(EZMenu), new Thickness(0D));

        public static readonly BindableProperty HasShadowProperty = BindableProperty.Create(nameof(HasShadow), typeof(bool), typeof(EZMenu), false);
        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(nameof(CornerRadius), typeof(float), typeof(EZMenu), 0F);
        public static readonly BindableProperty ItemsPaddingProperty = BindableProperty.Create(nameof(ItemsPadding), typeof(Thickness), typeof(EZMenu), new Thickness(5D,0D,0D,0D));
        public static readonly BindableProperty ItemsSpacingProperty = BindableProperty.Create(nameof(ItemsSpacing), typeof(double), typeof(EZMenu), 1D);
        public static readonly BindableProperty AlignmentProperty = BindableProperty.Create(nameof(Alignment), typeof(EZMenuAlignment), typeof(EZMenu), EZMenuAlignment.Top);
        public static readonly BindableProperty SlideFromProperty = BindableProperty.Create(nameof(SlideFrom), typeof(EZMenuSlide), typeof(EZMenu), EZMenuSlide.Left);
        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(List<EZMenuItem>), typeof(EZMenu), null);

        public static readonly BindableProperty IconFillProperty = BindableProperty.Create(nameof(IconFill), typeof(Brush), typeof(EZMenu), defaultValueCreator: bindable => Default.Brush("ezmenu_iconfill"));
        public static readonly BindableProperty IconStrokeProperty = BindableProperty.Create(nameof(IconStroke), typeof(Brush), typeof(EZMenu), defaultValueCreator: bindable => Default.Brush("ezmenu_iconstroke"));
        public static readonly BindableProperty IconStrokeThicknessProperty = BindableProperty.Create(nameof(IconStrokeThickness), typeof(double), typeof(EZMenu), 0D);

        public static readonly BindableProperty FillProperty = BindableProperty.Create(nameof(IconFill), typeof(Brush), typeof(EZMenu), defaultValueCreator: bindable => Default.Brush("ezmenu_fill"));
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EZMenu), defaultValueCreator: bindable => Default.Color("ezmenu_border"));
        public static readonly BindableProperty ItemsBackgroundColorProperty = BindableProperty.Create(nameof(ItemsBackgroundColor), typeof(Color), typeof(EZMenu), defaultValueCreator: bindable => Default.Color("ezmenu_items_background"));
        public static readonly BindableProperty ItemsColorProperty = BindableProperty.Create(nameof(ItemsColor), typeof(Color), typeof(EZMenu), defaultValueCreator: bindable => Default.Color("ezmenu_items"));

        private Grid blocker;

        public ICommand OnItemTap { get; private set; }

        private int open = 0;
        private int tapping = 0;

        public EZMenu()
        {
            OnItemTap = new Command(Handle_ItemTap);
            
            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZMenuTemplate"];
        }

        protected override async void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if ((width <= 0) || (height <= 0)) return;

            System.Diagnostics.Debug.WriteLine($"EZMenu.OnSizeAllocated: [{Math.Floor(width)}x{Math.Floor(height)}]");

            if (Initialized)
            {
                switch(open)
                {
                    case 0: await SlideOut(false); break;
                    case 1: await SlideIn(false); break;
                }
                return;
            }

            Initialized = true;

            await SlideOut(false);
        }

        public void ThemeChanged()
        {
            Fill = Default.Brush("ezmenu_fill");
            IconFill = Default.Brush("ezmenu_iconfill");
            IconStroke = Default.Brush("ezmenu_iconstroke");
            BorderColor = Default.Color("ezmenu_border");
            ItemsBackgroundColor = Default.Color("ezmenu_items_background");
            ItemsColor = Default.Color("ezmenu_items");

            OnPropertyChanged(nameof(Fill)); // <- Should not be needed, but...
        }

        public void SetBlocker(Grid blocker)
        {
            this.blocker = blocker;
        }

        public bool Initialized { get; private set; } = false;

        public bool IsOpen
        {
            get => (open == 1);
        }

        public new Thickness Margin
        {
            get => (Thickness)GetValue(MarginProperty);
            set => SetValue(MarginProperty, value);
        }

        public new Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        public bool HasShadow
        {
            get => (bool)GetValue(HasShadowProperty);
            set => SetValue(HasShadowProperty, value);
        }

        public Thickness ItemsPadding
        {
            get => (Thickness)GetValue(ItemsPaddingProperty);
            set => SetValue(ItemsPaddingProperty, value);
        }

        public double ItemsSpacing
        {
            get => (double)GetValue(ItemsSpacingProperty);
            set => SetValue(ItemsSpacingProperty, value);
        }

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public EZMenuAlignment Alignment
        {
            get => (EZMenuAlignment)GetValue(AlignmentProperty);
            set => SetValue(AlignmentProperty, value);
        }

        public EZMenuSlide SlideFrom
        {
            get => (EZMenuSlide)GetValue(SlideFromProperty);
            set => SetValue(SlideFromProperty, value);
        }

        public List<EZMenuItem> ItemsSource
        {
            get => (List<EZMenuItem>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public Brush IconFill
        {
            get => (Brush)GetValue(IconFillProperty);
            set => SetValue(IconFillProperty, value);
        }

        public Brush IconStroke
        {
            get => (Brush)GetValue(IconStrokeProperty);
            set => SetValue(IconStrokeProperty, value);
        }

        public double IconStrokeThickness
        {
            get => (double)GetValue(IconStrokeThicknessProperty);
            set => SetValue(IconStrokeThicknessProperty, value);
        }

        public Color BorderColor
        {
            get => (Color)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public Color ItemsBackgroundColor
        {
            get => (Color)GetValue(ItemsBackgroundColorProperty);
            set => SetValue(ItemsBackgroundColorProperty, value);
        }

        public Color ItemsColor
        {
            get => (Color)GetValue(ItemsColorProperty);
            set => SetValue(ItemsColorProperty, value);
        }

        [ComponentEventHandler, AsyncVoidOnPurpose]
        private async void Handle_ItemTap(object item)
        {
            if (0 == Interlocked.Exchange(ref tapping, 1))
            {
                Hide();

                await Task.Delay(300);

                EZMenuItem menu = (EZMenuItem)item;

                EZRaiseResult raised = await EZApp.Container.RaiseContentView(menu.ItemId);

                if (raised == EZRaiseResult.NotFound)
                {
                    EZContentView view = EZApp.Builder.BuildContentView(menu.ItemId);

                    if (view != null)
                    {
                        await EZApp.Container.PushContentView(view);
                    }
                }

                Interlocked.Exchange(ref tapping, 0);
            }            
        }

        [AsyncVoidOnPurpose]
        public async void Hide(bool animated = true)
        {
            if (1 == Interlocked.Exchange(ref open, 0))
            {
                await SlideOut(animated);

                if (blocker != null) blocker.IsVisible = false;
            }
        }

        [AsyncVoidOnPurpose]
        public async void Show(bool animated = true)
        {
            if (0 == Interlocked.Exchange(ref open, 1))
            {
                Opacity = 1D; // <- This should be done once at the end of OnSizeAllocated, but, there it produces a "flick" on iOS, which I don't like :oP

                await SlideIn(animated);

                if (blocker != null) blocker.IsVisible = true;
            }
        }

        [AsyncVoidOnPurpose]
        private async Task SlideOut(bool animated)
        {
            double x = 0D;
            double y = 0D;

            switch (SlideFrom)
            {
                case EZMenuSlide.Left: x = -Width; break;
                case EZMenuSlide.Right: x = Width; break;
                case EZMenuSlide.Top: y = -Height; break;
                case EZMenuSlide.Bottom: y = EZApp.Container.Height + Height; break;
            }

            switch (Alignment)
            {
                case EZMenuAlignment.Bottom: y = EZApp.Container.Height - Height; break;
                case EZMenuAlignment.Center: y = (EZApp.Container.Height / 2) - (Height / 2); break;
            }

            if (!animated)
            {
                TranslationX = x;
                TranslationY = y;
                return;
            }

            _ = await this.TranslateTo(x, y, 250U, Easing.Linear);
        }

        [AsyncVoidOnPurpose]
        private async Task SlideIn(bool animated = true)
        {
            double y = 0D;

            switch (Alignment)
            {
                case EZMenuAlignment.Bottom: y = EZApp.Container.Height - Height; break;
                case EZMenuAlignment.Center: y = (EZApp.Container.Height / 2) - (Height / 2); break;
            }

            if (!animated)
            {
                TranslationX = 0;
                TranslationY = y;
                return;
            }

            _ = await this.TranslateTo(0, y, 250U, Easing.Linear);
        }
    }
}