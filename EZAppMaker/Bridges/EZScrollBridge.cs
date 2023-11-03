using System.Runtime.CompilerServices;

using Microsoft.Maui.Handlers;

namespace EZAppMaker.Bridges
{
    public class EZScrollBridge: ScrollView
    {
        public IScrollViewHandler handler;

        public delegate void SetScrollingState(IScrollViewHandler handler, bool state);
        public SetScrollingState SetScrolling; // For EZAppMaker internal use. Do not mess with this! :o)

        public static readonly BindableProperty ScrollingEnabledProperty = BindableProperty.Create(nameof(ScrollingEnabled), typeof(bool), typeof(EZScrollBridge), true);

        public EZScrollBridge() 
        {
        }

        private void EZScrollBridge_ChildAdded(object sender, ElementEventArgs e)
        {
            ContentView view = (ContentView)e.Element;

            view.VerticalOptions = LayoutOptions.Start;
        }

        public bool ScrollingEnabled
        {
            get => (bool)GetValue(ScrollingEnabledProperty);
            set => SetValue(ScrollingEnabledProperty, value);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == "ScrollingEnabled")
            {
                SetScrolling?.Invoke(handler, ScrollingEnabled);
            }
        }
    }
}