/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using EZAppMaker.Interfaces;

namespace EZAppMaker.Components
{
    public class EZRadioGroup: StackLayout, IEZComponent
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZRadioGroup), null);

        private object initial = null;
        private object state = null;

        public EZRadioGroup() : base() { }

        public bool Detached { get; set; }

        public string ItemId
        {
            get => (string)GetValue(ItemIdProperty);
            set => SetValue(ItemIdProperty, value);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            initial = SelectedChild();
        }

        private object SelectedChild()
        {
            object selected = null;

            EZRadioButton radio;

            foreach (Element element in Children)
            {
                if (element.GetType() == typeof(EZRadioButton))
                {
                    radio = (EZRadioButton)element;

                    if (radio.IsChecked)
                    {
                        selected = radio.Value;
                        break;
                    }
                }
            }

            return selected;
        }

        private void SelectChild(object value)
        {
            EZRadioButton radio;

            foreach (Element element in Children)
            {
                if (element.GetType() == typeof(EZRadioButton))
                {
                    radio = (EZRadioButton) element;

                    if (radio.Value == value)
                    {
                        radio.IsChecked = true;
                        break;
                    }
                }
            }
        }

        public void StateManager(StateFormAction action)
        {
            switch (action)
            {
                case StateFormAction.Save:

                     state = SelectedChild();
                     break;

                case StateFormAction.Restore:

                     SelectChild(state);
                     break;
            }
        }

        public new void Clear()
        {
            foreach (EZRadioButton radio in Children)
            {
                radio.IsChecked = false;
            }
        }

        public bool Modified()
        {
            EZRadioButton radio;

            object current = null;

            foreach (Element element in Children)
            {
                if (element.GetType() == typeof(EZRadioButton))
                {
                    radio = (EZRadioButton)element;

                    if (radio.IsChecked)
                    {
                        current = radio.Value;
                        break;
                    }
                }
            }

            return current?.ToString() != initial?.ToString();
        }

        public object ToDatabaseValue(object target)
        {
            object result = null;

            EZRadioButton radio;

            foreach (Element element in Children)
            {
                if (element.GetType() == typeof(EZRadioButton))
                {
                    radio = (EZRadioButton)element;

                    if (radio.IsChecked)
                    {
                        result = radio.Value;
                        break;
                    }
                }
            }

            return result;
        }
    }
}