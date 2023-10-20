/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

using System.Reflection;

using Microsoft.Maui.Controls.Shapes;

namespace EZAppMaker.Support
{
    public static class EZEmbedded
    {
        public static string GetJson(string resource)
        {
            string json = null;

            var assembly = GetAssembly(resource);

            if (assembly != null)
            {
                try
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resource))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            json = reader.ReadToEnd();
                        }
                    }
                }
                catch
                { 
                    System.Diagnostics.Debug.WriteLine($"Error loading JSon Resource: {resource}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"JSon Resource do not exist: {resource}");
            }

            return json;
        }

        public static ImageSource GetImage(string resource)
        {
            ImageSource img = null;

            var assembly = GetAssembly(resource);

            if (assembly != null)
            {
                try
                {
                    img = ImageSource.FromResource(resource, assembly);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading Image Resource: {resource}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Image Resource do not exist: {resource}");
            }

            return img;
        }

        public static string GetText(string resource)
        {
            string result = null;

            var assembly = GetAssembly(resource);

            if (assembly != null)
            {
                try
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resource))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading Text Resource: {resource}");

                    result = $"Could not load: {resource}";
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Text Resource do not exist: {resource}");
            }

            return result;
        }

        public static GeometryGroup GetPath(string resource)
        {
            if (string.IsNullOrWhiteSpace(resource)) return null;

            string result = "M12.45 37.65 10.35 35.55 21.9 24 10.35 12.45 12.45 10.35 24 21.9 35.55 10.35 37.65 12.45 26.1 24 37.65 35.55 35.55 37.65 24 26.1Z";

            var assembly = GetAssembly(resource);

            if (assembly != null)
            {
                try
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resource))
                    {
                        if (stream != null)
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                result = reader.ReadToEnd();
                            }
                        }
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading Path Data: {resource}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Path Data do not exist: {resource}");
            }

            result = result.Replace("\r\n", "\n");

            string[] paths = result.Split('\n');

            GeometryGroup g = new GeometryGroup();

            foreach (var p in paths)
            {
                if (!string.IsNullOrWhiteSpace(p))
                {
                    g.Children.Add((Geometry)new PathGeometryConverter().ConvertFromInvariantString(p));
                }
            }

            return g;
        }

        public static HtmlWebViewSource GetHtml(string resource)
        {
            HtmlWebViewSource html = new HtmlWebViewSource();

            var assembly = GetAssembly(resource);

            if (assembly != null)
            {
                try
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resource))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string result = reader.ReadToEnd();

                            html.Html = result;
                        }
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading HTML Resource: {resource}");

                    html.Html = $"Could not load: {resource}";
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"HTML Resource do not exist: {resource}");
            }

            return html;
        }

        public static Stream GetStream(string resource)
        {
            Stream stream = null;

            var assembly = GetAssembly(resource);

            if (assembly != null)
            {
                try
                {
                    stream = assembly.GetManifestResourceStream(resource);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting Stream Resource: {resource}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Stream Resource do not exist: {resource}");
            }

            return stream;
        }

        public static bool DumpToAppFolder(string resource, string file)
        {
            bool ok = true;

            var assembly = GetAssembly(resource);

            if (assembly != null)
            {
                try
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resource))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            byte[] buffer = new byte[stream.Length];

                            stream.Read(buffer, 0, buffer.Length);

                            string f = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), file);

                            if (File.Exists(f)) File.Delete(f);

                            File.WriteAllBytes(f, buffer);
                        }
                    }
                }
                catch
                {
                    ok = false;
                }
            }

            return ok;
        }

        //   ___                         _   
        //  / __|_  _ _ __ _ __  ___ _ _| |_ 
        //  \__ \ || | '_ \ '_ \/ _ \ '_|  _|
        //  |___/\_,_| .__/ .__/\___/_|  \__|
        //           |_|  |_|

        private static Assembly GetAssembly(string resource)
        {
            string assembly = resource.Split('.')[0];

            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach(Assembly a in loadedAssemblies)
            {
                AssemblyName n = a.GetName();

                if (n.Name == assembly)
                {
                    return a;
                }
            }

            return null;
        }
    }
}