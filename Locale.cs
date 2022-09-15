using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunCat
{
    public class Locale
    {
        public static bool isZH 
        { 
            get
            {
                return System.Globalization.CultureInfo.InstalledUICulture.Name.ToLower().Contains("zh");
            }
        }

        public static string Runner 
        {
            get
            {
                return isZH ? "图标" : "Runner";
            }
        }

        public static string Theme
        {
            get
            {
                return isZH ? "主题" : "Theme";
            }
        }

        public static string Default
        {
            get
            {
                return isZH ? "默认" : "Default";
            }
        }

        public static string Light
        {
            get
            {
                return isZH ? "浅色" : "Light";
            }
        }

        public static string Dark
        {
            get
            {
                return isZH ? "深色" : "Dark";
            }
        }

        public static string Performance
        {
            get
            {
                return isZH ? "显示" : "Performance";
            }
        }

        public static string CPU
        {
            get
            {
                return isZH ? "CPU" : "CPU";
            }
        }

        public static string Memory
        {
            get
            {
                return isZH ? "内存" : "Memory";
            }
        }

        public static string Network
        {
            get
            {
                return isZH ? "网络" : "Network";
            }
        }

        public static string Temperature
        {
            get
            {
                return isZH ? "温度" : "Temperature";
            }
        }

        public static string Startup
        {
            get
            {
                return isZH ? "开机启动" : "Startup";
            }
        }

        public static string Exit
        {
            get
            {
                return isZH ? "退出" : "Startup";
            }
        }
    }
}
