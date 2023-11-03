namespace EZAppMaker.Support
{
	public static class EZWorkarounds
	{
		public static bool KeyboardFocus
		{
			get
			{   // EZAppMaker will implement ways to handle
				// input fields focus and the soft-keyboard.

				return true;
			}
		}

		public static bool ScrollViewContentSize
		{
            get
            {   // EZAppMaker will implement a workaround to keep
				// the main ScrollView content properly aligned
				// when it's size changes (when a combo is open,
				// for example). So far, just needed on iOS.

				return DeviceInfo.Platform == DevicePlatform.iOS;
            }
        }

		public static bool IsEnabledPropagation
		{
            get
			{   // EZAppMaker will check IsEnabled status
				// of custom components to  handle touch
				// interactions on disabled containers.

				return true;
			}
        }
    }
}