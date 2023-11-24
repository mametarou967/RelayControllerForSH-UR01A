using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayControllerForSHUR01A.Model.SerialInterfaceProtocol
{
    public class RelayOnCommand : ICommand
    {
        public CommandType CommandType => CommandType.RelayOn;

        public byte[] ByteArray()
        {
            List<byte> data = new List<byte>() { };

            data.AddRange(Encoding.ASCII.GetBytes("AT+CH1=1"));

            return data.ToArray();
        }
    }
}
