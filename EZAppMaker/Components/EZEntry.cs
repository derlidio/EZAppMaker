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
using EZAppMaker.Behaviors;
using EZAppMaker.Bridges;
using EZAppMaker.Defaults;
using EZAppMaker.Interfaces;
using EZAppMaker.Support;

using System.Runtime.CompilerServices;

namespace EZAppMaker.Components
{
    public class EZEntry : ContentView, IEZComponent, IEZFocusable
    {
        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZEntry), null);
        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(EZEntry), null);
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(EZEntry), null);
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(EZEntry), null, BindingMode.TwoWay);
        public static readonly BindableProperty MaskProperty = BindableProperty.Create(nameof(Mask), typeof(string), typeof(EZEntry), null);
        public static readonly BindableProperty RequiredAlertProperty = BindableProperty.Create(nameof(RequiredAlert), typeof(string), typeof(EZEntry), null);
        public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(EZEntry), 255);
        public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(EZEntry), false);
        public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(EZEntry), false);
        public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(nameof(IsRequired), typeof(bool), typeof(EZEntry), false);
        public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(EZEntry), Keyboard.Plain);
        public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(EZEntry), TextAlignment.Start);

        public static readonly BindableProperty BackColorProperty = BindableProperty.Create(nameof(BackColor), typeof(Color), typeof(EZEntry), defaultValueCreator: bindable => Default.Color("ezentry_back"));
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EZEntry), defaultValueCreator: bindable => Default.Color("ezentry_border"));
        public static readonly BindableProperty FocusedColorProperty = BindableProperty.Create(nameof(FocusedColor), typeof(Color), typeof(EZEntry), defaultValueCreator: bindable => Default.Color("ezentry_focused"));
        public static readonly BindableProperty FlashColorProperty = BindableProperty.Create(nameof(FlashColor), typeof(Color), typeof(EZEntry), defaultValueCreator: bindable => Default.Color("ezentry_flash"));
        public static readonly BindableProperty LabelColorProperty = BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(EZEntry), defaultValueCreator: bindable => Default.Color("ezentry_label"));
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(EZEntry), defaultValueCreator: bindable => Default.Color("ezentry_text"));
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(EZEntry), defaultValueCreator: bindable => Default.Color("ezentry_placeholder"));
        public static readonly BindableProperty IconFillProperty = BindableProperty.Create(nameof(IconFill), typeof(Brush), typeof(EZEntry), defaultValueCreator: bindable => Default.Brush("ezentry_icon"));

        private string initial;
        private string state;
        private bool focused;

        private readonly Entry InternalEntry;
        private readonly Grid EntryTapper;
        private readonly Frame EntryFrame;
        private readonly Grid ClearButton;
        private readonly Grid Password;

        private EZMaskedEntry MaskBehavior;

        private bool isPasswordVisible = false;
        private int flashing = 0;

        public delegate void OnChange(EZEntry entry, TextChangedEventArgs args);
        public event OnChange OnChanged;

        public delegate void OnSearchTap(EZEntry entry);
        public OnSearchTap OnSearchTapped;

        public delegate void OnReturnPressed(EZEntry entry);
        public event OnReturnPressed OnCompleted;

        public delegate void OnFocused(EZEntry entry);
        public new event OnFocused Focused;

        public delegate void OnUnfocused(EZEntry entry);
        public new event OnUnfocused Unfocused;

        public ICommand OnClearTap { get; private set; }
        public ICommand OnEntryTap { get; private set; }
        public ICommand OnPasswordVisibilityTap { get; private set; }

        public delegate bool OnValidateHandler(EZEntry entry);
        public event OnValidateHandler OnValidate;

        public EZEntry()
        {
            OnClearTap = new Command(Handle_ClearTap);
            OnEntryTap = new Command(Handle_EntryTap);
            OnPasswordVisibilityTap = new Command(Handle_PasswordVisibility);

            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZEntryTemplate"];

            EntryFrame = (Frame)GetTemplateChild("EntryFrame");
            InternalEntry = (EZEntryBridge)GetTemplateChild("InternalEntry");
            EntryTapper = (Grid)GetTemplateChild("EntryTapper");
            ClearButton = (Grid)GetTemplateChild("ClearButton");
            Password = (Grid)GetTemplateChild("Password");

            if (InternalEntry != null)
            {
                InternalEntry.TextChanged += Handle_TextChanged;
                InternalEntry.Completed += Handle_Completed;
            }
        }

        public void ThemeChanged()
        {
            BackColor = Default.Color("ezentry_back");
            BorderColor = Default.Color("ezentry_border");
            FocusedColor = Default.Color("ezentry_focused");
            FlashColor = Default.Color("ezentry_flash");
            LabelColor = Default.Color("ezentry_label");
            TextColor = Default.Color("ezentry_text");
            PlaceholderColor = Default.Color("ezentry_placeholder");
            IconFill = Default.Brush("ezentry_icon");

            EntryFrame.BorderColor = BorderColor;
            EntryFrame.BackgroundColor = focused ? FocusedColor : BackColor;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            initial = Text;
        }

        public new void Focus()
        {
            if (!IsEnabled || IsReadOnly) return;

            focused = true;
            ToggleFocus();
            Focused?.Invoke(this);
        }

        public new void Unfocus()
        {
            if (!IsEnabled || IsReadOnly) return;

            focused = false;
            ToggleFocus();
            Unfocused?.Invoke(this);
        }

        public void Clear()
        {
            Text = null;
        }

        public bool Modified()
        {
            return Text != initial;
        }

        public object ToDatabaseValue(object target)
        {
            object result = null;

            Type type = target?.GetType();

            if (type == typeof(string))
            {
                if (!string.IsNullOrWhiteSpace(Text))
                {
                    result = Text;
                }
            }

            else

            if (type == typeof(int))
            {
                result = int.TryParse(Text, out int int_value) ? int_value : target;
            }

            else

            if (type == typeof(long))
            {
                result = long.TryParse(Text, out long long_value) ? long_value : target;
            }

            else

            if (type == typeof(double))
            {
                result = double.TryParse(Text, out double double_value) ? double_value : target;
            }

            return result;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case "IsEnabled":

                    Opacity = IsEnabled ? 1.0D : 0.5D; break;

                case "IsPassword":

                    OnPropertyChanged(nameof(IsPasswordBridge));
                    OnPropertyChanged(nameof(PasswordIndicatorVisibilityOn));
                    OnPropertyChanged(nameof(PasswordIndicatorVisibilityOff));
                    break;

                case "Mask":

                    if (MaskBehavior != null)
                    {
                        InternalEntry.Behaviors.Remove(MaskBehavior);
                    }

                    if (!string.IsNullOrWhiteSpace(Mask))
                    {
                        MaskBehavior = new EZMaskedEntry(Mask);
                        InternalEntry.Behaviors.Add(MaskBehavior);
                        InternalEntry.MaxLength = Mask.Length;
                    }
                    break;

                case "Label":

                    OnPropertyChanged(nameof(HasLabel));
                    break;
            }
        }

        public void StateManager(StateFormAction action)
        {
            switch (action)
            {
                case StateFormAction.Save:

                    state = Text;
                    break;

                case StateFormAction.Restore:

                    if (Text != state)
                    {
                        Text = state;
                    }
                    break;
            }
        }

        public bool Detached { get; set; }
        public bool IsSearchable { get; set; }

        public VisualElement FocusedElement
        {
            get
            {
                return InternalEntry;
            }
        }

        public string ItemId
        {
            get => (string)GetValue(ItemIdProperty);
            set => SetValue(ItemIdProperty, value);
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public string Mask
        {
            get => (string)GetValue(MaskProperty);
            set => SetValue(MaskProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public string RequiredAlert
        {
            get => (string)GetValue(RequiredAlertProperty);
            set => SetValue(RequiredAlertProperty, value);
        }

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        public Color BackColor
        {
            get => (Color)GetValue(BackColorProperty);
            set => SetValue(BackColorProperty, value);
        }

        public Color BorderColor
        {
            get => (Color)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public Color FocusedColor
        {
            get => (Color)GetValue(FocusedColorProperty);
            set => SetValue(FocusedColorProperty, value);
        }

        public Color FlashColor
        {
            get => (Color)GetValue(FlashColorProperty);
            set => SetValue(FlashColorProperty, value);
        }

        public Color LabelColor
        {
            get => (Color)GetValue(LabelColorProperty);
            set => SetValue(LabelColorProperty, value);
        }

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public Color PlaceholderColor
        {
            get => (Color)GetValue(PlaceholderColorProperty);
            set => SetValue(PlaceholderColorProperty, value);
        }

        public Brush IconFill
        {
            get => (Brush)GetValue(IconFillProperty);
            set => SetValue(IconFillProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public bool IsPassword
        {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }

        public bool IsRequired
        {
            get => (bool)GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

        public Keyboard Keyboard
        {
            get => (Keyboard)GetValue(KeyboardProperty);
            set => SetValue(KeyboardProperty, value);
        }

        public TextAlignment HorizontalTextAlignment
        {
            get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
            set => SetValue(HorizontalTextAlignmentProperty, value);
        }

        public bool HasLabel
        {
            get => !string.IsNullOrEmpty(Label);
        }

        public bool IsPasswordBridge
        {
            get => IsPassword && !isPasswordVisible;
        }

        public bool PasswordIndicatorVisibilityOn
        {
            get => IsPassword && !isPasswordVisible;
        }

        public bool PasswordIndicatorVisibilityOff
        {
            get => IsPassword && isPasswordVisible;
        }

        public string RawText
        {
            get
            {
                string value = EZText.RemoveMask(Text, Mask); ;

                return value;
            }
        }

        [AsyncVoidOnPurpose]
        public async void Flash()
        {
            if (flashing != 0) return;

            if (0 == Interlocked.Exchange(ref flashing, 1))
            {
                for (int i = 1; i < 5; i++)
                {
                    EntryFrame.BackgroundColor = FlashColor;
                    await Task.Delay(250);
                    EntryFrame.BackgroundColor = InternalEntry.IsFocused ? FocusedColor : BackColor;
                    await Task.Delay(250);
                }

                Interlocked.Exchange(ref flashing, 0);
            }
        }

        public bool Validate()
        {
            bool valid = true;

            if (OnValidate != null)
            {
                valid = OnValidate.Invoke(this);
            }

            return valid;
        }

        [AsyncVoidOnPurpose]
        private void ToggleFocus()
        {
            if (focused) // Focus gain
            {
                InternalEntry.Margin = new Thickness(10, 0, 32, 0);
                InternalEntry.IsReadOnly = false;

                EntryFrame.BackgroundColor = FocusedColor;

                if (IsPassword)
                {
                    Password.IsVisible = true;
                }
                else
                {
                    ClearButton.IsVisible = true;
                }

                EZApp.Container.HandleFocus(this, true);

                EntryTapper.IsVisible = false;

                return;
            }

            // Focus lost

            EntryFrame.BackgroundColor = BackColor;

            ClearButton.IsVisible = false;
            Password.IsVisible = false;
            InternalEntry.Margin = new Thickness(10, 0, 10, 0);
            InternalEntry.IsReadOnly = true;

            EZApp.Container.HandleFocus(this, false);

            EntryTapper.IsVisible = true;
        }

        [ComponentEventHandler]
        private void Handle_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnChanged?.Invoke(this, e);
        }

        [ComponentEventHandler]
        private void Handle_Completed(object sender, EventArgs e)
        {
            Unfocus();
            OnCompleted?.Invoke(this);
        }

        [ComponentEventHandler]
        public void Handle_ClearTap()
        {
            Text = null;
        }

        [ComponentEventHandler]
        public void Handle_EntryTap()
        {
            if (focused || IsReadOnly) return;

            Focus();
        }

        [ComponentEventHandler]
        public void Handle_PasswordVisibility()
        {
            isPasswordVisible = !isPasswordVisible;

            OnPropertyChanged(nameof(IsPasswordBridge));
            OnPropertyChanged(nameof(PasswordIndicatorVisibilityOn));
            OnPropertyChanged(nameof(PasswordIndicatorVisibilityOff));
        }
    }
}