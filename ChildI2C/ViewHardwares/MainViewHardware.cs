using IoT.Common;
using IoT.Hardwares.Base;
using IoT.ViewHardwares.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.UI.Xaml;

namespace ChildI2C.ViewHardwares
{
    public class MainViewHardware : ViewHardware
    {
        public async override void Setup()
        {
            if (Hardware.IsIoT && await MainHardware.InitializeI2CController(4,true))
            {
                Loop();
            }
        }

        public GpioPin GpioPort => Hardware.Gpios[0];


        #region Led On/Off
        private bool on = false;
        public  bool On
        {
            get { return on; }
            set
            {
                on = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Servo angle
        int angle = 0;
        int Angle
        {
            get { return angle; }
            set
            {
                angle = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        public async void Loop()
        {
            if(await SetGpio(12, on) == I2CMessageStatus.Acknowledge)
            {
                Ack = true;
            }

            await Task.Delay(200);

            if (await SetServo(9, angle) == I2CMessageStatus.Acknowledge)
            {
                Angle += 60;
                if (angle > 180)
                    Angle = 0;
            }

            await Task.Delay(100);

            if (Ack)
            {
                Read = await GetGpio(12) != null;
            }

            await Task.Delay(1000);

            Read = false;
            Ack = false;
            On = !on;
            Loop();
        }

        #region Check Properties
        private bool ack;
        public bool Ack
        {
            get { return ack; }
            set
            {
                ack = value;
                NotifyPropertyChanged();
            }
        }

        private bool read;
        public bool Read
        {
            get { return read; }
            set
            {
                read = value;
                NotifyPropertyChanged();
            }
        }
        #endregion



        private async Task<I2CMessageStatus> SetGpio(int port, bool state)
        {
            var message = Hardware.CreateI2CMessage("sio", new Dictionary<string, string>()
            {
                ["pt"] = port.ToString(),
                ["st"] = state ? "on" : "off"
            });

            return await Hardware.SendI2CMessage(message);
        }

        private async Task<I2CMessageStatus> SetServo(int port, int angle)
        {
            var message = Hardware.CreateI2CMessage("svo", new Dictionary<string, string>()
            {
                ["pt"] = port.ToString(),
                ["an"] = angle.ToString()
            });

            return await Hardware.SendI2CMessage(message);
        }

        private async Task<Nullable<bool>> GetGpio(int port)
        {
            var message = Hardware.CreateI2CMessage("gio", new Dictionary<string, string>()
            {
                ["pt"] = port.ToString()
            });

            var ret = await Hardware.ReadI2CMessage(message);
            if (ret != null)
            {
                return ret == "on";
            }
            return null;
        }



    }

}
