using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace RunCat
{
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }

        public void VisitParameter(IParameter parameter) { }

        public void VisitSensor(ISensor sensor) { }
    }

    public class Hardware: IDisposable
    {
        private readonly UpdateVisitor updateVisitor = new UpdateVisitor();
        private readonly Computer computer = new Computer();
        public Hardware()
        {
            computer.Open();
            computer.CPUEnabled = true;
            computer.Accept(updateVisitor);
        }

        public void Dispose()
        {
            computer.Close();
        }

        public static bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public bool CheckTemperature()
        {
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public float GetTemperature()
        {
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
                        {
                            computer.Hardware[i].Update();
                            return computer.Hardware[i].Sensors[j].Value ?? 0;
                        }
                    }
                }
            }
            return 0;
        }
    }
}
