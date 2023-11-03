/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public class EZExpander: ContentView
    {
        public EZExpander()
        {
            IsVisible = false;

            HeightRequest = Math.Floor(EZApp.MainPage.AvailableHeight / 2);            
        }

        public async Task<bool> Expand()
        {
            if (IsVisible) return false;

            double desired = Math.Floor(EZApp.MainPage.AvailableHeight / 2); // In case of orientation change...

            if (HeightRequest != desired)
            {
                HeightRequest = desired;
            }

            System.Diagnostics.Debug.WriteLine("Expand: awaiting");

            await EZApp.Container.Resizing.WaitAsync();
            {
                System.Diagnostics.Debug.WriteLine("Expand: acquired");
                IsVisible = true;
                await Task.Delay(100);
            }
            EZApp.Container.Resizing.Release();

            System.Diagnostics.Debug.WriteLine("Expand: released");

            return true;
        }

        public async Task<bool> Contract()
        {
            System.Diagnostics.Debug.WriteLine("Contract: awaiting");

            await EZApp.Container.Resizing.WaitAsync();
            {
                System.Diagnostics.Debug.WriteLine("Contract: acquired");
                IsVisible = false;
                await Task.Delay(100);
            }
            EZApp.Container.Resizing.Release();

            System.Diagnostics.Debug.WriteLine("Contract: released");

            return !IsVisible;
        }
    }
}