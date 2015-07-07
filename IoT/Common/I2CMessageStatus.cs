using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Common
{
    public enum I2CMessageStatus
    {
        NotSent,
        Sent,
        NotAcknowledge,
        Acknowledge
    }
}
