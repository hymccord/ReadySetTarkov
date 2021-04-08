using System.IO;
using System.Text.Json;

using ReadySetTarkov.Utility;

namespace ReadySetTarkov.Settings
{
    class AppSettings<T> where T : new()
    {
        private const string DEFAULT_FILENAME = "settings.json";

        private static readonly string s_directoryPath = Constant.ProgramDirectory;

        public void Save(string fileName = DEFAULT_FILENAME)
        {
            var filePath = Path.Combine(s_directoryPath, fileName);
            File.WriteAllText(filePath, JsonSerializer.Serialize(this));
        }

        public static void Save(T settings, string fileName = DEFAULT_FILENAME)
        {
            var filePath = Path.Combine(s_directoryPath, fileName);
            File.WriteAllText(filePath, JsonSerializer.Serialize(settings));
        }

        public static T Load(string fileName = DEFAULT_FILENAME)
        {
            var filePath = Path.Combine(s_directoryPath, fileName);
            T? t = new();
            if (File.Exists(filePath))
                t = JsonSerializer.Deserialize<T>(File.ReadAllText(filePath));

            return t ?? new();
        }
    }
}
