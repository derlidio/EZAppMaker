/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Windows.Input;

using EZAppMaker.Attributes;
using EZAppMaker.Defaults;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public class EZRadioButton : RadioButton
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZRadioButton), null);
        public static readonly BindableProperty GroupProperty = BindableProperty.Create(nameof(Group), typeof(EZRadioGroup), typeof(EZRadioButton), null);
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(EZRadioButton), null);

        public static readonly BindableProperty LabelColorProperty = BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(EZRadioButton), defaultValueCreator: bindable => Default.Color("ezradiobutton_label"));
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Brush), typeof(EZRadioButton), defaultValueCreator: bindable => Default.Brush("ezradiobutton"));

        private bool state;

        public ICommand OnRadioTap { get; private set; }

        public EZRadioButton()
        {
            HorizontalOptions = LayoutOptions.Start;

            OnRadioTap = new Command(Handle_RadioTap);

            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZRadioButtonTemplate"];
        }

        public void ThemeChanged()
        {
            Color = Default.Brush("ezradiobutton");
            LabelColor = Default.Color("ezradiobutton_label");
        }

        public void StateManager(StateFormAction action)
        {
            switch (action)
            {
                case StateFormAction.Save:

                    state = IsChecked;
                    break;

                case StateFormAction.Restore:

                    if (IsChecked != state)
                    {
                        IsChecked = state;
                    }
                    break;
            }
        }

        public string ItemId
        {
            get => (string)GetValue(ItemIdProperty);
            set => SetValue(ItemIdProperty, value);
        }

        public EZRadioGroup Group
        {
            get => (EZRadioGroup)GetValue(GroupProperty);
            set => SetValue(GroupProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public Brush Color
        {
            get => (Brush)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public Color LabelColor
        {
            get => (Color)GetValue(LabelColorProperty);
            set => SetValue(LabelColorProperty, value);
        }

        [ComponentEventHandler]
        private void Handle_RadioTap(object ItemId)
        {
            // .NET 8 IsEnabled property is broken. It does not propagate
            // the status to layout children. So... we have to workaround.

            if (!IsEnabled && EZWorkarounds.IsEnabledPropagation) return; /* WORKAROUND */

            EZApp.Container.HideKeyboard();

            IsChecked = !IsChecked;
        }
    }
}