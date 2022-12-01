using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuicNet.Infrastructure
{
    public enum PacketType : UInt16
    {
        Initial = 0x0,
        ZeroRTTProtected = 0x1,
        Handshake = 0x2,
        RetryPacket = 0x3
    }
}
