using RelayControllerForSHUR01A.Model.Common;
using System.Collections.Generic;
using System.Text;

namespace RelayControllerForSHUR01A.Model.SerialInterfaceProtocol
{
    public class RelayOffCommand : ICommand
    {
        public CommandType CommandType => CommandType.RelayOff;

        public byte[] ByteArray()
        {
            List<byte> data = new List<byte>() { };

            data.AddRange(Encoding.ASCII.GetBytes("AT+CH1=0"));

            return data.ToArray();
        }
    }
}
