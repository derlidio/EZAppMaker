using System.Runtime.CompilerServices;

using EZAppMaker.Support;

namespace EZAppMaker.Components
{
	public class EZProgressBar : ContentView
	{
        private int updating = 0;
        private int recalc = 0;
        private double current = 0;

        private readonly Frame frame;
        private readonly Frame bar;
        private readonly Label progress;

        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZProgressBar), string.Empty);
        public static readonly BindableProperty PercentProperty = BindableProperty.Create(nameof(Percent), typeof(double), typeof(EZProgressBar), 0D);

        public EZProgressBar()
        {
            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZProgressBarTemplate"];

            frame = (Frame)GetTemplateChild("OuterFrame");
            bar = (Frame)GetTemplateChild("ProgressBar");
            progress = (Label)GetTemplateChild("Progress");
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(Percent)) UpdateProgress();
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

        public double Current
        {
            get
            {
                return current;
            }
        }

        private async void UpdateProgress()
        {
            if (0 != Interlocked.Exchange(ref updating, 1))
            {
                Interlocked.Exchange(ref recalc, 1);
                return;
            }

            double step = Percent - current / 100;

            while (current < Percent)
            {
                if (recalc == 1)
                {
                    step = Percent - current / 100;
                    Interlocked.Exchange(ref recalc, 0);
                    continue;
                }

                current += step;

                if (current > Percent)
                {
                    current = Percent;
                }

                double width = frame.Width * current / 100;

                if (width > bar.MinimumHeightRequest)
                {
                    bar.WidthRequest = width;
                }

                progress.Text = current.ToString("0.#");

                await Task.Delay(10);
            }

            Interlocked.Exchange(ref updating, 0);
        }
    }
}