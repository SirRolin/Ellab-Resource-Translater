﻿using Newtonsoft.Json;
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
        public int insertersToUse = 8;
        public Size MainWindowSize = new(900, 650);
        public Size SettingWindowSize = new(600, 400);
        public bool closeOnceDone = false;

        // Singleton instanciation accessed with Get()
        private static Config? instance;
        private Config()
        {
            this.languagesToTranslate = defaultLanguages.Select(x => x.Key).ToList();
            this.languagesToAiTranslate = defaultLanguages.Select(x => x.Key).ToList();
            this.languagesToAiTranslate.RemoveAll(x => languagesNotToAi.Contains(x));
        }
        [JsonConstructor]
        public Config(string eMPath, string ValPath, List<string> languagesToTranslate, List<string> languagesToAiTranslate, int threadsToUse, int insertersToUse, Size? MainWindowSize, Size? SettingWindowSize, bool closeOnceDone)
        {
            this.EMPath = eMPath ?? this.EMPath;
            this.ValPath = ValPath ?? this.ValPath;
            this.languagesToTranslate = languagesToTranslate ?? this.languagesToTranslate;
            this.languagesToAiTranslate = languagesToAiTranslate ?? this.languagesToAiTranslate;
            this.threadsToUse = threadsToUse;
            this.insertersToUse = insertersToUse;
            this.MainWindowSize = MainWindowSize ?? new(900, 650);
            this.SettingWindowSize = SettingWindowSize ?? new(600, 400);
            this.closeOnceDone = closeOnceDone;
        }


        public static Config Get()
        {
            // try loading from disc
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
            instance ??= new Config();

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

        public static void AssignSizeSetting<T>(T window, Action<Size> settingSize, Size size) where T : Form
        {
            window.Size = size;
            window.ClientSizeChanged += (s, e) =>
            {
                settingSize.Invoke(window.Size);
            };
        }
    }
}
