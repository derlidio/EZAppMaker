# EZAppMaker
A toolkit for multiplatform apps development on top of MAUI [.NET 8 ready]

EZAppMaker comprises a set of components that shows the same appearance and behavior on both,
Android and iOS. Custom themes and localization are easily done by adding some JSon files or
overwriting existing settings in code.

In orer to build an app with EZAppMaker you'll have to understand the toolkit's paradigm. Basically,
you place a container (`EZContainer`) inside your main page, and use the toolkit's components to present
your content inside that container (with some perks). Instead of using a `<ContentView>`, use `<EZContentView>`.
Instead of `<Button>`, use `<EZButton>`. Instead of `<Entry>` use `<EZEntry>`, and so on.

Not all MAUI components have been replaced. Just a handfull of them (mostly those related
to user input). Well... I think you should see it for yourself. For a demo project showing how
to use EZAppMaker and EZForms, please follow the link below:

[EZTemplate Project Repository](https://github.com/derlidio/EZTemplate)

Note: You must clone EZAppMaker and [EZForms](https://github.com/derlidio/EZForms) in order to run EZTemplate.

The following sections contains an overview of what you should change in your solution
in order to enable EZAppMaker, but I highly reccomend you to download the 3 repositories
and inspect the code. This will give you a better grasp of the framework.

## EZForms
EZForms is a basic set of helper classes that integrates with EZAppMaker. It works with
SQLite and will take care of all your needs on the CRUD regard. You won't have
to write a single SQL statement. You won't have to create models for your tables. Yow
wont't have to bind to Entity Framework. Just plug your SQLite database and write a markup
file (XML) to tell EZForms what you want to see, how you want to see it, and that's it.

EZForms is still under development. It's current version can handle single-record forms pretty
well. Future releases may bring: master-detail forms, multi-column-forms, and linked-forms.

## Perks

Freedom to design your app's interface. Easy to change theme and localization with JSon.
Events to detect and automatically adapt interface appearance when OS theme changes or
when the device is rotated. Non linear page navigation allowed (and automatically controlled,
for single pages or page groups).

## Getting Started
1. Clone EZAppMaker to a folder (of the same name) in your projects directory. If you intend to use EZForms, clone it too;
2. Open Visual Studio and create a '.NET MAUI App'. Your app solution folder should be at the same level of EZAppMaker's;
```
C:\
|
+----> [My Projects]
       |
       +----> [Your_App]
       |      | 
       |      +----> [Your_App]
       |      |      |
       |      |      +----> [Platforms]
       |      |      |
       |      |      +----> [Properties]
       |      |      |
       |      |      +----> [Resources]
       |      |
       |      +----> [Your_App.sln]
       |
       +----> [EZAppMaker]
       |      |
       |      +----> [EZAppMaker]
       .      |
       .      +----> [EZAppMaker.sln]
       .
```
3. Add EZAppMaker.csproj to your solution and then a reference to EZAppMaker on your app's project;
4. Change your `App.xaml.cs` file following the example below:
```csharp
using Microsoft.Maui.Handlers;

using EZAppMaker;
using EZAppMaker.Support;

namespace Your_App;

public partial class App : Application
{
	public App()
	{
        InitializeComponent();

        EntryHandler.Mapper.AppendToMapping("EZEntryBridge", EZHandlers.EZEntryHandler);
        ScrollViewHandler.Mapper.AppendToMapping("EZScrollBridge", EZHandlers.EZScrollTouchHandler);

        EZApp.Initialize(new Builder());

        MainPage = EZApp.MainPage;
	}
}
```
5. Change your MauiProgram.cs following the example below. You'll notice that there are several
font files being added. You may have to download the extra fonts and place them at the `/Resources/Fonts`
folder of your app's project. Tip: [EZTemplate](https://github.com/derlidio/EZTemplate) has all those fonts.
```csharp
using Microsoft.Extensions.Logging;

using EZAppMaker;
using EZAppMaker.Effects;
using EZAppMaker.Bridges;

namespace Your_App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        var builder = MauiApp.CreateBuilder();
        builder
		.UseMauiApp<App>()
        .ConfigureMauiHandlers(handlers =>
        {
#if ANDROID
        handlers.AddHandler(typeof(EZScrollBridge), typeof(EZScrollViewHandler));
#endif
        })
        .ConfigureEffects
        (
            effects =>
            {
                effects.Add<TouchRoutingEffect, TouchPlatformEffect>();
            }
        )
	.ConfigureFonts
        (
            fonts =>
            {
                fonts.AddFont("OpenSans-Bold.ttf", "OpenSansBold");
                fonts.AddFont("OpenSans-BoldItalic.ttf", "OpenSansBoldItalic");
                fonts.AddFont("OpenSans-ExtraBold.ttf", "OpenSansExtraBold");
                fonts.AddFont("OpenSans-ExtraBoldItalic.ttf", "OpenSansExtraBoldItalic");
                fonts.AddFont("OpenSans-Italic.ttf", "OpenSansItalic");
                fonts.AddFont("OpenSans-Light.ttf", "OpenSansLight");
                fonts.AddFont("OpenSans-LightItalic.ttf", "OpenSansLightItalic");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("OpenSans-SemiboldItalic.ttf", "OpenSansSemiboldItalic");
                fonts.AddFont("CourierNew.ttf", "CourierNew");
            }
        );

#if DEBUG
		builder.Logging.AddDebug();
#endif
		return builder.Build();
	}
}
```
6. Change your app's `MainPage.xaml` header following the example below:
```xml
<ez:EZContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                  xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                  xmlns:ez="clr-namespace:EZAppMaker.Components;assembly=EZAppMaker"
                  xmlns:resource="clr-namespace:EZAppMaker.Resources;assembly=EZAppMaker"
                  xmlns:view="clr-namespace:Your_App"
                  x:DataType="view:MainPage"
                  x:Class="Your_App.MainPage"
                  ItemId="MainPage"
                  Title="Main Page">
</ez:EZContentView>            
```
7. On the body of your `MainPage.xaml`, define your app's interface. You must place
the `<ez:EZContainer>` tag where you want your content to show up. EZAppMaker
deppends on this element to function properly. Also, remember that you must create
your views using `EZContentView` derived class so `EZContainer` can handle them.
```xml
    <ContentView.Content>

        <... your preffered layout style ...>
        
            <component:EZContainer x:Name="EZContainer"/>

        </... your preffered layout style ...>

    </ContentView.Content>
```
8. Update your `MainPage.xaml.cs` file to match the changes made to the XAML counterpart:
```csharp
using EZAppMaker.Components;
using EZAppMaker.Support;

namespace Your_App
{
    public partial class MainPage : EZContentView
    {
        public MainPage()
        {
            InitializeComponent();
        }
    }
}
```
9. Create a new Class File, name it Builder.cs, and copy the following code:
```csharp
using EZAppMaker;
using EZAppMaker.Components;
using EZAppMaker.Support;
using EZForms;

namespace Your_App
{
    public class Builder: EZBuilder
    {
        private MainPage main; // Do NOT initialize it here! Instead, do it at the BuildMainPage() event.

        public override void BeforeStart()
        {
            // +---------------------------------------------------------------------+
            // | This method will be called from the App Class Constructor. You MUST |
            // | return from it as quickly as possible. If the App's Constructor do  |
            // | not finish in a short amount of time the Operating System may end   |
            // | the App's process due to timeout!                                   |
            // +---------------------------------------------------------------------+

            // Use this method to set defaults for your App:

            EZSettings.AppName = "Your App Title";
            EZSettings.SmoothTransitions = false;
        }

        public override EZContentView BuildMainPage()
        {
            // Here you must return the EZContentView wich contains your main page layout.
            // Note that EZAppMaker base layout is an AbsoluteLayout. Your ContentView
            // dimensions (WidthRequest and HeightRequest) will be set to the available
            // screen space minus iOS/Android fringe (safe area).

            main = new MainPage();

            return main;
        }

        public override void BuildMenu(EZMenu menu)
        {
            // If you want to use EZMenu Component, you must return
            // a list containing the identifiers and labels for the
            // menu items. You can also provide an icon to accompain
            // the item label in form of a GeometryGroup (SVG Path).

            List<EZMenuItem> items = new List<EZMenuItem>()
            {
                new EZMenuItem()
                {
                    ItemId = "HomeView",
                    Label = "Home",
                    Icon = EZEmbedded.GetPath("Your_App.Assets.Paths.home.txt")
                },

                new EZMenuItem()
                {
                    ItemId = "LicenseView",
                    Label = "License",
                    Icon = EZEmbedded.GetPath("Your_App.Assets.Paths.license.txt")
                },

                new EZMenuItem()
                {
                    ItemId = "ComponentsView",
                    Label = "Components",
                    Icon = EZEmbedded.GetPath("Your_App.Assets.Paths.form.txt")
                },
            };

            menu.ItemsSource = items;

            // You may want to set a Margin and Padding for the menu...

            menu.Padding = 10;
            menu.Margin = 10;

            // ... and define where it'll be displayed:

            menu.SlideFrom = EZMenuSlide.Left;
            menu.Alignment = EZMenuAlignment.Top;
        }

        public override EZContentView BuildContentView(string id)
        {
            // Here you must return the EZContentView wich corresponds to the given id.
            // EZAppMaker will put the view on the top of the ContentView stack (make
            // the page visible). If it already exists on the stack but is not at the
            // top, EZAppMaker will just raise it and this method will not be called.

            EZContentView view = null;

            switch (id)
            {
                case "HomeView": view = new HomeView(); break;
                case "LicenseView": view = new LicenseView(); break;
                case "ComponentsView": view = new ComponentsView(); break;

                default: break;
            }

            return view;
        }

        public override string BuildTheme(AppTheme requested)
        {
            // If you want to override EZAppMaker themes, then you must
            // return a JSon with the same structure found in dark.json
            // or light.json at EZAppMaker.Defaults.Data folder.
            // Otherwise, just return null.

            string json = null;

            switch (requested)
            {
                case AppTheme.Light:

                    // Load your light theme here

                    break;

                case AppTheme.Dark:

                    // Load your dark theme here

                    break;
            }

            return json;
        }

        public override string BuildLocalization()
        {
            // If you want to change EZAppMaker default alert
            // messages and buttons text, return a JSon using
            // the same structure found in localization.json 
            // at EZAppMaker.Defaults.Data folder. Otherwise,
            // just return null.

            return null;
        }

        public override void ThemeChanged()
        {
            // Do things that deppend on the current theme.
            // In order to know what theme is active, inspect:
            // App.Current.RequestedTheme.
        }

        public override void OrientationChanged(EZOrientation orientation)
        {
            // Do things that deppend on the current device orientation.
        }

        public override void CurrentViewChanged(EZContentView view)
        {
            // This method will be called whenever a view is set
            // to the top of the ContentView stack.

            if (view != null)
            {
                // You may interact with the current view here...
            }
        }
    }
}
```
## Notes
On Android Emulator your phisical keyboard (desktop keyboard) will enter text
as expected on entry and combo fields, but the `return` key may behave strange.
So, in order to test if your app is working as you expect, use the soft-keyboard
(on-screen keyboard of the emulator) instead when testing the `return` key behavior.
## Permissions:
EZAppMaker has only one component that requires OS permission to function properly: `EZPhoto`.
If you do not intend to use this feature, you may disregard the following two sections.
### Android Permissions: EZPhoto (Camera)
To be added to "AndroidManifest.xml":
```xml
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.CAMERA" />
<queries>
  <intent>
    <action android:name="android.media.action.IMAGE_CAPTURE" />
  </intent>
</queries>
```
### iOS Permissions: EZPhoto (Camera)
To be added to "info.plist":
```xml
<key>NSCameraUsageDescription</key>
<string>This app needs access to the camera to take photos.</string>
<key>NSMicrophoneUsageDescription</key>
<string>This app needs access to microphone for taking videos.</string>
<key>NSPhotoLibraryAddUsageDescription</key>
<string>This app needs access to the photo gallery for picking photos and videos.</string>
<key>NSPhotoLibraryUsageDescription</key>
<string>This app needs access to photos gallery for picking photos and videos.</string>
```
Note that the messages above are too generic. In order to have your app accepted by the
App Store (in order to pass their review process) you may have to be more specific!