using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayControllerForSHUR01A.Model.SerialInterfaceProtocol
{
    public class NinshouJoutaiYoukyuuCommand : Command
    {
        public override CommandType CommandType => CommandType.NinshouJoutaiYoukyuu;

        protected override byte[] CommandPayloadByteArray => new byte[] { };

        protected override string CommadString => "";

        public NinshouJoutaiYoukyuuCommand(
            int idTanmatsuAddress,
            NyuutaishitsuHoukou nyuutaishitsuHoukou
            ) : base(idTanmatsuAddress, nyuutaishitsuHoukou) { }
    }
}
