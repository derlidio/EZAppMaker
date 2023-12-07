using System.Runtime.CompilerServices;

using EZAppMaker.Support;

namespace EZAppMaker.Components
{
	public class EZProgressBar : ContentView
	{
        private bool initialized = false;
        private int updating = 0;
        private int recalc = 0;
        private double current = 0;

        private readonly Border bar;
        private readonly Label progress;

        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZProgressBar), string.Empty);
        public static readonly BindableProperty PercentProperty = BindableProperty.Create(nameof(Percent), typeof(double), typeof(EZProgressBar), 0D);

        public delegate void OnFinish();
        public OnFinish OnFinished;

        public EZProgressBar()
        {
            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZProgressBarTemplate"];

            bar = (Border)GetTemplateChild("ProgressBar");
            progress = (Label)GetTemplateChild("Progress");

            bar.SizeChanged += Handle_Resize;
        }

        private void Handle_Resize(object sender, EventArgs e)
        {
            if (bar.Width > 0)
            {
                bar.TranslationX = -bar.Width;
                bar.Opacity = 1D;
                initialized = true;
                UpdateProgress();
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(Percent))
            {
                UpdateProgress();
            }
        }

        public string ItemId
        {
            get => (string)GetValue(ItemIdProperty);
            set => SetValue(ItemIdProperty, value);
        }

        public double Percent
        {
            get => (double)GetValue(PercentProperty);

            set
            {
                if (value > 100D) value = 100D;

                SetValue(PercentProperty, value);
            }
        }

        private async void UpdateProgress()
        {
            if (!initialized) return;

            if (0 != Interlocked.Exchange(ref updating, 1))
            {
                Interlocked.Exchange(ref recalc, 1);
                return;
            }

            bool exit = false;

            double step = (Percent - current) / 10;

            while(!exit && (step != 0D))
            {
                current += step;

                if (current > Percent) current = Percent;

                progress.Text = current.ToString("0");
                bar.TranslationX = - bar.Width + bar.Width * current / 100D;
                await Task.Delay(10);

                if (1 == Interlocked.Exchange(ref recalc, 0))
                {
                    step = (Percent - current) / 100D;
                }

                exit = current == Percent;
            }

            Interlocked.Exchange(ref updating, 0);

            if (Percent == 100D)
            {
                OnFinished?.Invoke();
            }
        }
    }
}