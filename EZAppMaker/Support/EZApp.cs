/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Text.RegularExpressions;

using Newtonsoft.Json;

using EZAppMaker.Components;
using EZAppMaker.Defaults;
using EZAppMaker.PopUp;

namespace EZAppMaker.Support
{
    public class CrashReport
    {
        public DateTime DateTime { get; set; }
        public string View { get; set; }

        public string Manufacturer { get; set; }
        public string Platform { get; set; }
        public string System { get; set; }
        public string Model { get; set; }
        public string Name { get; set; }
        public string AppVersion { get; set; }
        public string AppBuild { get; set; }
        public string Exception { get; set; }
    }

    public static class EZApp
    {
        public static EZBuilder Builder { get; private set; }
        public static EZModal Modal { get; private set; }
        public static EZContainer Container { get; private set; }
        public static EZMainPage MainPage { get; private set; }
        public static bool Initialized { get; private set; }

        public delegate void HideKeyboardHandler();
        public static HideKeyboardHandler HideKeyboard;

        static EZApp()
        {
            Application.Current.RequestedThemeChanged += ThemeChanged;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            var e = new Exception("TaskSchedulerOnUnobservedTaskException", unobservedTaskExceptionEventArgs.Exception);
            LogUnhandledException(e);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var e = new Exception("CurrentDomainOnUnhandledException", unhandledExceptionEventArgs.ExceptionObject as Exception);
            LogUnhandledException(e);
        }

        internal static async void LogUnhandledException(Exception exception)
        {
            VersionTracking.Track();

            CrashReport report = new CrashReport() { DateTime = DateTime.Now };

            EZContentView view = await Container.GetTopPage();

            report.View = (view == null) ? "Undefined" : view.ItemId;
            report.Manufacturer = DeviceInfo.Manufacturer;
            report.Platform = DeviceInfo.Platform.ToString();
            report.System = DeviceInfo.VersionString;
            report.Model = DeviceInfo.Model;
            report.Name = DeviceInfo.Name;
            report.AppVersion = VersionTracking.CurrentVersion;
            report.AppBuild = VersionTracking.CurrentBuild;
            report.Exception = Regex.Replace(exception.ToString(), "in ([/A-Z a-z 0-9 \x2D_.]*)[/]", "in ");

            string json = JsonConvert.SerializeObject(report);

            EZLocalAppData.SaveFile("crash_exception.json", json);
        }

        public static void Initialize(EZBuilder builder)
        {
            System.Diagnostics.Debug.WriteLine($"Initializing EZApp");
            Builder = builder;
            MainPage = new EZMainPage(Ready);
        }

        private static async void Ready()
        {
            System.Diagnostics.Debug.WriteLine($"Building EZApp Content");

            await Task.Delay(100);

            //   ___              _                   _      __  __      _      ___               
            //  |   \ _____ _____| |___ _ __  ___ _ _( )___ |  \/  |__ _(_)_ _ | _ \__ _ __ _ ___ 
            //  | |) / -_) V / -_) / _ \ '_ \/ -_) '_|/(_-< | |\/| / _` | | ' \|  _/ _` / _` / -_)
            //  |___/\___|\_/\___|_\___/ .__/\___|_|   /__/ |_|  |_\__,_|_|_||_|_| \__,_\__, \___|
            //                         |_|                                              |___/

            EZContentView main = Builder?.BuildMainPage();

            if (main == null)
            {
                System.Diagnostics.Debug.WriteLine("No Main Page provided!");
                return;
            }
            
            main.WidthRequest = MainPage.AvailableWidth;
            main.HeightRequest = MainPage.AvailableHeight;

            Container = main.GetChildren<EZContainer>()?.FirstOrDefault();

            if (Container == null)
            {
                System.Diagnostics.Debug.WriteLine("No EZContainer provided on Main Page!");
                return;
            }

            Container.OnReady += ContainerReady;

            MainPage.AddChild(main);

            System.Diagnostics.Debug.WriteLine($"User MainPage added to MainLayout");

            //   ___ ______  __         _      _ 
            //  | __|_  /  \/  |___  __| |__ _| |
            //  | _| / /| |\/| / _ \/ _` / _` | |
            //  |___/___|_|  |_\___/\__,_\__,_|_|

            Modal = new EZModal();

            Modal.WidthRequest = MainPage.AvailableWidth;
            Modal.HeightRequest = MainPage.AvailableHeight;

            MainPage.AddChild(Modal);

            Modal.IsVisible = false;

            System.Diagnostics.Debug.WriteLine($"EZModal added to MainLayout");
        }

