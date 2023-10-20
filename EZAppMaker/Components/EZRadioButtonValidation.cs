/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

namespace EZAppMaker.Components
{
    public class EZRadioButtonValidation : ContentView
    {
        public delegate bool OnValidateHandler(EZRadioButtonValidation validation, IEnumerable<EZRadioButton> items);
        public event OnValidateHandler OnValidate;

        public static readonly BindableProperty GroupNameProperty = BindableProperty.Create(nameof(GroupName), typeof(string), typeof(EZRadioButtonValidation), null);
        public static readonly BindableProperty RequiredAlertProperty = BindableProperty.Create(nameof(RequiredAlert), typeof(string), typeof(EZRadioButtonValidation), null);
        public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(nameof(IsRequired), typeof(bool), typeof(EZRadioButtonValidation), false);

        public EZRadioButtonValidation()
        {
            /* nothing here */
        }

        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        public string RequiredAlert
        {
            get => (string)GetValue(RequiredAlertProperty);
            set => SetValue(RequiredAlertProperty, value);
        }

        public bool IsRequired
        {
            get => (bool)GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

        public bool Validate(IEnumerable<EZRadioButton> items)
        {
            bool valid = true;

            if (OnValidate != null)
            {
                valid = OnValidate.Invoke(this, items);
            }

            return valid;
        }
    }
}