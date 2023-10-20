/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using EZAppMaker.Components;
using EZAppMaker.Support;

namespace EZAppMaker
{
    public partial class EZModal : ContentView
    {
        public object Result { get; private set; }

        public EZModal()
        {
            BindingContext = this;

            InitializeComponent();
        }

        public void Show(EZContentView content)
        {
            IsVisible = true;

            EZModalTarget.Add(content);
        }

        public void Hide()
        {
            IsVisible = false;

            EZModalTarget.Clear();
        }

        public void Hide(object result)
        {
            Result = result;

            Hide();
        }
    }
}