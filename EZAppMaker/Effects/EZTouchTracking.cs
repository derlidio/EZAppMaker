/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

namespace EZAppMaker.Effects
{
    public enum TouchActionType
    {
        Entered,
        Pressed,
        Moved,
        Released,
        Exited,
        Cancelled
    }

    public class TouchActionEventArgs : EventArgs
    {
        public TouchActionEventArgs(long id, TouchActionType type, System.Drawing.Point location, bool isInContact)
        {
            Id = id;
            Type = type;
            Location = location;
            IsInContact = isInContact;
        }

        public long Id { get; private set; }

        public TouchActionType Type { get; private set; }

        public System.Drawing.Point Location { get; private set; }

        public bool IsInContact { get; private set; }
    }

    public delegate void TouchActionEventHandler(object sender, TouchActionEventArgs args);

    public class TouchRoutingEffect : RoutingEffect
    {
        public event TouchActionEventHandler TouchAction;

        public bool Capture { set; get; }

        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            TouchAction?.Invoke(element, args);
        }
    }
}