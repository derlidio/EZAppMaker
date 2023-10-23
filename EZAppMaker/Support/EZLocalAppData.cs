/*__ ____  _             
| __|_  / /_\  _ __ _ __ 
| _| / / / _ \| '_ \ '_ \
|___/___/_/ \_\ .__/ .__/
|  \/  |__ _| |_|__|_| _ 
| |\/| / _` | / / -_) '_|
|_|  |_\__,_|_\_\___|_|
 
(C)2022-2023 Derlidio Siqueira - Expoente Zero */

namespace EZAppMaker.Support
{
    public static class EZLocalAppData
    {
        private static readonly string folder;

        static EZLocalAppData()
        {
            folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        public static bool SaveFile(string file, string contents)
        {
            bool success = false;

            try
            {
                string fileName = Path.Combine(folder, file);

                File.WriteAllText(fileName, contents);

                success = true;
            }
            catch { /* Dismiss */ }

            return success;
        }

        public static bool FileExists(string file)
        {
            bool exists = false;

            try
            {
                string fileName = Path.Combine(folder, file);

                exists = File.Exists(fileName);
            }
            catch { /* Dismiss */ }

            return exists;
        }

        public static string LoadFile(string file)
        {
            string content = null;

            try
            {
                string fileName = Path.Combine(folder, file);

                if (File.Exists(fileName))
                {
                    content = File.ReadAllText(fileName);
                }
            }
            catch { /* Dismiss */ }

            return content;
        }

        public static void DeleteFile(string file)
        {
            try
            {
                string fileName = Path.Combine(folder, file);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch { /* Dismiss */}
        }

        public static void DeleteFiles(string pattern)
        {
            try
            {
                string[] files = Directory.GetFiles(folder, pattern);

                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
            catch { /* Dismiss */ }
        }

        public static async Task AppendFile(string file, string lines)
        {
            try
            {
                string fileName = Path.Combine(folder, file);

                using (StreamWriter writer = File.AppendText(fileName))
                {
                    await writer.WriteLineAsync(lines);
                }
            }
            catch { /* Dismiss */ }
        }

        public static string GetPhotosPath()
        {
            string directory = Path.Combine(folder, "ez_photos");

            if (!Directory.Exists(directory))
            {
                try
                {
                    DirectoryInfo info = Directory.CreateDirectory(directory);
                }
                catch { /* Dismiss */ }
            }

            return directory;
        }
    }
}