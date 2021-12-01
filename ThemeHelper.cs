using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace RunCat
{
    public class ThemeHelper
    {
        private const string _registryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string _registryValueName = "AppsUseLightTheme";


        public static WindowsTheme GetWindowsTheme()
        {
            object registryValueObject;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(_registryKeyPath))
            {
                registryValueObject = key?.GetValue(_registryValueName);
                if (registryValueObject != null)
                {
                    int registryValue = (int)registryValueObject;
                    return registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
                }
            }
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(_registryKeyPath))
            {
                registryValueObject = key?.GetValue(_registryValueName);
                if (registryValueObject != null)
                {
                    int registryValue = (int)registryValueObject;
                    return registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
                }
            }
            return WindowsTheme.Light;
        }
    }

    public static class MicaHelper
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref int pvAttribute, int cbAttribute);

        [Flags]
        private enum DwmWindowAttribute : uint
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_MICA_EFFECT = 1029
        }

        public static void UpdateStyleAttributes(this MainWindow window)
        {
            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            var darkThemeEnabled = ThemeHelper.GetWindowsTheme();

            int trueValue = 0x01;
            int falseValue = 0x00;

            if (darkThemeEnabled == WindowsTheme.Dark)
            {
                _ = DwmSetWindowAttribute(windowHandle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(int)));
            }
            else
            {
                _ = DwmSetWindowAttribute(windowHandle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref falseValue, Marshal.SizeOf(typeof(int)));
            }

            _ = DwmSetWindowAttribute(windowHandle, DwmWindowAttribute.DWMWA_MICA_EFFECT, ref trueValue, Marshal.SizeOf(typeof(int)));
        }
    }

    public enum WindowsTheme
    {
        Light,
        Dark
    }

    public enum RunnerIcon
    {
        Cat,
        Parrot
    }

    public enum PerformanceType
    {
        CPU,
        Memory,
        NetWork,
        Temperature
    }
}
