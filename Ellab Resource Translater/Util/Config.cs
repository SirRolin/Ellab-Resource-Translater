using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;

namespace Ellab_Resource_Translater.Util
{
    public class Config
    {
        // Static variables to help functions
        private static readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ellab/ResourceTranslater/settings.json");
        private static readonly Dictionary<string, string> defaultLanguages = new()
        {
            ["DE"] = "German",
            ["ES"] = "Spanish",
            ["FR"] = "French",
            ["IT"] = "Italian",
            ["JA"] = "Japanese",
            ["KO"] = "Korean",
            ["NL"] = "Netherland",
            //["PL"] = "Polish",
            ["PT"] = "Portugese",
            ["TR"] = "Tyrkish",
            ["ZH"] = "Chinese"
        };
        public static Dictionary<string, string> DefaultLanguages() => defaultLanguages;

        /// <summary>
        /// inputs use the key
        /// </summary>
        private static readonly List<string> languagesNotToAi = [
            "JA"
        ];

        // Local Variables (The once being saved)
        public string EMPath = "";
        public string ValPath = "";
        public List<string> languagesToTranslate = [];
        public List<string> languagesToAiTranslate = [];
        public int threadsToUse = 32;

        // Singleton instanciation accessed with Get()
        private static Config? instance;
        private Config() {
        }

        private Config Setup()
        {
            this.languagesToTranslate = defaultLanguages.Select(x => x.Key).ToList();
            this.languagesToAiTranslate = defaultLanguages.Select(x => x.Key).ToList();
            this.languagesToAiTranslate.RemoveAll(x => languagesNotToAi.Contains(x));
            return this;
        }
        [JsonConstructor]
        public Config(string eMPath, string ValPath, List<string> languagesToTranslate, List<string> languagesToAiTranslate, int threadsToUse)
        {
            this.EMPath = eMPath;
            this.ValPath = ValPath;
            this.languagesToTranslate = languagesToTranslate;
            this.languagesToAiTranslate = languagesToAiTranslate;
            this.threadsToUse = threadsToUse;
        }


        public static Config Get()
        {
            // loading from disc
            if (instance == null & File.Exists(path))
            {
                try
                {
                    instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
                }
                catch (Exception e) {
                    Debug.WriteLine(e);
                } // If the saved config is incompatible with the current version it will throw an error instead of loading
            }
            
            // new setup
            if (instance == null)
            {
                instance = new Config().Setup();
            }

            return instance;
        }

        public static bool Save()
        {
            Debug.WriteLine("path: " + path);

            if(instance == null)
                return false;

            try
            {
                if(!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    var folderPath = Path.GetDirectoryName(path);
                    Debug.WriteLine("folderPath: " + folderPath);
                    if (folderPath != null) 
                        Directory.CreateDirectory(folderPath);
                }
                File.WriteAllText(path, JsonConvert.SerializeObject(instance));

                return true;
            } catch (Exception)
            {
                return false;
            }
        }

    }
}
