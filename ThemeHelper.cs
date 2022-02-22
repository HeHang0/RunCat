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
        Dark,
        Default
    }

    public class RunnerIcon
    {
        public static string Cat = "cat";
        public static string Parrot = "parrot";

        public static string AllRunners = "all_runners";
        public static string Bird = "bird";
        public static string Bonfire = "bonfire";
        public static string Butterfly = "butterfly";
        public static string CatB = "cat_b";
        public static string CatC = "cat_c";
        public static string CatTail = "cat_tail";
        public static string Chameleon = "chameleon";
        public static string Cheetah = "cheetah";
        public static string Chicken = "chicken";
        public static string City = "city";
        public static string Coffee = "coffee";
        public static string Cogwheel = "cogwheel";
        public static string Cradle = "cradle";
        public static string Dinosaur = "dinosaur";
        public static string Dog = "dog";
        public static string Dogeza = "dogeza";
        public static string Dolphin = "dolphin";
        public static string Dots = "dots";
        public static string Dragon = "dragon";
        public static string Drop = "drop";
        public static string Earth = "earth";
        public static string Engine = "engine";
        public static string Factory = "factory";
        public static string Fishman = "fishman";
        public static string FlashCat = "flash_cat";
        public static string Fox = "fox";
        public static string Frog = "frog";
        public static string Frypan = "frypan";
        public static string GamingCat = "gaming_cat";
        public static string Ghost = "ghost";
        public static string GoldenCat = "golden_cat";
        public static string HamsterWheel = "hamster_wheel";
        public static string Hedgehog = "hedgehog";
        public static string Horse = "horse";
        public static string Human = "human";
        public static string JackOLantern = "jack_o_lantern";
        public static string MeteorCat = "meteor_cat";
        public static string Mochi = "mochi";
        public static string MockNyanCat = "mock_nyan_cat";
        public static string Mouse = "mouse";
        public static string Octopus = "octopus";
        public static string Otter = "otter";
        public static string Owl = "owl";
        public static string PartyParrot = "party_parrot";
        public static string PartyPeople = "party_people";
        public static string Pendulum = "pendulum";
        public static string Penguin = "penguin";
        public static string Pig = "pig";
        public static string Pulse = "pulse";
        public static string Puppy = "puppy";
        public static string PushUp = "push_up";
        public static string Rabbit = "rabbit";
        public static string Reactor = "reactor";
        public static string ReindeerSleigh = "reindeer_sleigh";
        public static string Rocket = "rocket";
        public static string RotatingSushi = "rotating_sushi";
        public static string RubberDuck = "rubber_duck";
        public static string Sausage = "sausage";
        public static string SelfMade = "self_made";
        public static string Sheep = "sheep";
        public static string SineCurve = "sine_curve";
        public static string SitUp = "sit_up";
        public static string Slime = "slime";
        public static string Snowman = "snowman";
        public static string Sparkler = "sparkler";
        public static string Squirrel = "squirrel";
        public static string SteamLocomotive = "steam_locomotive";
        public static string Sushi = "sushi";
        public static string TapiocaDrink = "tapioca_drink";
        public static string Terrier = "terrier";
        public static string Whale = "whale";
        public static string WindChime = "wind_chime";
    }

    public enum PerformanceType
    {
        CPU,
        Memory,
        NetWork,
        Temperature
    }
}
