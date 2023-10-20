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
    public static class EZSettings
    {
        public static string AppName { get; set; } = "EZ App Maker";
        public static bool SmoothTransitions { get; set; } = false; // Fade content in & out on page transitions.
        public static bool ContentReposition { get; set; } = true;  // Reposition the scroller when raising a ContentView from the stack.
        public static bool UseSafeArea { get; set; } = true;        // Account the notch space when rendering EZAppMaker MainPage shell.
        public static int MinTapInterval { get; set; } = 250;       // Disable tapping on some components at intervals less than this setting.
        public static uint BallonTime { get; set; } = 3000;         // Visibility time of message balloons.
        public static double FormsColumnSpacing { get; set; } = 5;  // Spacing for same line controls on EZForms
    }
}