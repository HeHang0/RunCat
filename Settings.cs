using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RunCat
{
    [Serializable]
    public class Settings
    {
        private static string AppSettingPath
        {
            get
            {
                string roming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appDataPath = Path.Combine(roming, AppName);
                string appSettingPath = Path.Combine(appDataPath, "settings.json");
                if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
                return appSettingPath;
            }
        }

        public static string AppName
        {
            get
            {
                string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                return string.IsNullOrWhiteSpace(appName) ? "RunCat" : appName;
            }
        }

        public bool Save()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(new Settings().GetType());
                string content = string.Empty;
                using (StringWriter writer = new StringWriter())
                {
                    serializer.Serialize(writer, this);
                    content = writer.ToString();
                }
                File.WriteAllText(AppSettingPath, content);
                return true;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                return false;
            }
        }

        public static Settings ReadSetting()
        {
            Settings settings = new Settings();
            if (File.Exists(AppSettingPath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(settings.GetType());
                    using (StreamReader reader = new StreamReader(AppSettingPath))
                    {
                        settings = (Settings)serializer.Deserialize(reader);
                    }
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.ToString());
                }
            }
            return settings;
        }

        public static int GetPhisicalMemory()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
            {
                Query = new SelectQuery("Win32_PhysicalMemory ", "", new string[] { "Capacity" })//设置查询条件 
            };   //用于查询一些如系统信息的管理对象 
            ManagementObjectCollection collection = searcher.Get();   //获取内存容量 
            ManagementObjectCollection.ManagementObjectEnumerator em = collection.GetEnumerator();

            long capacity = 0;
            while (em.MoveNext())
            {
                ManagementBaseObject baseObj = em.Current;
                if (baseObj.Properties["Capacity"].Value != null)
                {
                    try
                    {
                        capacity += long.Parse(baseObj.Properties["Capacity"].Value.ToString());
                    }
                    catch
                    {
                        return 0;
                    }
                }
            }
            return (int)(capacity / 1024 / 1024);
        }

        private string _runner = RunnerIcon.Cat;
        public string Runner
        {
            get { return _runner.ToLower(); }
            set { _runner = value; }
        }
        private PerformanceType _performance = PerformanceType.CPU;
        public PerformanceType Performance
        {
            get { return _performance; }
            set { _performance = value; }
        }
        private WindowsTheme _customTheme = WindowsTheme.Default;
        public WindowsTheme CustomTheme
        {
            get { return _customTheme; }
            set { _customTheme = value; }
        }

    }
}
