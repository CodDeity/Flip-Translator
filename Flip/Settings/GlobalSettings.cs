using Flip.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
namespace Flip.Settings
{
    public class GlobalSettings
    {
        public static GlobalSettings Settings = new GlobalSettings();
        public static string SavePath = Path.Combine(Directory.GetCurrentDirectory(),"Settings.xml");
        public static event EventHandler<GlobalSettings> OnSettingsChanged;
        private GlobalSettings() { }
        public string ProxyHost { get; set; } = "";
        public int ProxyPort { get; set; } = 0;
        public bool AlertNetChange { get; set; } = false;
        public bool StoreSingleWord { get; set; } = true;
        public bool StoreClipData { get; set; } = false;
        public TranslateApi TranslateApi { get; set; } = TranslateApi.Google;
        public bool TryOfflineUse { get; set; } = false;
        public bool ClipInterceptOnStart { get; set; } = false;
        public bool FilterInvalidChar { get; set; } = true;
        
        public static void SaveSettings(GlobalSettings settings)
        {
            File.Delete(SavePath);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(GlobalSettings));
            using(Stream fileStream = File.Open(SavePath,FileMode.OpenOrCreate))
            {
                fileStream.Position = 0;
                using(XmlWriter xmlStream = XmlWriter.Create(fileStream))
                {
                    xmlSerializer.Serialize(xmlStream, settings);
                }
            }
            if(OnSettingsChanged != null)
                OnSettingsChanged(null, settings);
        }
        public static void LoadSettings()
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(GlobalSettings));
                using (Stream fileStream = File.Open(SavePath, FileMode.OpenOrCreate))
                {
                    using (XmlReader xmlStream = XmlReader.Create(fileStream))
                    {
                        GlobalSettings? sg = (GlobalSettings?)xmlSerializer.Deserialize(xmlStream);
                        if (sg != null)
                            Settings = sg;
                    }
                }
            }
            catch(InvalidOperationException)
            {
                MessageBox.Show("New Settings Generated.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                SaveSettings(Settings);
            }
        }
    }
}
