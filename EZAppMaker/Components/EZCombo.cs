/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Windows.Input;
using System.Runtime.CompilerServices;

using EZAppMaker.Attributes;
using EZAppMaker.Behaviors;
using EZAppMaker.Bridges;
using EZAppMaker.Defaults;
using EZAppMaker.Interfaces;
using EZAppMaker.Support;

namespace EZAppMaker.Components
{
    public class EZCombo : ContentView, IEZComponent, IEZFocusable
    {
        private class ComboGroup: List<object>
        {
            public string Header { get; set; }
        }

        // Entry:

        public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(EZCombo), null);
        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(EZCombo), null);
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(EZCombo), null, BindingMode.TwoWay);
        public static readonly BindableProperty MaskProperty = BindableProperty.Create(nameof(Mask), typeof(string), typeof(EZCombo), null);
        public static readonly BindableProperty RequiredAlertProperty = BindableProperty.Create(nameof(RequiredAlert), typeof(string), typeof(EZCombo), null);
        public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(EZCombo), 255);
        public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(EZCombo), false);
        public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(nameof(IsRequired), typeof(bool), typeof(EZCombo), false);
        public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(EZCombo), Keyboard.Plain);
        public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(EZCombo), TextAlignment.Start);

        public static readonly BindableProperty BackColorProperty = BindableProperty.Create(nameof(BackColor), typeof(Color), typeof(EZCombo), defaultValueCreator: bindable => Default.Color("ezentry_back"));
        public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EZCombo), defaultValueCreator: bindable => Default.Color("ezentry_border"));
        public static readonly BindableProperty FocusedColorProperty = BindableProperty.Create(nameof(FocusedColor), typeof(Color), typeof(EZCombo), defaultValueCreator: bindable => Default.Color("ezentry_focused"));
        public static readonly BindableProperty FlashColorProperty = BindableProperty.Create(nameof(FlashColor), typeof(Color), typeof(EZCombo), defaultValueCreator: bindable => Default.Color("ezentry_flash"));
        public static readonly BindableProperty LabelColorProperty = BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(EZCombo), defaultValueCreator: bindable => Default.Color("ezentry_label"));
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(EZCombo), defaultValueCreator: bindable => Default.Color("ezentry_text"));
        public static readonly BindableProperty PlaceholderColorProperty = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(EZCombo), defaultValueCreator: bindable => Default.Color("ezentry_placeholder"));
        public static readonly BindableProperty IconFillProperty = BindableProperty.Create(nameof(IconFill), typeof(Brush), typeof(EZCombo), defaultValueCreator: bindable => Default.Brush("ezentry_icon"));

        // List:

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<object>), typeof(EZCombo), null);

        public static readonly BindableProperty RawValueProperty = BindableProperty.Create(nameof(RawValue), typeof(object), typeof(EZCombo), null, BindingMode.TwoWay);

        public static readonly BindableProperty ItemIdProperty = BindableProperty.Create(nameof(ItemId), typeof(string), typeof(EZCombo), null);
        public static readonly BindableProperty KeyProperty = BindableProperty.Create(nameof(Key), typeof(string), typeof(EZCombo), null);
        public static readonly BindableProperty ItemProperty = BindableProperty.Create(nameof(Item), typeof(string), typeof(EZCombo), null);
        public static readonly BindableProperty DetailProperty = BindableProperty.Create(nameof(Detail), typeof(string), typeof(EZCombo), null);
        public static readonly BindableProperty GroupProperty = BindableProperty.Create(nameof(Group), typeof(string), typeof(EZCombo), null);
        public static readonly BindableProperty SortedProperty = BindableProperty.Create(nameof(Sorted), typeof(bool), typeof(EZCombo), false);

        public static readonly BindableProperty HeaderBackgroundColorProperty = BindableProperty.Create(nameof(HeaderBackgroundColor), typeof(Color), typeof(EZCombo), defaultValueCreator: bindable => Default.Color("ezcombo_header_background"));
        public static readonly BindableProperty HeaderColorProperty = BindableProperty.Create(nameof(HeaderColor), typeof(Color), typeof(EZCombo), defaultValueCreator: bindable => Default.Color("ezcombo_header"));
        public static readonly BindableProperty ItemColorProperty = BindableProperty.Create(nameof(ItemColor), typeof(Color), typeof(EZCombo), defaultValueCreator: bindable => Default.Color("ezcombo_item"));
        public static readonly BindableProperty DetailColorProperty = BindableProperty.Create(nameof(DetailColor), typeof(Color), typeof(EZCombo), defaultValueCreator: bindable => Default.Color("ezcombo_detail"));
        public static readonly BindableProperty ListBorderColorProperty = BindableProperty.Create(nameof(ListBorderColor), typeof(Color), typeof(EZCombo), defaultValueCreator: bindable => Default.Color("ezcombo_list_border"));
        public static readonly BindableProperty ListFillProperty = BindableProperty.Create(nameof(ListFill), typeof(Brush), typeof(EZCombo), defaultValueCreator: bindable => Default.Brush("ezcombo_list_fill"));

        private object initial;
        private string state;
        private bool focused;

        private readonly VerticalStackLayout ComboStack;
        private readonly VerticalStackLayout EntryStack;
        private readonly Entry InternalEntry;
        private readonly Grid EntryTapper;
        private readonly Frame EntryFrame;
        private readonly Frame ComboListViewFrame;
        private readonly ListView ComboListView;
        private readonly Grid ClearButton;

        private EZMaskedEntry MaskBehavior;

        private int flashing = 0;

        private int focus_changed = 0;

        public delegate void OnChange(EZCombo combo, TextChangedEventArgs args);
        public event OnChange OnChanged;

        public delegate void OnReturnPressed(EZCombo combo);
        public event OnReturnPressed OnCompleted;

        public delegate void OnFocused(EZCombo combo);
        public new event OnFocused Focused;

        public delegate void OnUnfocused(EZCombo combo);
        public new event OnFocused Unfocused;

        public delegate void OnSelect(EZCombo combo, object selected);
        public event OnSelect OnItemSelected;

        public ICommand OnClearTap { get; private set; }
        public ICommand OnEntryTap { get; private set; }

        private bool rebuild = true;
        private bool filter = false;
        private bool ignore = false;

        private List<string> triggers = new List<string>() { "ItemsSource", "Group", "Item", "Detail", "Sorted" };
        
        IEnumerable<object> ItemsList;

        public delegate bool OnValidateHandler(EZCombo combo);
        public event OnValidateHandler OnValidate;

        public EZCombo()
        {
            OnClearTap = new Command(Handle_ClearTap);
            OnEntryTap = new Command(Handle_EntryTap);

            ControlTemplate = (ControlTemplate)EZDictionary.Resources["EZComboTemplate"];

            ComboStack = (VerticalStackLayout)GetTemplateChild("EZComboStack");
            EntryStack = (VerticalStackLayout)GetTemplateChild("EZEntryStack");
            ComboListViewFrame = (Frame)GetTemplateChild("EZComboFrame");
            ComboListView = (ListView)GetTemplateChild("EZComboListView");
            EntryFrame = (Frame)GetTemplateChild("EntryFrame");
            InternalEntry = (EZEntryBridge)GetTemplateChild("InternalEntry");
            EntryTapper = (Grid)GetTemplateChild("EntryTapper");
            ClearButton = (Grid)GetTemplateChild("ClearButton");

            var item = new DataTemplate(typeof(TextCell));
            item.SetValue(TextCell.TextColorProperty, ItemColor);
            item.SetValue(TextCell.DetailColorProperty, DetailColor);
            ComboListView.ItemTemplate = item;

            var header = new DataTemplate(typeof(TextCell));
            header.SetValue(TextCell.TextColorProperty, HeaderColor);
            header.SetBinding(TextCell.TextProperty, "Header");
            ComboListView.GroupHeaderTemplate = header;

            InternalEntry.TextChanged += Handle_TextChanged;
            InternalEntry.Completed += Handle_Completed;

            ComboStack.SizeChanged += Handle_ComboResize;
            EntryStack.SizeChanged += Handle_ListResize;

            ComboListView.ItemTapped += Handle_ItemTapped;
        }

        public void ThemeChanged()
        {
            // Entry:

            BackColor = Default.Color("ezentry_back");
            BorderColor = Default.Color("ezentry_border");
            FocusedColor = Default.Color("ezentry_focused");
            FlashColor = Default.Color("ezentry_flash");
            LabelColor = Default.Color("ezentry_label");
            TextColor = Default.Color("ezentry_text");
            PlaceholderColor = Default.Color("ezentry_placeholder");
            IconFill = Default.Brush("ezentry_icon");

            // List:

            HeaderColor = Default.Color("ezcombo_header");
            ItemColor = Default.Color("ezcombo_item");
            DetailColor = Default.Color("ezcombo_detail");
            ListBorderColor = Default.Color("ezcombo_list_border");
            ListFill = Default.Brush("ezcombo_list_fill");

            ComboListView.ItemsSource = null;

            ComboListView.ItemTemplate.SetValue(TextCell.TextColorProperty, ItemColor);
            ComboListView.ItemTemplate.SetValue(TextCell.DetailColorProperty, DetailColor);
            ComboListView.GroupHeaderTemplate.SetValue(TextCell.TextColorProperty, HeaderColor);

            ComboListView.ItemsSource = ItemsList;
            EntryFrame.BackgroundColor = focused ? FocusedColor : BackColor;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            initial = SelectedItem;
        }

        public new void Focus()
        {
            focused = true;            
            ToggleFocus();
            Focused?.Invoke(this);
        }

        public new void Unfocus()
        {
            focused = false;
            ToggleFocus();
            Unfocused?.Invoke(this);
        }

        public void Clear()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                ignore = true;
                Text = null;
            }

            SelectedItem = null;

            OnItemSelected?.Invoke(this, null);
        }

        public bool Modified()
        {
            return SelectedItem != initial;
        }

        public object ToDatabaseValue(object target)
        {
            object result;

            if (!string.IsNullOrWhiteSpace(Key))
            {
                result = SelectedItem?.GetType().GetProperty(Key)?.GetValue(SelectedItem);
            }
            else
            {
                result = SelectedItem?.GetType().GetProperty(Item)?.GetValue(SelectedItem);
            }

            return result;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (triggers.Contains(propertyName)) rebuild = true;

            switch (propertyName)
            {
                case "IsEnabled": Opacity = IsEnabled ? 1.0D : 0.5D; break;
                case "Group": GroupPropertyChanged(); break;
                case "Item": ItemPropertyChanged(); break;
                case "RawValue": RawValuePropertyChanged(); break;
                case "Detail": DetailPropertyChanged(); break;
                case "Label": OnPropertyChanged(nameof(HasLabel)); break;

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

        private void RawValuePropertyChanged()
        {
            if (!string.IsNullOrWhiteSpace(Key))
            {
                SelectByProperty(Key, RawValue);
                return;
            }

            SelectByProperty(Item, RawValue);
        }

        private void GroupPropertyChanged()
        {
            ComboListView.IsGroupingEnabled = !string.IsNullOrWhiteSpace(Group);
        }

        private void ItemPropertyChanged()
        {
            ComboListView.ItemTemplate.Bindings.Remove(TextCell.TextProperty);

            if (!string.IsNullOrWhiteSpace(Item))
            {
                ComboListView.ItemTemplate.SetBinding(TextCell.TextProperty, Item);
            }
        }

        private void DetailPropertyChanged()
        {
            ComboListView.ItemTemplate.Bindings.Remove(TextCell.DetailProperty);

            if (!string.IsNullOrWhiteSpace(Detail))
            {
                ComboListView.ItemTemplate.SetBinding(TextCell.DetailProperty, Detail);
            }
        }

        public object SelectedItem { get; private set; }

        public new bool IsFocused
        {
            get
            {
                return focused;
            }
        }

        public int SelectedIndex
        {
            get
            {
                int index = -1;

                if ((ItemsSource != null) && (SelectedItem != null))
                {
                    int i = 0;

                    foreach(object obj in ItemsSource)
                    {
                        if (SelectedItem == obj)
                        {
                            index = i;
                            break;
                        }
                        i++;
                    }
                }

                return index;
            }
        }

        public double ListHeight
        {
            get
            {
                double stack_height = 0;

                if (EntryStack != null)
                {
                    stack_height = EntryStack.Height;
                }

                return (EZApp.Container.Height / 2) - EZApp.Container.TopOffset - stack_height;
            }
        }

        public bool Detached { get; set; }

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

        public IEnumerable<object> ItemsSource
        {
            get => (IEnumerable<object>)GetValue(ItemsSourceProperty);

            set
            {
                SelectedItem = null;
                SetValue(ItemsSourceProperty, value);
            }
        }

        public object RawValue
        {
            get => GetValue(RawValueProperty);
            set => SetValue(RawValueProperty, value);
        }

        public string Key
        {
            get => (string)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public string Item
        {
            get => (string)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public string Detail
        {
            get => (string)GetValue(DetailProperty);
            set => SetValue(DetailProperty, value);
        }

        public string Group
        {
            get => (string)GetValue(GroupProperty);
            set => SetValue(GroupProperty, value);
        }

        public bool Sorted
        {
            get => (bool)GetValue(SortedProperty);
            set => SetValue(SortedProperty, value);
        }

        public Color HeaderBackgroundColor
        {
            get => (Color)GetValue(HeaderBackgroundColorProperty);
            set => SetValue(HeaderBackgroundColorProperty, value);
        }

        public Color HeaderColor
        {
            get => (Color)GetValue(HeaderColorProperty);
            set => SetValue(HeaderColorProperty, value);
        }

        public Color ItemColor
        {
            get => (Color)GetValue(ItemColorProperty);
            set => SetValue(ItemColorProperty, value);
        }

        public Color DetailColor
        {
            get => (Color)GetValue(DetailColorProperty);
            set => SetValue(DetailColorProperty, value);
        }

        public Color ListBorderColor
        {
            get => (Color)GetValue(ListBorderColorProperty);
            set => SetValue(ListBorderColorProperty, value);
        }

        public Brush ListFill
        {
            get => (Brush)GetValue(ListFillProperty);
            set => SetValue(ListFillProperty, value);
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

        public string RawText
        {
            get => EZText.RemoveMask(Text, Mask);
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
                    EntryFrame.BackgroundColor = focused ? FocusedColor : BackColor;
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

        public void Select(object item)
        {
            if ((ItemsSource == null) || (SelectedItem == item)) return;

            if (item != null)
            {
                foreach(object obj in ItemsSource)
                {
                    if (obj == item)
                    {
                        SelectedItem = item;
                        ignore = true;
                        Text = item.GetType().GetProperty(Item)?.GetValue(item)?.ToString();
                        OnItemSelected?.Invoke(this, SelectedItem);
                        break;
                    }
                }
                return;
            }

            Clear();
        }

        public void SelectByIndex(int index)
        {
            if (ItemsSource == null) return;

            if (index < 0)
            {
                Clear();
                return;
            }

            int i = 0;

            foreach(object obj in ItemsSource)
            {
                if (i == index)
                {
                    Select(obj);
                    break;
                }
                i++;
            }
        }

        public void SelectByKey(object key)
        {
            SelectByProperty(Key, key);
        }

        public void SelectByItem(object item)
        {
            SelectByProperty(Item, item);
        }

        public void SelectByDetail(object detail)
        {
            SelectByProperty(Detail, detail);
        }

        private void SelectByProperty(string property, object target)
        {
            if ((ItemsSource == null) || string.IsNullOrWhiteSpace(property)) return;

            if (target == null)
            {
                Clear();
                return;
            }

            foreach (object obj in ItemsSource)
            {
                if (obj.GetType().GetProperty(property)?.GetValue(obj)?.ToString() == target.ToString())
                {
                    Select(obj);
                    return;
                }
            }

            Clear();
        }

        [AsyncVoidOnPurpose]
        private async void ToggleFocus()
        {
            if (focused) // Focus gain
            {
                Interlocked.Exchange(ref focus_changed, 1);

                BuildList();

                InternalEntry.Margin = new Thickness(10, 0, 32, 0);
                EntryFrame.BackgroundColor = FocusedColor;
                ClearButton.IsVisible = true;

                await EZApp.Container.Resizing.WaitAsync();
                {
                    ComboListViewFrame.HeightRequest = ListHeight;
                    ComboListViewFrame.IsVisible = true;
                    await Task.Delay(100);
                    EZApp.Container.Resizing.Release();
                }

                EntryTapper.IsVisible = false;

                return;
            }

            // Focus lost

            ForceSelection();

            Interlocked.Exchange(ref focus_changed, 1);

            EntryFrame.BackgroundColor = BackColor;
            ClearButton.IsVisible = false;
            InternalEntry.Margin = new Thickness(10, 0, 10, 0);

            await EZApp.Container.Resizing.WaitAsync();
            {
                ComboListViewFrame.IsVisible = false;
                await Task.Delay(100);
                EZApp.Container.Resizing.Release();
            }

            EntryTapper.IsVisible = true;
        }

        [ComponentEventHandler]
        private void Handle_ComboResize(object sender, EventArgs e)
        {
            if (1 == Interlocked.Exchange(ref focus_changed, 0))
            {
                System.Diagnostics.Debug.WriteLine($"EZCombo Resized: {(focused ? "↓" : "↑")}");

                EZApp.Container.HandleFocus(this, focused);
            }
        }

        [ComponentEventHandler]
        private void Handle_ListResize(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ListHeight));
        }

        [ComponentEventHandler]
        private void Handle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignore)
            {
                ignore = false;
                return;
            }

            rebuild = true;
            filter = !string.IsNullOrEmpty(Text);

            BuildList();

            OnChanged?.Invoke(this, e);
        }

        [ComponentEventHandler]
        private void Handle_Completed(object sender, EventArgs e)
        {
            Unfocus();

            OnCompleted?.Invoke(this);
        }

        [ComponentEventHandler]
        private void Handle_ClearTap()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                Clear();
            }
        }

        [ComponentEventHandler]
        private void Handle_EntryTap()
        {
            if (focused) return; Focus();
        }

        [ComponentEventHandler]
        public void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Select(e.Item); Unfocus();
        }

        private void BuildList()
        {
            if (!rebuild) return;

            if (string.IsNullOrWhiteSpace(Group))
            {
                ItemsList = BuildSimpleList();
            }
            else
            {
                ItemsList = BuildGroupedList();
            }

            ComboListView.ItemsSource = ItemsList;

            rebuild = filter;
            filter = false;
        }

        private List<object> BuildSimpleList()
        {
            if (ItemsSource == null) return null;

            string content = filter ? EZText.RemoveDiacritics(InternalEntry.Text?.ToUpper()) : null;

            List<object> items = new List<object>();

            foreach(object obj in ItemsSource)
            {
                string item = EZText.RemoveDiacritics(obj.GetType().GetProperty(Item)?.GetValue(obj)?.ToString()?.ToUpper());

                if (string.IsNullOrEmpty(content) || (item.IndexOf(content) != -1))
                {
                    items.Add(obj);
                }                
            }

            if (Sorted)
            {
                items.Sort(Compare);
            }

            return items;
        }

        private List<ComboGroup> BuildGroupedList()
        {
            if (ItemsSource == null) return null;

            string content = filter ? EZText.RemoveDiacritics(InternalEntry.Text?.ToUpper()) : null;

            List<string> headers = new List<string>();

            foreach (object obj in ItemsSource)
            {
                string header = obj.GetType().GetProperty(Group)?.GetValue(obj)?.ToString();

                if (header == null) header = "";

                if (!headers.Contains(header))
                {
                    headers.Add(header);
                }
            }

            if (Sorted) headers.Sort((x, y) => x.CompareTo(y));

            List<ComboGroup> groups = new List<ComboGroup>();

            foreach (string header in headers)
            {
                ComboGroup items = new ComboGroup() { Header = header };

                foreach(object obj in ItemsSource)
                {
                    string h = obj.GetType().GetProperty(Group)?.GetValue(obj)?.ToString();

                    string item = EZText.RemoveDiacritics(obj.GetType().GetProperty(Item)?.GetValue(obj)?.ToString()?.ToUpper());

                    if (h == header && (string.IsNullOrEmpty(content) || (item.IndexOf(content) != -1)))
                    {
                        items.Add(obj);
                    }
                }

                if (Sorted)
                {
                    items.Sort(Compare);
                }

                if (items.Count > 0)
                {
                    groups.Add(items);
                }
            }

            return groups;
        }

        private int Compare(object x, object y)
        {
            int result = 0;

            try
            {
                IComparable a = (IComparable)x.GetType().GetProperty(Item)?.GetValue(x);
                IComparable b = (IComparable)y.GetType().GetProperty(Item)?.GetValue(y);

                result = a.CompareTo(b);
            }
            catch { /* Dismiss */ }

            return result;
        }

        private void ForceSelection()
        {
            if (string.IsNullOrWhiteSpace(Item) || string.IsNullOrWhiteSpace(Text)) return;

            string compare = Text?.ToUpper() ?? "";

            if (SelectedItem != null)
            {
                string text = SelectedItem.GetType().GetProperty(Item)?.GetValue(SelectedItem)?.ToString().ToUpper();

                if (compare == text) return; // User selected the same item.
            }

            object item = null;

            if (ComboListView.IsGroupingEnabled)
            {
                List<ComboGroup> groups = (List<ComboGroup>)ComboListView.ItemsSource;

                if ((groups != null) && (groups.Count > 0) && (groups[0] != null) && (groups[0].Count > 0))
                {
                    item = groups[0][0];
                }
            }
            else
            {
                List<object> simple = (List<object>)ComboListView.ItemsSource;

                if ((simple != null) && (simple.Count > 0))
                {
                    item = simple[0];
                }
            }

            if (item == null)
            {
                Clear();
                return;
            }

            string item_text = item.GetType().GetProperty(Item)?.GetValue(item)?.ToString().ToUpper();

            if (item_text.IndexOf(compare) == -1)
            {
                Clear();
                return;
            }

            if (item != SelectedItem) Select(item);
        }
    }
}