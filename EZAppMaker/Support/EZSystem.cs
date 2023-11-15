/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

namespace EZAppMaker.Support
{
    public static class EZSystem
    {
        public static void PlacePhoneCall(string number)
        {
            if (DeviceInfo.DeviceType != DeviceType.Virtual) // Can't place calls on Simulator or Emulator.
            {
                try
                {
                    PhoneDialer.Open(number);
                }
                catch { /* Dismiss */ }
            }
        }

        public static async Task OpenBrowserAsync(string address)
        {
            try
            {
                if
                (
                    !address.StartsWith("http://")
                    &&
                    !address.StartsWith("https://")
                    &&
                    !address.StartsWith("mailto:")
                )
                {
                    address = "http://" + address;
                }

                await Browser.OpenAsync(address);
            }
            catch { /* Dismiss */ }
        }
    }
}