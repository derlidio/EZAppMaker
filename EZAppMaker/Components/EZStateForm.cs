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
        public readonly BindableProperty DimContentProperty = BindableProperty.Create(nameof(DimContent), typeof(bool), typeof(EZStateForm), true);

        private readonly Grid Blocker;
        private readonly EZButton Change;
        private readonly EZButton Cancel;
        private readonly EZButton Save;

        private readonly ContentView Container;

        public delegate void StateManager(StateFormAction action);

        public delegate bool OnSaveRequestHandler(EZStateForm form);
        public event OnSaveRequestHandler OnSaveRequest;

        public EZStateForm()
        {
            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZStateFormTemplate"];

            Container = (ContentView)GetTemplateChild("Container");

            Blocker = (Grid)GetTemplateChild("Blocker");
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

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            OnPropertyChanged(nameof(ContentOpacity)); /* WORKAROUND - Should not be needed, but... */
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

        public bool DimContent
        {
            get => (bool)GetValue(DimContentProperty);
            set => SetValue(DimContentProperty, value);
        }

        public double ContentOpacity
        {
            get
            {
                System.Diagnostics.Debug.WriteLine($"DimContent: {DimContent}");

                if (!DimContent) return 1D;

                if (Change != null)
                {
                    return Change.IsEnabled ? 0.75D : 1D; // It's really inverted. Do not confuse! ;o)
                }

                return 1D;
            }
        }

        public bool Locked => !(Container.IsEnabled);

        [ComponentEventHandler]
        private void Handle_Change(EZButton button)
        {
            Change.IsEnabled = false;
            Cancel.IsEnabled = true;
            Save.IsEnabled = true;

            Blocker.IsVisible = false;

            CascadeAction(this, StateFormAction.Save);
            OnPropertyChanged(nameof(Locked));
            OnPropertyChanged(nameof(ContentOpacity));
        }

        [ComponentEventHandler]
        private void Handle_Cancel(EZButton button)
        {
            Change.IsEnabled = true;
            Cancel.IsEnabled = false;
            Save.IsEnabled = false;

            Blocker.IsVisible = true;

            CascadeAction(this, StateFormAction.Restore);
            OnPropertyChanged(nameof(Locked));
            OnPropertyChanged(nameof(ContentOpacity));
        }

        [ComponentEventHandler]
        private void Handle_Save(EZButton button)
        {
            if (OnSaveRequest != null)
            {
                bool ok = (bool)OnSaveRequest?.Invoke(this);

                if (!ok) return;
            }

            Change.IsEnabled = true;
            Cancel.IsEnabled = false;
            Save.IsEnabled = false;

            Blocker.IsVisible = true;

            OnPropertyChanged(nameof(Locked));
            OnPropertyChanged(nameof(ContentOpacity));
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