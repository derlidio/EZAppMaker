﻿/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System;

namespace EZAppMaker.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]

    public class NavigationEventHandlerAttribute: Attribute
    {
        public NavigationEventHandlerAttribute()
        {

        }
    }
}