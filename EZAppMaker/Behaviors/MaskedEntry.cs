/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using EZAppMaker.Support;

namespace EZAppMaker.Behaviors
{
    public class EZMaskedEntry : Behavior<Entry>
    {
        public EZMaskedEntry(string mask)
        {
            Mask = mask;
        }

        public string Mask { get; set; }

        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            var entry = (Entry)sender;

            string masked = EZText.Mask(entry.Text, Mask);

            if (masked != entry.Text)
            {
                entry.Text = masked;
            }
        }
    }
}