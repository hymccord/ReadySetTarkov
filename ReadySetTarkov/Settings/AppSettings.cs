using System.IO;
using System.Text.Json;

namespace ReadySetTarkov.Settings
{

    class AppSettings<T> where T : new()
    {
        private const string DEFAULT_FILENAME = "settings.json";

        public void Save(string filename = DEFAULT_FILENAME)
        {
            File.WriteAllText(filename, JsonSerializer.Serialize(this));
        }

        public static void Save(T settings, string fileName = DEFAULT_FILENAME)
        {
            File.WriteAllText(fileName, JsonSerializer.Serialize(settings));
        }

        public static T Load(string fileName = DEFAULT_FILENAME)
        {
            T? t = new();
            if (File.Exists(fileName))
                t = JsonSerializer.Deserialize<T>(File.ReadAllText(fileName));

            return t ?? new();
        }
    }
}
