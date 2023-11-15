/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Reflection;
using System.Windows.Input;

using EZAppMaker.Attributes;
using EZAppMaker.Defaults;
using EZAppMaker.Interfaces;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public class EZCheckBox : ContentView, IEZComponent
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZCheckBox), null);
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(EZCheckBox), null);
        public static readonly BindableProperty GroupNameProperty = BindableProperty.Create(nameof(GroupName), typeof(string), typeof(EZCheckBox), null);
        public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(EZCheckBox), false, BindingMode.TwoWay);

        public static readonly BindableProperty LabelColorProperty = BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(EZCheckBox), defaultValueCreator: bindable => Default.Color("ezcheckbox_label"));
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Brush), typeof(EZCheckBox), defaultValueCreator: bindable => Default.Brush("ezcheckbox"));

        public static readonly BindableProperty ContextOnChangeProperty = BindableProperty.Create(nameof(ContextOnChange), typeof(string), typeof(EZButton), null);

        public delegate void OnChangeHandler(EZCheckBox checkbox);
        public event OnChangeHandler OnChange;

        private bool initial;
        private bool state;

        public ICommand OnCheckTap { get; private set; }

        public EZCheckBox()
        {
            OnCheckTap = new Command(Handle_CheckTap);

            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZCheckBoxTemplate"];
            
            SetState();
        }

        public void ThemeChanged()
        {
            LabelColor = Default.Color("ezcheckbox_label");
            Color = Default.Brush("ezcheckbox");
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            initial = IsChecked;

            SetState();
        }

        public void Clear()
        {
            IsChecked = false;
        }

        public bool Modified()
        {
            return IsChecked != initial;
        }

        public object ToDatabaseValue(object target)
        {
            return IsChecked ? 1L : 0L;
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

        public bool Detached { get; set; }

        public string ItemId
        {
            get => (string)GetValue(ItemIdProperty);
            set => SetValue(ItemIdProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);

            set
            {
                SetValue(IsCheckedProperty, value);
                SetState();
            }
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

        public string ContextOnChange
        {
            get => (string)GetValue(ContextOnChangeProperty);
            set => SetValue(ContextOnChangeProperty, value);
        }

        [ComponentEventHandler]
        private void Handle_CheckTap(object ItemId)
        {
            // .NET 8 IsEnabled property is broken. It does not propagate
            // the status to layout children. So... we have to workaround.

            if (!IsEnabled && EZWorkarounds.IsEnabledPropagation) return; /* WORKAROUND */

            EZApp.Container.HideKeyboard();

            SetValue(IsCheckedProperty, !(bool)GetValue(IsCheckedProperty));
            SetState();
            OnChange?.Invoke(this);

            if (!string.IsNullOrWhiteSpace(ContextOnChange))
            {
                MethodInfo tap = BindingContext?.GetType()?.GetMethod(ContextOnChange);

                if (tap != null)
                {
                    tap?.Invoke(BindingContext, new object[] { this });
                }
            }
        }

        private void SetState()
        {
            object ic = GetTemplateChild("BoxChecked");
            object iu = GetTemplateChild("BoxUnchecked");

            if (ic != null)
            {
                Microsoft.Maui.Controls.Shapes.Path p = (Microsoft.Maui.Controls.Shapes.Path)ic;
                p.Opacity = IsChecked ? 1D : 0D;
            }

            if (iu != null)
            {
                Microsoft.Maui.Controls.Shapes.Path p = (Microsoft.Maui.Controls.Shapes.Path)iu;
                p.Opacity = IsChecked ? 0D : 1D;
            }
        }
    }
}