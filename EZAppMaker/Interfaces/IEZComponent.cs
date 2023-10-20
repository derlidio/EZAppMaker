/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using EZAppMaker.Components;

namespace EZAppMaker.Interfaces
{
    public interface IEZComponent
    {
        string ItemId { get; set; }
        bool Detached { get; set; }

        bool Modified();
        void Clear();
        object ToDatabaseValue(object target);
        void StateManager(StateFormAction action);
    }
}