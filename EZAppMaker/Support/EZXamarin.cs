/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Reflection;
using System.Collections;

using Newtonsoft.Json;

namespace EZAppMaker.Support
{
    public static class EZXamarin
    {

        public static T Clone<T>(this T instance)
        {
            var json = JsonConvert.SerializeObject(instance);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static IEnumerable<T> GetChildren<T>(this Element element) where T : Element
        {
            // -----------------------------------------------------------------------
            // Visual Tree Helper - (C) 2017 Bryan
            // http://www.bryancook.net/2017/03/visualtreehelper-for-xamarinforms.html
            // -----------------------------------------------------------------------

            var properties = element.GetType().GetRuntimeProperties();

            // try to parse the Content property

            var contentProperty = properties.FirstOrDefault(w => w.Name == "Content");

            if (contentProperty != null)
            {
                var content = contentProperty.GetValue(element) as Element;

                if (content != null)
                {
                    if (content is T)
                    {
                        yield return content as T;
                    }
                    foreach (var child in content.GetChildren<T>())
                    {
                        yield return child;
                    }
                }
            }
            else
            {
                // try to parse the Children property

                var childrenProperty = properties.FirstOrDefault(w => w.Name == "Children");

                if (childrenProperty != null)
                {
                    // loop through children

                    IEnumerable children = childrenProperty.GetValue(element) as IEnumerable;

                    foreach (var child in children)
                    {
                        var childVisualElement = child as Element;

                        if (childVisualElement != null)
                        {
                            // return match

                            if (childVisualElement is T)
                            {
                                yield return childVisualElement as T;
                            }

                            // return recursive results of children

                            foreach (var childVisual in childVisualElement.GetChildren<T>())
                            {
                                yield return childVisual;
                            }
                        }
                    }
                }
            }
        }
    }
}