﻿/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

//     _           _         _    _ 
//    /_\  _ _  __| |_ _ ___(_)__| |
//   / _ \| ' \/ _` | '_/ _ \ / _` |
//  /_/ \_\_||_\__,_|_| \___/_\__,_|

using Microsoft.Maui.Handlers;

using EZAppMaker.Bridges;
using EZAppMaker.Components;

namespace EZAppMaker
{
    public static class EZHandlers
    {
        //   ___ _______     _            ___     _    _          
        //  | __|_  / __|_ _| |_ _ _ _  _| _ )_ _(_)__| |__ _ ___ 
        //  | _| / /| _|| ' \  _| '_| || | _ \ '_| / _` / _` / -_)
        //  |___/___|___|_||_\__|_|  \_, |___/_| |_\__,_\__, \___|
        //                           |__/               |___/

        public static void EZEntryHandler(IEntryHandler handler, IEntry view)
        {
            if ((handler == null) || (handler.PlatformView == null) || (view == null) || (view is not EZEntryBridge)) return;
            
            handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent); // Remove the Underline.
            handler.PlatformView.SetPadding(0, 1, 0, 1); // This will assure correct entry rendering (else it cuts text vertically). Odd, but necessary.
        }

        //   ___ _______             _ _ ___     _    _          
        //  | __|_  / __| __ _ _ ___| | | _ )_ _(_)__| |__ _ ___ 
        //  | _| / /\__ \/ _| '_/ _ \ | | _ \ '_| / _` / _` / -_)
        //  |___/___|___/\__|_| \___/_|_|___/_| |_\__,_\__, \___|
        //                                             |___/

        public static void EZScrollTouchHandler(IScrollViewHandler handler, IScrollView view)
        {
            if ((handler == null) || (handler.PlatformView == null) || (view == null) || (view is not EZScrollBridge)) return;

            EZScrollBridge scroll = (EZScrollBridge)view;

            scroll.handler = handler;
            scroll.SetScrolling = SetScrolling;
        }

        private static void SetScrolling(IScrollViewHandler handler, bool state)
        {
            if ((handler == null) || (handler.PlatformView == null)) return;

            EZScrollView v = (EZScrollView)handler.PlatformView;

            v.ScrollEnabled = state;

            System.Diagnostics.Debug.WriteLine($"Scrolling is {(state ? "Enabled" : "Disabled")}");
        }

        //   ___ _____         _         _ ___     _    _          
        //  | __|_  / |   __ _| |__  ___| | _ )_ _(_)__| |__ _ ___ 
        //  | _| / /| |__/ _` | '_ \/ -_) | _ \ '_| / _` / _` / -_)
        //  |___/___|____\__,_|_.__/\___|_|___/_| |_\__,_\__, \___|
        //                                               |___/

        public static void EZLabelHandler(ILabelHandler handler, ILabel label)
        {
            if ((handler == null) || (handler.PlatformView == null) || (label == null) || (label is not EZLabel)) return;

            if (handler.PlatformView.Ellipsize != Android.Text.TextUtils.TruncateAt.End) return;

            EZLabel lbl = (EZLabel)label;

            if (lbl.MaxLines > 1)
            {
                handler.PlatformView.SetMaxLines(lbl.MaxLines);
            }                
        }

        //  __      __   _  __   ___            
        //  \ \    / /__| |_\ \ / (_)_____ __ __
        //   \ \/\/ / -_) '_ \ V /| / -_) V  V /
        //    \_/\_/\___|_.__/\_/ |_\___|\_/\_/

        public static void EZWebViewHandler(IWebViewHandler handler, IWebView view)
        {
            handler.PlatformView.Settings.AllowContentAccess = true;
            handler.PlatformView.Settings.AllowFileAccess = true;

            handler.PlatformView.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
        }
    }
}