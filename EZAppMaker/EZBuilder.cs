﻿/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using EZAppMaker.Components;

namespace EZAppMaker
{ 
    public class EZBuilder
	{
        public EZBuilder()
        {
            BeforeStart();
        }

        public virtual void BeforeStart()
        {
            // +---------------------------------------------------------------------+
            // | This method will be called from the App Class Constructor. You MUST |
            // | return from it as quickly as possible. If the App's Constructor do  |
            // | not finish in a short amount of time the Operating System may end   |
            // | the App's process due to timeout!                                   |
            // +---------------------------------------------------------------------+
        }

        public virtual void Ready()
        {
            // +----------------------------------------------------------------+
            // | This method will be called when all elements of EZAppMaker are |
            // | properly in place and initialized. Here you can start your app |
            // | activities.                                                    |
            // +----------------------------------------------------------------+
        }

        public virtual EZContentView BuildMainPage()
        {
            // Here you must return the ContentView wich contains your main page layout.
            // Your MainPage dimensions (WidthRequest and HeightRequest) will be set to
            // the available screen space minus iOS/Android fringe (safe area).

            return new EZContentView();
        }

        public virtual void BuildMenu(EZMenu menu)
        {
            // If you want to use EZMenu Component, you must return
            // a list containing the identifiers and labels for the
            // menu items. You can also provide an icon to ilustrate
            // the item label in form of a GeometryGroup (SVG Path).
        }

        public virtual EZContentView BuildContentView(string id)
        {
            // Here you must return the ContentView wich corresponds to the given id.
            // EZAppMaker will put the view on the top of the ContentView stack (make
            // the page visible). If it already exists on the stack but is not at the
            // top, EZAppMaker will just raise it and this method will not be called.

            return null;
        }

        public virtual string BuildTheme(AppTheme requested)
        {
            // If you want to override EZAppMaker themes, then you must
            // return a JSon with the same structure found in dark.json
            // or light.json at EZAppMaker.Defaults.Data folder.

            return null;
        }

        public virtual string BuildLocalization()
        {
            // If you want to change EZAppMaker default messages
            // and alert buttons text, return a JSon using the
            // same structure found in localization.json
            // at EZAppMaker.Defaults.Data folder.

            return null;
        }

        public virtual void ThemeChanged()
        {
            // Do things that deppend on the current theme.
            // In order to know what theme is active, inspect:
            // App.Current.RequestedTheme.
        }

        public virtual void OrientationChanged(EZOrientation orientation)
        {
            // Do things that deppend on the current device orientation.   
        }

        public virtual void CurrentViewChanged(EZContentView view)
        {
            // This method will be called whenever a view is set
            // to the top of the ContentView stack. Here we are
            // setting the "page title area" of our main page to
            // reflect the title of the page being viewed:
        }

        public virtual bool BackButtonPressed()
        {
            // This method will be called whenever the user presses
            // the operating system's Back Button. Mainly for Android
            // devices. Return true if your app has handled the event.
            // Return false to keep the OS default behavior.

            return false;
        }
    }
}