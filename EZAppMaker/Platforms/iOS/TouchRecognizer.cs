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

using CoreGraphics;
using Foundation;
using UIKit;

namespace EZAppMaker.Effects
{
    class TouchRecognizer : UIGestureRecognizer
    {
        Element element;
        UIView view;
        TouchRoutingEffect touchEffect;
        bool capture;

        static Dictionary<UIView, TouchRecognizer> viewDictionary = new Dictionary<UIView, TouchRecognizer>();

        static Dictionary<long, TouchRecognizer> idToTouchDictionary = new Dictionary<long, TouchRecognizer>();

        public TouchRecognizer(Element element, UIView view, TouchRoutingEffect touchEffect)
        {
            this.element = element;
            this.view = view;
            this.touchEffect = touchEffect;

            viewDictionary.Add(view, this);
        }

        public void Detach()
        {
            viewDictionary.Remove(view);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            // touches = touches of interest
            // evt = all touches of type UITouch

            base.TouchesBegan(touches, evt);

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = ((IntPtr)touch.Handle).ToInt64();

                FireEvent(this, id, TouchActionType.Pressed, touch, true);

                if (!idToTouchDictionary.ContainsKey(id))
                {
                    idToTouchDictionary.Add(id, this);
                }
            }

            // Save the setting of the Capture property:

            capture = touchEffect.Capture;
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = ((IntPtr)touch.Handle).ToInt64();

                if (capture)
                {
                    FireEvent(this, id, TouchActionType.Moved, touch, true);
                }
                else
                {
                    CheckForBoundaryHop(touch);

                    if (idToTouchDictionary[id] != null)
                    {
                        FireEvent(idToTouchDictionary[id], id, TouchActionType.Moved, touch, true);
                    }
                }
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = ((IntPtr)touch.Handle).ToInt64();

                if (capture)
                {
                    FireEvent(this, id, TouchActionType.Released, touch, false);
                }
                else
                {
                    CheckForBoundaryHop(touch);

                    if (idToTouchDictionary[id] != null)
                    {
                        FireEvent(idToTouchDictionary[id], id, TouchActionType.Released, touch, false);
                    }
                }

                idToTouchDictionary.Remove(id);
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = ((IntPtr)touch.Handle).ToInt64();

                if (capture)
                {
                    FireEvent(this, id, TouchActionType.Cancelled, touch, false);
                }
                else if (idToTouchDictionary[id] != null)
                {
                    FireEvent(idToTouchDictionary[id], id, TouchActionType.Cancelled, touch, false);
                }

                idToTouchDictionary.Remove(id);
            }
        }

        void CheckForBoundaryHop(UITouch touch)
        {
            long id = ((IntPtr)touch.Handle).ToInt64();

            TouchRecognizer recognizerHit = null; // TODO: Might require converting to a List for multiple hits!

            foreach (UIView view in viewDictionary.Keys)
            {
                CGPoint location = touch.LocationInView(view);

                if (new CGRect(new CGPoint(), view.Frame.Size).Contains(location))
                {
                    recognizerHit = viewDictionary[view];
                }
            }
            if (recognizerHit != idToTouchDictionary[id])
            {
                if (idToTouchDictionary[id] != null)
                {
                    FireEvent(idToTouchDictionary[id], id, TouchActionType.Exited, touch, true);
                }

                if (recognizerHit != null)
                {
                    FireEvent(recognizerHit, id, TouchActionType.Entered, touch, true);
                }

                idToTouchDictionary[id] = recognizerHit;
            }
        }

        void FireEvent(TouchRecognizer recognizer, long id, TouchActionType actionType, UITouch touch, bool isInContact)
        {
            // Convert touch location to Xamarin.Forms Point value:

            CGPoint cgPoint = touch.LocationInView(recognizer.View);

            System.Drawing.Point xfPoint = new System.Drawing.Point((int)cgPoint.X, (int)cgPoint.Y);

            // Get the method to call for firing events:

            Action<Element, TouchActionEventArgs> onTouchAction = recognizer.touchEffect.OnTouchAction;

            // Call that method:

            onTouchAction(recognizer.element, new TouchActionEventArgs(id, actionType, xfPoint, isInContact));
        }
    }
}