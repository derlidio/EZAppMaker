/*__ ____  _             
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

using Android.Views;

using Microsoft.Maui.Platform;

namespace EZAppMaker.Effects
{
    public class TouchPlatformEffect : Microsoft.Maui.Controls.Platform.PlatformEffect
    {
        Android.Views.View view;
        Element formsElement;
        TouchRoutingEffect libTouchEffect;
        bool capture;
        Func<double, double> fromPixels;
        int[] twoIntArray = new int[2];

        static Dictionary<Android.Views.View, TouchPlatformEffect> viewDictionary = new Dictionary<Android.Views.View, TouchPlatformEffect>();

        static Dictionary<int, TouchPlatformEffect> idToEffectDictionary = new Dictionary<int, TouchPlatformEffect>();

        protected override void OnAttached()
        {
            // Get the Android View corresponding to the Element that the effect is attached to:

            view = Control == null ? Container : Control;

            // Get access to the TouchEffect class in the .NET Standard library:

            TouchRoutingEffect touchEffect = (TouchRoutingEffect)Element.Effects.FirstOrDefault(e => e is TouchRoutingEffect);

            if ((touchEffect != null) && (view != null))
            {
                viewDictionary.Add(view, this);

                formsElement = Element;

                libTouchEffect = touchEffect;

                // Save fromPixels function:

                fromPixels = view.Context.FromPixels;

                // Set event handler on View:

                view.Touch += OnTouch;
            }
        }

        protected override void OnDetached()
        {
            if (viewDictionary.ContainsKey(view))
            {
                viewDictionary.Remove(view);
                view.Touch -= OnTouch;
            }
        }

        void OnTouch(object sender, Android.Views.View.TouchEventArgs args)
        {
            // Two object common to all the events:

            Android.Views.View senderView = sender as Android.Views.View;
            MotionEvent motionEvent = args.Event;

            // Get the pointer index:

            int pointerIndex = motionEvent.ActionIndex;

            // Get the id that identifies a finger over the course of its progress:

            int id = motionEvent.GetPointerId(pointerIndex);

            senderView.GetLocationOnScreen(twoIntArray);

            Point screenPointerCoords = new Point
            (
                  twoIntArray[0] + motionEvent.GetX(pointerIndex),
                  twoIntArray[1] + motionEvent.GetY(pointerIndex)
            );

            // Use ActionMasked here rather than Action to reduce the number of possibilities:

            switch (args.Event.ActionMasked)
            {
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:

                    FireEvent(this, id, TouchActionType.Pressed, screenPointerCoords, true);
                    idToEffectDictionary.Add(id, this);
                    capture = libTouchEffect.Capture;

                    break;

                case MotionEventActions.Move:

                    // Multiple Move events are bundled, so handle them in a loop:

                    for (pointerIndex = 0; pointerIndex < motionEvent.PointerCount; pointerIndex++)
                    {
                        id = motionEvent.GetPointerId(pointerIndex);

                        if (capture)
                        {
                            senderView.GetLocationOnScreen(twoIntArray);

                            screenPointerCoords = new Point
                            (
                                twoIntArray[0] + motionEvent.GetX(pointerIndex),
                                twoIntArray[1] + motionEvent.GetY(pointerIndex)
                            );

                            FireEvent(this, id, TouchActionType.Moved, screenPointerCoords, true);
                        }
                        else
                        {
                            CheckForBoundaryHop(id, screenPointerCoords);

                            if (idToEffectDictionary[id] != null)
                            {
                                FireEvent(idToEffectDictionary[id], id, TouchActionType.Moved, screenPointerCoords, true);
                            }
                        }
                    }

                    break;

                case MotionEventActions.Up:
                case MotionEventActions.Pointer1Up:

                    if (capture)
                    {
                        FireEvent(this, id, TouchActionType.Released, screenPointerCoords, false);
                    }
                    else
                    {
                        CheckForBoundaryHop(id, screenPointerCoords);

                        if (idToEffectDictionary[id] != null)
                        {
                            FireEvent(idToEffectDictionary[id], id, TouchActionType.Released, screenPointerCoords, false);
                        }
                    }

                    idToEffectDictionary.Remove(id);

                    break;

                case MotionEventActions.Cancel:

                    if (capture)
                    {
                        FireEvent(this, id, TouchActionType.Cancelled, screenPointerCoords, false);
                    }
                    else
                    {
                        if (idToEffectDictionary[id] != null)
                        {
                            FireEvent(idToEffectDictionary[id], id, TouchActionType.Cancelled, screenPointerCoords, false);
                        }
                    }

                    idToEffectDictionary.Remove(id);

                    break;
            }
        }

        void CheckForBoundaryHop(int id, Point pointerLocation)
        {
            TouchPlatformEffect touchEffectHit = null;

            foreach (Android.Views.View view in viewDictionary.Keys)
            {
                try // Get the view rectangle:
                {
                    view.GetLocationOnScreen(twoIntArray);
                }
                catch // System.ObjectDisposedException: Cannot access a disposed object!
                {
                    continue;
                }

                System.Drawing.Rectangle viewRect = new System.Drawing.Rectangle(twoIntArray[0], twoIntArray[1], view.Width, view.Height);

                System.Drawing.Point p = new System.Drawing.Point()
                {
                    X = (int)pointerLocation.X,
                    Y = (int)pointerLocation.Y
                };

                if (viewRect.Contains(p))
                {
                    touchEffectHit = viewDictionary[view];
                }
            }

            if (touchEffectHit != idToEffectDictionary[id])
            {
                if (idToEffectDictionary[id] != null)
                {
                    FireEvent(idToEffectDictionary[id], id, TouchActionType.Exited, pointerLocation, true);
                }

                if (touchEffectHit != null)
                {
                    FireEvent(touchEffectHit, id, TouchActionType.Entered, pointerLocation, true);
                }

                idToEffectDictionary[id] = touchEffectHit;
            }
        }

        void FireEvent(TouchPlatformEffect touchEffect, int id, TouchActionType actionType, Point pointerLocation, bool isInContact)
        {
            // Get the method to call for firing events:

            Action<Element, TouchActionEventArgs> onTouchAction = touchEffect.libTouchEffect.OnTouchAction;

            // Get the location of the pointer within the view:

            touchEffect.view.GetLocationOnScreen(twoIntArray);

            double x = pointerLocation.X - twoIntArray[0];
            double y = pointerLocation.Y - twoIntArray[1];

            System.Drawing.Point point = new System.Drawing.Point()
            {
                X = (int)fromPixels(x),
                Y = (int)fromPixels(y)
            };

            // Call the method:

            onTouchAction(touchEffect.formsElement, new TouchActionEventArgs(id, actionType, point, isInContact));
        }
    }
}