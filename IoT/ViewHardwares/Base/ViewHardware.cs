using IoT.Hardwares.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;

namespace IoT.ViewHardwares.Base
{
    public class ViewHardware : INotifyPropertyChanged
    {
        public ViewHardware()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                MainHardware = new Hardware();
                Setup();
            }
        }

        public virtual void Setup()
        {

        }

        #region MainHardware
        private Hardware mainhardware;
        public Hardware MainHardware
        {
            get { return mainhardware; }
            set
            {
                mainhardware = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public async void NotifyPropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                if (Window.Current != null &&  Window.Current.Content != null && Window.Current.Content.Dispatcher != null)
                {
                    await Window.Current.Content.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(caller));
                    });
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
