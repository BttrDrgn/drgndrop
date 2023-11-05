using Tomlyn;
using Tomlyn.Model;

namespace drgndrop
{
    public class TOML
    {
        public TomlTable Table { get; set; }

        public TOML(string path)
        {
            if (!File.Exists(path)) return;

            try
            {
                StreamReader reader = new StreamReader(path);
                Table = Toml.ToModel(reader.ReadToEnd());
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public T Get<T>(string header, string key, T failsafe)
        {
            try
            {
                return (T)((TomlTable)Table[header]!)[key];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return failsafe;
            }
        }
    }
}