        private static void ContainerReady()
        {
            System.Diagnostics.Debug.WriteLine("Container.Ready()");

            Container.BuildMenu();

            EZContentView content = Builder?.BuildContentView("HomeView");

            if (content != null)
            {
                _ = Container.PushContentView(content);
            }
        }

        public static void ThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
            Default.SetTheme();

            Container?.ThemeChanged();
            MainPage?.ThemeChanged();
            Builder?.ThemeChanged();
        }

        public static void OrientationChanged()
        {
            Builder.OrientationChanged(MainPage.Orientation);            
        }

        public static void Alert(string message)
        {
            EZAlertView alert = new EZAlertView(message);

            Modal.Show(alert);
        }

        public static async Task<bool> Question(string message)
        {
            EZQuestionView question = new EZQuestionView(message);

            Modal.Show(question);

            while(Modal.IsVisible)
            {
                await Task.Delay(50);
            }
            bool answer = (bool)Modal.Result;

            return answer;
        }

        public static bool ValidateRequired(EZContentView view, out VisualElement failed, bool alert = true, bool flash = true)
        {
            bool ok = true;
            string msg = "";

            failed = null;

            EZEntry entry = null;
            EZCombo combo = null;
            EZSignature signature = null;

            foreach (VisualElement element in view.GetChildren<VisualElement>())
            {
                // EZEntry

                if (element.GetType() == typeof(EZEntry))
                {
                    entry = (EZEntry)element;

                    if ((entry.IsRequired && string.IsNullOrWhiteSpace(entry.Text)) || !entry.Validate())
                    {
                        failed = entry;

                        msg = entry.RequiredAlert;

                        if (string.IsNullOrWhiteSpace(msg) && !string.IsNullOrWhiteSpace(entry.Label))
                        {
                            msg = Default.Localization("validate_required_entry").Replace("{1}", entry.Label?.Replace(":", ""));
                        }

                        entry = null; ok = false;

                        break;
                    }

                    continue;
                }

                // EZCombo

                if (element.GetType() == typeof(EZCombo))
                {
                    combo = (EZCombo)element;

                    if ((combo.IsRequired && (combo.SelectedItem == null)) || !combo.Validate())
                    {
                        failed = combo;

                        msg = combo.RequiredAlert;

                        if (string.IsNullOrWhiteSpace(msg) && !string.IsNullOrWhiteSpace(combo.Label))
                        {
                            msg = Default.Localization("validate_required_combo").Replace("{1}", combo.Label?.Replace(":", ""));
                        }

                        combo = null; ok = false;

                        break;
                    }

                    continue;
                }

                // EZSignature

                if (element.GetType() == typeof(EZSignature))
                {
                    signature = (EZSignature)element;

                    if ((signature.IsRequired && string.IsNullOrWhiteSpace(signature.Data)) || !signature.Validate())
                    {
                        failed = signature;

                        msg = signature.RequiredAlert;

                        if (string.IsNullOrWhiteSpace(msg) && !string.IsNullOrWhiteSpace(entry.Label))
                        {
                            msg = Default.Localization("validate_required_signature");
                        }

                        signature = null; ok = false; flash = false;

                        break;
                    }

                    continue;
                }
            }

            if (!ok)
            {
                if (flash)
                {
                    Container.MakeVisible(failed);

                    combo?.Flash();
                    entry?.Flash();
                }

                if (alert && !string.IsNullOrWhiteSpace(msg))
                {
                    Container.ShowBalloon(failed, msg);
                }
            }
            else
            {
                ok = ValidateCheckBoxes(view, out failed, alert) && ValidateRadioButtons(view, out failed, alert);
            }

            return ok;
        }

