/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

//   _  ___  ___ 
//  (_)/ _ \/ __|
//  | | (_) \__ \
//  |_|\___/|___/

using UIKit;

namespace EZAppMaker.Effects
{
    public class TouchPlatformEffect : Microsoft.Maui.Controls.Platform.PlatformEffect
    {
        UIView view;

        TouchRecognizer touchRecognizer;

        protected override void OnAttached()
        {
            view = Control == null ? Container : Control;

            TouchRoutingEffect effect = (TouchRoutingEffect)Element.Effects.FirstOrDefault(e => e is TouchRoutingEffect);

            if ((effect != null) && (view != null))
            {
                touchRecognizer = new TouchRecognizer(Element, view, effect);
                view.AddGestureRecognizer(touchRecognizer);
            }
        }

        protected override void OnDetached()
        {
            if (touchRecognizer != null)
            {
                touchRecognizer.Detach();
                view.RemoveGestureRecognizer(touchRecognizer);
            }
        }
    }
}