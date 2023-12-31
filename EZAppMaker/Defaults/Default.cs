﻿/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using Newtonsoft.Json;

using EZAppMaker.Support;

namespace EZAppMaker.Defaults
{
    public static class Default
    {
        public class EZPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        public class EZStop
        {
            public string Color { get; set; }
            public float Offset { get; set; }
        }

        public class EZGradient
        {
            public EZPoint StartPoint { get; set; }
            public EZPoint EndPoint { get; set; }

            public List<EZStop> Stops { get; set; }
        }

        public class EZBrush
        {
            public string Color { get; set; }
            public EZGradient Gradient { get; set;}
        }

        private static Dictionary<string, EZBrush> brushes;
        private static Dictionary<string, string> localization;

        static Default()
        {
            SetTheme();
            SetLocalization();
        }

        public static void SetTheme()
        {
            string theme = Application.Current.RequestedTheme ==
                           AppTheme.Dark ?
                           "EZAppMaker.Defaults.Data.dark.json" :
                           "EZAppMaker.Defaults.Data.light.json";

            string json = EZEmbedded.GetJson(theme);
            brushes = JsonConvert.DeserializeObject<Dictionary<string, EZBrush>>(json);

            json = EZApp.Builder.BuildTheme(Application.Current.RequestedTheme);

            if (string.IsNullOrWhiteSpace(json)) return;

            Dictionary<string, EZBrush> changes = JsonConvert.DeserializeObject<Dictionary<string, EZBrush>>(json);
            changes ??= new Dictionary<string, EZBrush>();

            foreach(KeyValuePair<string, EZBrush> pair in changes)
            {
                if (brushes.ContainsKey(pair.Key))
                {
                    brushes[pair.Key] = pair.Value;
                }
                else
                {
                    brushes.Add(pair.Key, pair.Value);
                }                
            }
        }

        public static void SetLocalization()
        {
            string json = EZEmbedded.GetJson("EZAppMaker.Defaults.Data.localization.json");
            localization = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            json = EZApp.Builder.BuildLocalization();

            if (string.IsNullOrWhiteSpace(json)) return;

            Dictionary<string, string> changes = null;

            try
            {
                changes = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);                
            }
            catch { /* Dismiss */ }

            changes ??= new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> pair in changes)
            {
                if (localization.ContainsKey(pair.Key))
                {
                    localization[pair.Key] = pair.Value;
                }
                else
                {
                    localization.Add(pair.Key, pair.Value);
                }
            }
        }

        public static Color Color(string id)
        {
            Color color = Colors.Pink;

            if (brushes.ContainsKey(id))
            {
                try
                {
                    color = Microsoft.Maui.Graphics.Color.FromArgb(brushes[id].Color);
                }
                catch { /* Dismiss */ }
            }

            return color;
        }

        public static Brush Brush(string id)
        {
            Brush brush = new SolidColorBrush(Colors.Black);

            if (!brushes.ContainsKey(id)) return brush;

            try
            {
                EZBrush ezbrush = brushes[id];

                if (!string.IsNullOrWhiteSpace(ezbrush.Color))
                {
                    brush = new SolidColorBrush(Microsoft.Maui.Graphics.Color.FromArgb(ezbrush.Color));
                }
                else
                {
                    LinearGradientBrush lgb = new LinearGradientBrush();

                    lgb.StartPoint = new Point()
                    {
                        X = ezbrush.Gradient.StartPoint.X,
                        Y = ezbrush.Gradient.StartPoint.Y
                    };

                    lgb.EndPoint = new Point()
                    {
                        X = ezbrush.Gradient.EndPoint.X,
                        Y = ezbrush.Gradient.EndPoint.Y
                    };

                    GradientStop stop;

                    foreach (EZStop gs in ezbrush.Gradient.Stops)
                    {
                        stop = new GradientStop()
                        {
                            Color = Microsoft.Maui.Graphics.Color.FromArgb(gs.Color),
                            Offset = gs.Offset
                        };

                        lgb.GradientStops.Add(stop);
                    }

                    brush = lgb;
                }                    
            }
            catch { /* Dismiss */ }

            return brush;
        }

        public static string Localization(string id)
        {
            string message = null;

            if (localization.ContainsKey(id))
            {
                message = localization[id];
            }

            return message;
        }

        public static void SetColor(string id, string color)
        {
            if (brushes.ContainsKey(id))
            {
                brushes[id].Color = color;
                return;
            }

            EZBrush brush = new EZBrush() { Color = color };
            
            brushes.Add(id, brush);
        }

        public static void SetBrush(string id, EZBrush brush)
        {
            if (brushes.ContainsKey(id))
            {
                brushes[id] = brush;
                return;
            }

            brushes.Add(id, brush);
        }
    }
}