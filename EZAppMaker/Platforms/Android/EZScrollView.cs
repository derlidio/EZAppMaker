using Android.Content;
using Android.Util;
using Android.Views;

using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace EZAppMaker
{
    public class EZScrollView : MauiScrollView
    {
        public bool ScrollEnabled = true;

        public EZScrollView(Context context) : base(context) { }
        public EZScrollView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) { }

        public override bool OnTouchEvent(MotionEvent me)
        {
            return ScrollEnabled && base.OnTouchEvent(me);
        }

        public override bool OnInterceptTouchEvent(MotionEvent me)
        {
            return ScrollEnabled && base.OnInterceptTouchEvent(me);
        }
    }

    public class EZScrollViewHandler : ScrollViewHandler
    {
        protected override MauiScrollView CreatePlatformView()
        {
            return new EZScrollView(MauiContext!.Context)
            {
                ClipToOutline = true,
                FillViewport = true
            };

            //var scrollView = new EZScrollView
            //(
            //    new ContextThemeWrapper
            //    (
            //        MauiContext!.Context,
            //        Resource.Style.scrollViewTheme
            //    ),
            //    null!,
            //    Resource.Attribute.scrollViewStyle
            //)
            //{
            //    ClipToOutline = true,
            //    FillViewport = true
            //};
            //
            //return scrollView;
        }
    }
}