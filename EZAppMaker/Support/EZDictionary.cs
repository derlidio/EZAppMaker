/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using EZAppMaker.Dictionary;

namespace EZAppMaker.Support
{
    public static partial class EZDictionary
    {
        private static readonly View view;

        static EZDictionary()
        {
            view = new EZDictionaryView();

            Application.Current.Resources.Add(view.Resources);
        }

        public static ResourceDictionary Resources
        {
            get { return view.Resources; }
        }
    }
}