using IoT.Hardwares.Base;
using IoT.ViewHardwares.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Blinky.ViewHardwares
{
    public class MainViewHardware : ViewHardware
    {
        #region On
        private bool on;
        public bool On
        {
            get { return on; }
            set { on = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region Delay
        private double delay = 1000;
        public double Delay
        {
            get { return delay; }
            set { delay = Math.Min(int.MaxValue, value); NotifyPropertyChanged(); }
        }
        #endregion

        public override void Setup()
        {
            if (Hardware.IsIoT && MainHardware.InitializeGpioController(18))
            {
                GpioPort.SetDriveMode(GpioPinDriveMode.Output);
                Loop();
            }
        }

        public GpioPin GpioPort => Hardware.Gpios[0];

        public async void Loop()
        {
            On = !On;
            GpioPort.Write(On ? GpioPinValue.High : GpioPinValue.Low);
            await Task.Delay((int)delay);
            Loop();
        }
    }
}
