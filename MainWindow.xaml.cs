using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace RunCat
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int CPU_TIMER_DEFAULT_INTERVAL = 3000;
        private const int ANIMATE_TIMER_DEFAULT_INTERVAL = 200;
        private readonly int MemoryTotleMbytes = Settings.GetPhisicalMemory();
        private readonly Settings settings = Settings.ReadSetting();
        private readonly Timer animateTimer = new Timer();
        private readonly Timer cpuTimer = new Timer();
        private PerformanceCounter cpuUsage;
        private PerformanceCounter memoryAvailable;
        private PerformanceCounter networkTotal = null;
        private PerformanceCounter temperatureUsage = null;
        private Hardware hardware = null;
        private MenuItem runnerMenu;
        private MenuItem themeMenu;
        private MenuItem performanceMenu;
        private MenuItem symmetryMenu;
        private MenuItem startupMenu;
        private NotifyIcon notifyIcon;
        private int current = 0;
        private WindowsTheme systemTheme = ThemeHelper.GetWindowsTheme();
        private Icon[] icons;
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (hardware != null) hardware.Dispose();
            notifyIcon.Dispose();
        }

        private string PerformanceInstanceName(string categoryName, string counterName = "")
        {
            PerformanceCounterCategory performanceCounterCategory = new PerformanceCounterCategory(categoryName);
            if(performanceCounterCategory != null)
            {
                var instances = performanceCounterCategory.GetInstanceNames();
                foreach (string instanceName in instances)
                {

                    foreach (PerformanceCounter counter in performanceCounterCategory.GetCounters(instanceName))
                    {
                        if (string.IsNullOrWhiteSpace(counterName) || counter.CounterName.Contains(counterName))
                        {
                            return counter.InstanceName;
                        }
                    }
                }
            }
            return string.Empty;
        }

        private void Init()
        {
            cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            memoryAvailable = new PerformanceCounter("Memory", "Available MBytes");
            _ = cpuUsage.NextValue();
            _ = memoryAvailable.NextValue();
            string networkInstanceName = PerformanceInstanceName("Network Interface", "Bytes Total/sec");//"Bytes Received/sec"
            if (!string.IsNullOrWhiteSpace(networkInstanceName))
            {
                networkTotal = new PerformanceCounter("Network Interface", "Bytes Received/sec", networkInstanceName);
                _ = networkTotal.NextValue(); // discards first return value
            }
            string temperatureInstanceName = PerformanceInstanceName("Thermal Zone Information", "Temperature");
            if (!string.IsNullOrWhiteSpace(temperatureInstanceName))
            {
                temperatureUsage = new PerformanceCounter("Thermal Zone Information", "Temperature", temperatureInstanceName);
                _ = temperatureUsage.NextValue(); // discards first return value
            }
            else if (Hardware.IsRunAsAdmin())
            {
                hardware = new Hardware();
                if (!hardware.CheckTemperature())
                {
                    hardware.Dispose();
                    hardware = null;
                }
            }

            runnerMenu = new MenuItem("Runner", new MenuItem[]
            {
                new MenuItem("Cat", SetRunner)
                {
                    Checked = settings.Runner == RunnerIcon.Cat,
                    Tag = RunnerIcon.Cat
                },
                new MenuItem("Parrot", SetRunner)
                {
                    Checked = settings.Runner == RunnerIcon.Parrot,
                    Tag = RunnerIcon.Parrot
                }
            });
            RunnerIcon instance = new RunnerIcon();
            System.Reflection.FieldInfo[] properties = instance.GetType().GetFields();
            foreach (System.Reflection.FieldInfo item in properties)
            {
                string name = item.Name;
                if (name == "Cat" || name == "Parrot") continue;
                string value = (string)item.GetValue(instance);
                runnerMenu.MenuItems.Add(new MenuItem(name, SetRunner)
                {
                    Checked = settings.Runner == value,
                    Tag = value
                });
            }
            runnerMenu.Popup += RunnerMenu_Popup;

            themeMenu = new MenuItem("Theme", new MenuItem[]
            {
                new MenuItem("Default", SetTheme)
                {
                    Checked = settings.CustomTheme == WindowsTheme.Default,
                    Tag = WindowsTheme.Default
                },
                new MenuItem("Light", SetTheme)
                {
                    Checked = settings.CustomTheme == WindowsTheme.Light,
                    Tag = WindowsTheme.Light
                },
                new MenuItem("Dark", SetTheme)
                {
                    Checked = settings.CustomTheme == WindowsTheme.Dark,
                    Tag = WindowsTheme.Dark
                }
            });
            if (temperatureUsage == null && hardware == null && settings.Performance == PerformanceType.Temperature)
            {
                settings.Performance = PerformanceType.CPU;
            }
            performanceMenu = new MenuItem("Performance", new MenuItem[]
            {
                new MenuItem("CPU", SetPerformance)
                {
                    Checked = settings.Performance == PerformanceType.CPU,
                    Tag = PerformanceType.CPU
                },
                new MenuItem("Memory", SetPerformance)
                {
                    Checked = settings.Performance == PerformanceType.Memory,
                    Tag = PerformanceType.Memory
                }
            });
            if (networkTotal != null)
            {
                performanceMenu.MenuItems.Add(new MenuItem("Network", SetPerformance)
                {
                    Checked = settings.Performance == PerformanceType.NetWork,
                    Tag = PerformanceType.NetWork
                });
            }
            if (temperatureUsage != null || hardware != null)
            {
                performanceMenu.MenuItems.Add(new MenuItem("Temperature", SetPerformance)
                {
                    Checked = settings.Performance == PerformanceType.Temperature,
                    Tag = PerformanceType.Temperature
                });
            }

            startupMenu = new MenuItem("Startup", OnSetStartup)
            {
                Checked = IsStartupEnabled()
            };

            symmetryMenu = new MenuItem("SymmetryIcon", OnSetSymmetry)
            {
                Checked = settings.SymmetryIcon
            };

            MenuItem[] childen = new MenuItem[] { runnerMenu, themeMenu, performanceMenu, symmetryMenu, startupMenu, new MenuItem("Exit", Exit) };

            notifyIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.appIcon,
                ContextMenu = new ContextMenu(childen),
                Text = "",
                Visible = true,
            };
            SetIcons();
            SetAnimation();
            CPUTick();
            StartObserveCPU();
            current = 1;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool SetMenuInfo(IntPtr hMenu, MENUINFO lpcmi);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool SetMenuItemInfo(IntPtr hMenu, int uItem, bool fByPosition, MENUITEMINFO lpmii);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MENUINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MENUINFO));
            public int fMask = 0x10; //MIM_STYLE
            public int dwStyle = 0x4000000; //MNS_CHECKORBMP
            public uint cyMax;
            public IntPtr hbrBack;
            public int dwContextHelpID;
            public IntPtr dwMenuData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MENUITEMINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MENUITEMINFO));
            public int fMask = 0x80; //MIIM_BITMAP
            public int fType;
            public int fState;
            public int wID;
            public IntPtr hSubMenu;
            public IntPtr hbmpChecked;
            public IntPtr hbmpUnchecked;
            public IntPtr dwItemData;
            public IntPtr dwTypeData;
            public int cch;
            public IntPtr hbmpItem;
        }

        private void RunnerMenu_Popup(object sender, EventArgs e)
        {
            var info = new MENUITEMINFO();
            int i = 0;
            foreach (MenuItem item in ((Menu)sender).MenuItems)
            {

                Icon icon = null;
                try
                {
                    icon = (Icon)Properties.Resources.ResourceManager.GetObject($"{item.Tag as string}_0");
                }
                catch (Exception)
                { }
                if (item.Visible && icon != null)
                {
                    try
                    {
                        info.hbmpItem = icon.ToBitmap().GetHbitmap();
                        SetMenuItemInfo(((Menu)sender).Handle, i, true, info);
                    }
                    catch (Exception)
                    {
                    }
                    i++;
                }
            }
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr wptr = new WindowInteropHelper(this).Handle;
            HwndSource hs = HwndSource.FromHwnd(wptr);
            hs.AddHook(new HwndSourceHook(WndProc));
            Visibility = Visibility.Hidden;
            Hide();
            Init();
        }

        const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x320;
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_DWMCOLORIZATIONCOLORCHANGED:
                    systemTheme = ThemeHelper.GetWindowsTheme();
                    SetIcons();
                    return IntPtr.Zero;
                default:
                    return IntPtr.Zero;
            }
        }

        private void SetAnimation()
        {
            animateTimer.Interval = ANIMATE_TIMER_DEFAULT_INTERVAL;
            animateTimer.Tick += new EventHandler(AnimationTick);
        }

        private void AnimationTick(object sender, EventArgs e)
        {
            if (icons.Length <= current) current = 0;
            notifyIcon.Icon = icons[current];
            current = (current + 1) % icons.Length;
        }

        private float netMaxMB = 1;
        private void CPUTick()
        {
            float c = cpuUsage.NextValue();
            float m = memoryAvailable.NextValue();
            m = 100 - (m * 100 / MemoryTotleMbytes);
            float s = settings.Performance == PerformanceType.Memory ? m : c;
            string text = $"CPU: {c:f1}%\nMem: {m:f1}%";

            if(networkTotal != null)
            {
                float n = networkTotal.NextValue() / 1048576;
                netMaxMB = Math.Max(netMaxMB, n);
                if (settings.Performance == PerformanceType.NetWork) s = n * 100 / netMaxMB;
                if(n < 1)
                {
                    text += $"\nNetwork: {(n * 1024):f1}KB/s";
                }
                else
                {
                    text += $"\nNetwork: {n:f1}MB/s";
                }
            }

            if (temperatureUsage != null)
            {
                float t = temperatureUsage.NextValue() - (float)273.15;
                if (settings.Performance == PerformanceType.Temperature) s = t;
                text += $"\nTemperature: {t:f1}℃";
            }
            else if(hardware != null)
            {
                float t = hardware.GetTemperature();
                if (settings.Performance == PerformanceType.Temperature) s = t;
                text += $"\nTemperature: {t:f1}℃";
            }
            notifyIcon.Text = text;
            s = ANIMATE_TIMER_DEFAULT_INTERVAL / (float)Math.Max(1.0f, Math.Min(20.0f, s / 5.0f));
            animateTimer.Stop();
            animateTimer.Interval = (int)s;
            animateTimer.Start();
        }

        private void ObserveCPUTick(object sender, EventArgs e)
        {
            CPUTick();
        }

        private void StartObserveCPU()
        {
            cpuTimer.Interval = CPU_TIMER_DEFAULT_INTERVAL;
            cpuTimer.Tick += new EventHandler(ObserveCPUTick);
            cpuTimer.Start();
        }

        private void Exit(object sender, EventArgs e)
        {
            Close();
        }

        private void SetPerformance(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            UpdateCheckedState(item, performanceMenu);
            settings.Performance = (PerformanceType)item.Tag;
            settings.Save();
        }

        private void SetRunner(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            UpdateCheckedState(item, runnerMenu);
            settings.Runner = (string)item.Tag;
            SetIcons();
            settings.Save();
        }

        private void SetTheme(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            UpdateCheckedState(item, themeMenu);
            settings.CustomTheme = (WindowsTheme)item.Tag;
            SetIcons();
            settings.Save();
        }

        private void UpdateCheckedState(MenuItem sender, MenuItem menu)
        {
            foreach (MenuItem item in menu.MenuItems)
            {
                item.Checked = false;
            }
            sender.Checked = true;
        }

        private bool IsStartupEnabled()
        {
            string keyName = @"Software\Microsoft\Windows\CurrentVersion\Run";
            using (RegistryKey rKey = Registry.CurrentUser.OpenSubKey(keyName))
            {
                return rKey.GetValue(Settings.AppName) != null;
            }
        }

        private void SetIcons()
        {
            System.Resources.ResourceManager resourceManager = Properties.Resources.ResourceManager;
            List<Icon> list = new List<Icon>();
            var theme = systemTheme;
            if (settings.CustomTheme != WindowsTheme.Default) theme = settings.CustomTheme;
            int i = 0;
            Icon icon = null;
            do
            {
                try
                {
                    icon = (Icon)resourceManager.GetObject(
                        $"{(theme == WindowsTheme.Dark ? "dark_" : "")}" +
                        $"{settings.Runner}_" +
                        $"{i++}");
                    if (icon != null) list.Add(icon);
                }
                catch (Exception)
                { }
            } while (icon != null);
            if (settings.SymmetryIcon)
            for (i = list.Count - 2; i > 0; i--)
            {
                list.Add(list[i]);
            }
            if (list.Count == 0)
            {
                list.Add(Properties.Resources.appIcon);
            }
            icons = list.ToArray();
        }

        private void OnSetStartup(object sender, EventArgs e)
        {
            startupMenu.Checked = !startupMenu.Checked;
            SetStartup(startupMenu.Checked);
            settings.Save();
        }

        private void OnSetSymmetry(object sender, EventArgs e)
        {
            symmetryMenu.Checked = !symmetryMenu.Checked;
            settings.SymmetryIcon = symmetryMenu.Checked;
            SetIcons();
            settings.Save();
        }

        private void SetStartup(bool start)
        {
            string keyName = @"Software\Microsoft\Windows\CurrentVersion\Run";
            using (RegistryKey rKey = Registry.CurrentUser.OpenSubKey(keyName, true))
            {
                if (start)
                {
                    rKey.SetValue(Settings.AppName, Process.GetCurrentProcess().MainModule.FileName);
                }
                else
                {
                    rKey.DeleteValue(Settings.AppName, false);
                }
                rKey.Close();
            }
        }
    }
}
