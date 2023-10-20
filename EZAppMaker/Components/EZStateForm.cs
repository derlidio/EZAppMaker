/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|

(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Reflection;

using EZAppMaker.Attributes;
using EZAppMaker.Defaults;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public enum StateFormAction
    {
        Save,
        Restore
    }

    public class EZStateForm : ContentView
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZStateForm), null);
        public static readonly BindableProperty ShowButtonsProperty = BindableProperty.Create(nameof(ShowButtons), typeof(bool), typeof(EZStateForm), true);

        private readonly EZButton Change;
        private readonly EZButton Cancel;
        private readonly EZButton Save;

        private readonly ContentView Container;

        public delegate void StateManager(StateFormAction action);

        public delegate void OnSaveRequestHandler(EZStateForm form);
        public event OnSaveRequestHandler OnSaveRequest;

        public EZStateForm()
        {
            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZFormProtectorTemplate"];

            Container = (ContentView)GetTemplateChild("Container");

            Change = (EZButton)GetTemplateChild("Change");
            Cancel = (EZButton)GetTemplateChild("Cancel");
            Save = (EZButton)GetTemplateChild("Apply");

            Change.Label = Default.Localization("ezstateform_change");
            Cancel.Label = Default.Localization("ezstateform_cancel");
            Save.Label = Default.Localization("ezstateform_apply");

            Change.OnTap += Handle_Change;
            Cancel.OnTap += Handle_Cancel;
            Save.OnTap += Handle_Save;
        }

        public double ContentOpacity
        {
            get
            {
                if (Change == null)
                {
                    return 0.75;
                }
                else
                {
                    return Change.IsEnabled ? 0.75 : 1;
                }
            }
        }

        public string ItemId
        {
            get => (string)GetValue(ItemIdProperty);
            set => SetValue(ItemIdProperty, value);
        }

        public bool ShowButtons
        {
            get => (bool)GetValue(ShowButtonsProperty);
            set => SetValue(ShowButtonsProperty, value);
        }

        public bool Locked => !(Container.IsEnabled);

        [ComponentEventHandler]
        private void Handle_Change(EZButton button)
        {
            Change.IsEnabled = false;
            Cancel.IsVisible = true;
            Save.IsVisible = true;

            Container.IsEnabled = true;

            CascadeAction(this, StateFormAction.Save);

            OnPropertyChanged(nameof(ContentOpacity));
            OnPropertyChanged(nameof(Locked));
        }

        [ComponentEventHandler]
        private void Handle_Cancel(EZButton button)
        {
            Change.IsEnabled = true;
            Cancel.IsVisible = false;
            Save.IsVisible = false;

            Container.IsEnabled = false;

            CascadeAction(this, StateFormAction.Restore);

            OnPropertyChanged(nameof(ContentOpacity));
            OnPropertyChanged(nameof(Locked));
        }

        [ComponentEventHandler]
        private void Handle_Save(EZButton button)
        {
            Change.IsEnabled = true;
            Cancel.IsVisible = false;
            Save.IsVisible = false;

            Container.IsEnabled = false;

            OnPropertyChanged(nameof(ContentOpacity));
            OnPropertyChanged(nameof(Locked));

            OnSaveRequest?.Invoke(this);
        }

        private void CascadeAction(Element element, StateFormAction action)
        {
            foreach (Element child in EZXamarin.GetChildren<Element>(element))
            {
                MethodInfo m = child.GetType().GetMethod("StateManager");
                m?.Invoke(child, new object[] { action });
            }
        }
    }
}