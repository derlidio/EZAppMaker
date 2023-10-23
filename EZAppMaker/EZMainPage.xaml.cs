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

namespace EZAppMaker
{
    public enum EZOrientation
    {
        Undefined,
        Portrait,
        Landscape
    }

    public partial class EZMainPage : ContentPage
    {
        public delegate void OnReadyHandler();
        public event OnReadyHandler OnReady;

        private EZOrientation orientation = EZOrientation.Undefined;
        
        public EZMainPage(OnReadyHandler handler = null)
        {
            OnReady = handler;
            Background = Default.Brush("screen_background");

            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            return EZApp.Builder.BackButtonPressed();
        }

        public bool Initialized { get; private set; }

        public bool IsLandscape
        {
            get { return Width > Height; }
        }

        public bool IsPortrait
        {
            get { return Height > Width; }
        }

        public EZOrientation Orientation
        {
            get { return orientation; }
        }

        public double AvailableHeight
        {
            get
            {
                return MainLayout.Height;
            }            
        }

        public double AvailableWidth
        {
            get
            {
                return MainLayout.Width;
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            System.Diagnostics.Debug.WriteLine($"EZMainPage.OnSizeAllocated: [{Math.Floor(width)}x{Math.Floor(height)}]");

            //   ___ _           _   
            //  / __| |_  ___ __| |__
            // | (__| ' \/ -_) _| / /
            //  \___|_||_\___\__|_\_\

            if (Initialized)
            {
                Background = Default.Brush("screen_background");

                foreach (View view in MainLayout.Children)
                {
                    view.WidthRequest = width;
                    view.HeightRequest = height;
                }

                EZOrientation orientation = (width < height) ? EZOrientation.Portrait : EZOrientation.Landscape;

                if (this.orientation != orientation)
                {
                    this.orientation = orientation;
                    EZApp.OrientationChanged();
                }

                return;
            }

            //   ___      _ _   _      _ _         _   _
            //  |_ _|_ _ (_) |_(_)__ _| (_)_____ _| |_(_)___ _ _ 
            //   | || ' \| |  _| / _` | | |_ / _` |  _| / _ \ ' \
            //  |___|_||_|_|\__|_\__,_|_|_/__\__,_|\__|_\___/_||_|

            if ((width > 0) && (height > 0))
            {
                Initialized = true;
                orientation = (width < height) ? EZOrientation.Portrait : EZOrientation.Landscape;
                OnReady?.Invoke();
            }
        }

        public void ThemeChanged()
        {
            Background = Default.Brush("screen_background");
        }

        public void AddChild(ContentView view)
        {
            if (view != null)
            {
                MainLayout.Add(view);
            }
        }
    }
}