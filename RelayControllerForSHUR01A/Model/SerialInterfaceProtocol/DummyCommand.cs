using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayControllerForSHUR01A.Model.SerialInterfaceProtocol
{
    public class DummyCommand : Command
    {
        public override CommandType CommandType => CommandType.DummyCommand;

        protected override byte[] CommandPayloadByteArray => new byte[] { };

        protected override string CommadString => "";

        public DummyCommand(
            int idTanmatsuAddress,
            NyuutaishitsuHoukou nyuutaishitsuHoukou
        ) : base(idTanmatsuAddress, nyuutaishitsuHoukou)
        {

        }
    }
}