        private static bool ValidateCheckBoxes(EZContentView view, out VisualElement failed, bool alert)
        {
            bool ok = true;
            string msg = "";
            failed = null;

            IEnumerable <EZCheckBoxValidation> checks = view.GetChildren<EZCheckBoxValidation>();

            if (checks == null) return ok;

            foreach (EZCheckBoxValidation validation in checks)
            {
                if (string.IsNullOrWhiteSpace(validation.GroupName)) continue;

                IEnumerable<EZCheckBox> elements = GetCheckBoxes(view, validation.GroupName);

                if (elements == null) continue;

                ok = (validation.IsRequired && CountChecked(elements) != 0) || !validation.Validate(elements);

                if (!ok)
                {
                    failed = elements.FirstOrDefault<EZCheckBox>();                                
                    msg = string.IsNullOrWhiteSpace(validation.RequiredAlert) ? Default.Localization("validate_required_checkbox").Replace("{1}", validation.GroupName) : validation.RequiredAlert;

                    break;
                }
            }

            if (!ok && alert && !string.IsNullOrWhiteSpace(msg))
            {
                Container.MakeVisible(failed);
                Container.ShowBalloon(failed, msg);
            }

            return ok;
        }

        private static bool ValidateRadioButtons(EZContentView view, out VisualElement failed, bool alert)
        {
            bool ok = true;
            string msg = "";
            failed = null;

            IEnumerable<EZRadioButtonValidation> radios = view.GetChildren<EZRadioButtonValidation>();

            if (radios == null) return ok;

            foreach (EZRadioButtonValidation validation in radios)
            {
                if (string.IsNullOrWhiteSpace(validation.GroupName)) continue;

                IEnumerable<EZRadioButton> elements = GetRadioButtons(view, validation.GroupName);

                if (elements == null) continue;

                ok = (validation.IsRequired && CountChecked(elements) != 0) || !validation.Validate(elements);

                if (!ok)
                {
                    failed = elements.FirstOrDefault<EZRadioButton>();

                    msg = string.IsNullOrWhiteSpace(validation.RequiredAlert) ? Default.Localization("validate_required_radiobutton").Replace("{1}", validation.GroupName) : validation.RequiredAlert;

                    break;
                }
            }

            if (!ok && alert && !string.IsNullOrWhiteSpace(msg))
            {
                Container.MakeVisible(failed);
                Container.ShowBalloon(failed, msg);
            }

            return ok;
        }

        private static IEnumerable<EZCheckBox> GetCheckBoxes(EZContentView view, string group)
        {
            IEnumerable<EZCheckBox> elements = view.GetChildren<EZCheckBox>().Where((x) => x.GroupName == group);

            return elements;
        }

        private static IEnumerable<EZRadioButton> GetRadioButtons(EZContentView view, string group)
        {
            IEnumerable<EZRadioButton> elements = view.GetChildren<EZRadioButton>().Where((x) => x.GroupName == group);

            return elements;
        }

        private static int CountChecked(IEnumerable<EZRadioButton> elements)
        {
            int checks = 0;

            try
            {
                foreach (EZRadioButton radio in elements)
                {
                    if (radio.IsChecked) checks++;
                }
            }
            catch { /* Dismiss */ }

            return checks;
        }

        private static int CountChecked(IEnumerable<EZCheckBox> elements)
        {
            int checks = 0;

            try
            {
                foreach (EZCheckBox check in elements)
                {
                    if (check.IsChecked) checks++;
                }
            }
            catch { /* Dismiss */ }

            return checks;
        }
    }
}