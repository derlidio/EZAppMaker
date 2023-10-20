/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using EZAppMaker.Attributes;
using EZAppMaker.Components;
using EZAppMaker.Support;

namespace EZAppMaker.PopUp
{
    public partial class EZQuestionView : EZContentView
    {
        public string Message { get; private set; }

        public EZQuestionView(string message)
        {
            Message = message;

            BindingContext = this;

            InitializeComponent();

            PopUpTitle.Text = EZSettings.AppName;
        }

        [XamlEventHandler]
        private void Answer_OnTap(EZButton button)
        {
            bool answer = button.ItemId == "Yes";

            EZApp.Modal.Hide(answer);
        }
    }
}