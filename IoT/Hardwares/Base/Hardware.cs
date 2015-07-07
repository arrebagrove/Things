using IoT.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using static Windows.ApplicationModel.Resources.Core.ResourceContext;

namespace IoT.Hardwares.Base
{
    public class Hardware : INotifyPropertyChanged
    {
        #region Constructor
        public Hardware()
        {

        }
        #endregion

        #region GPIO Controller
        private static GpioController gpiocontroller;
        public GpioController GpioController
        {
            get
            {
                return gpiocontroller;
            }
            set
            {
                if (gpiocontroller != value)
                {
                    gpiocontroller = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool InitializeGpioController()
        {
            if (!IsIoT)
                return false;

            if (gpiocontroller == null)
                GpioController = GpioController.GetDefault();

            if (gpiocontroller == null) //Nullize the gpios when faulty
            {
                if (Gpios != null)
                {
                    for (int i = 0; i < Gpios.Count(); i++)
                    {
                        Gpios[i] = null;
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool InitializeGpioController(params int[] ports)
        {
            if(InitializeGpioController())
            {
                return InitializeGpios(ports);
            }
            return false;
        }
        #endregion

        #region GPIOs
        public static List<GpioPin> Gpios;
        public bool InitializeGpios(params int[] ports)
        {
            if(GpioController ==null)
            {
                throw new Exception("Initialize GpioController First");
            }

            if (ports == null)
            {
                throw new Exception("Initialize at least one Gpio port");
            }

            if (Gpios == null)
            {
                Gpios = new List<GpioPin>();
            }

            foreach(var port in ports)
            {
                Gpios.Add(GpioController.OpenPin(port));
                if (Gpios.Last() == null)
                    return false;
            }

            return true;
        }

        #endregion

        #region I2C
        private static I2cDevice i2ccontroller;
        public I2cDevice I2cController
        {
            get { return i2ccontroller; }
            set
            {
                if(i2ccontroller != value)
                {
                    i2ccontroller = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public async Task<bool> InitializeI2CController(int childaddress, bool fast = false)
        {
            if (!IsIoT)
                return false;

            var dis = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector());

            if (!dis.Any())
                return false;

            I2cController = await I2cDevice.FromIdAsync(dis.First().Id, new I2cConnectionSettings(childaddress)
            {
                 BusSpeed = fast ? I2cBusSpeed.FastMode : I2cBusSpeed.StandardMode
            });

            if (I2cController == null) // faulty
                return false;

            return true;
        }

        public static String CreateI2CMessage(string command, Dictionary<string, string> parameters)
        {
            var stamp = Hardware.GetSeconds();
            parameters.Add("dt", stamp);

            StringBuilder message = new StringBuilder($"{command}?");

            foreach (var parameter in parameters)
            {
                message.Append($"{parameter.Key}={parameter.Value}&");
            }

            return String.Concat((char)0x02, message.ToString().TrimEnd('&'), (char)0x03); // STX [message] ETX
        }

        public static async Task<I2CMessageStatus> SendI2CMessage(string message)
        {
            if(i2ccontroller == null)
            {
                throw (new Exception("Initialize I2cController first"));
            }

            #region Write
            try
            {
                if (message.Length < 32)
                {
                    i2ccontroller.Write(Hardware.CreateBuffer(message));
                }
                else
                {
                    var packages = from index in Enumerable.Range(0, message.Length)
                                   group message[index] by index / 32;

                    foreach (var package in packages)
                    {
                        i2ccontroller.WritePartial(Hardware.CreateBuffer(package.ToArray()));
                    }
                }
            }
            catch
            {
                return I2CMessageStatus.NotSent;
            }
            #endregion

            #region Acknowledge
            try
            {
                byte[] readbuffer = new byte[100];
                await Task.Delay(50);

                i2ccontroller.Read(readbuffer);

                if (Hardware.CheckAcknowledge(readbuffer, message))
                    return I2CMessageStatus.Acknowledge;

                return I2CMessageStatus.NotAcknowledge;
            }
            catch
            {
                return I2CMessageStatus.Sent;
            }
            #endregion
        }

        public static async Task<String> ReadI2CMessage(string message)
        {
            if (i2ccontroller == null)
            {
                throw (new Exception("Initialize I2cController first"));
            }

            #region Write
            try
            {
                if (message.Length < 32)
                {
                    i2ccontroller.Write(Hardware.CreateBuffer(message));
                }
                else
                {
                    var packages = from index in Enumerable.Range(0, message.Length)
                                   group message[index] by index / 32;

                    foreach (var package in packages)
                    {
                        i2ccontroller.WritePartial(Hardware.CreateBuffer(package.ToArray()));
                    }
                }
            }
            catch
            {
                return null;
            }
            #endregion

            #region Read
            try
            {
                byte[] readbuffer = new byte[100];
                await Task.Delay(50);

                i2ccontroller.Read(readbuffer);

                return DecodeBuffer(readbuffer);
            }
            catch
            {
                return null;
            }
            #endregion
        }
        #endregion

        #region string operations
        public static byte[] CreateBuffer(string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public static byte[] CreateBuffer(char[] input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public static String DecodeBuffer(byte[] input)
        {
            char remove = Encoding.UTF8.GetChars(new byte[] { 255 }).First();

            return Encoding.UTF8.GetString(input,0,input.Length).TrimEnd(remove);
        }

        public static bool CheckAcknowledge(byte[] input, string message)
        {
            return (message.Contains(DecodeBuffer(input)));
        }
        #endregion

        #region DateTime Stamp
        public static String GetSeconds()
        {
            return DateTime.Now.ToString("ssfff");
        }

        public static String GetTime()
        {
            return DateTime.Now.ToString("hhmmss");
        }

        public static String GetDateTime()
        {
            return DateTime.Now.ToString("ddMMyyyy_hhmmss");
        }

        public static String GetDate()
        {
            return DateTime.Now.ToString("ddMMyyyy");
        }
        #endregion

        #region Qualifiers
        public static bool IsIoT
        {
            get
            {
                var qualifier = GetForCurrentView().QualifierValues["DeviceFamily"];
                return qualifier == "Universal" || qualifier == "IoT";
            }
        }
        #endregion

        #region NotifyPropertyChanged
        public void NotifyPropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
