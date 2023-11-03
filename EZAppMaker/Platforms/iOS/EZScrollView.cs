using UIKit;
using CoreGraphics;

using Microsoft.Maui.Handlers;

namespace EZAppMaker
{
    public class EZScrollView : UIScrollView
    {

    }

    public class EZScrollViewHandler : ScrollViewHandler
    {
        protected override UIScrollView CreatePlatformView()
        {
            EZScrollView sv = new EZScrollView
            {
                ClipsToBounds = true,
                Bounces = true,
                ContentMode = UIViewContentMode.Top
            };

            return sv;
        }
    }
}