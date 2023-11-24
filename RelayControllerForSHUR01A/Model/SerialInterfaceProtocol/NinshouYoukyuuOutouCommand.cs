using RelayControllerForSHUR01A.Model.Common;
using System.Collections.Generic;

namespace RelayControllerForSHUR01A.Model.SerialInterfaceProtocol
{

    public enum YoukyuuOutouKekka
    {
        [StringValue("要求受理OK")]
        YoukyuuJuriOk = 0,
        [StringValue("要求受理NG")]
        YoukyuuJuriNg = 1
    }

    public enum YoukyuuJuriNgSyousai
    {
        [StringValue("なし")]
        Nashi = 0,
    }

    public class NinshouYoukyuuOutouCommand : Command
    {
        public YoukyuuOutouKekka YoukyuuOutouKekka { get; private set; }
        public YoukyuuJuriNgSyousai YoukyuuJuriNgSyousai { get; private set; }
        public string Id { get; private set; }

        protected override byte[] CommandPayloadByteArray
        {
            get
            {
                List<byte> data = new List<byte>();

                data.Add((byte)(0x30 + YoukyuuOutouKekka));
                data.Add((byte)(0x30 + YoukyuuJuriNgSyousai)); // 仮固定
                data.AddRange(ByteArrayToAsciiArray(SplitIntInto2ByteDigitsArray(Id.Length)));
                data.AddRange(ConvertDigitsToAsciiArray(Id));

                return data.ToArray();
            }
        }

        public override CommandType CommandType => CommandType.NinshouYoukyuuOutou;

        protected override string CommadString =>
            $"要求応答結果:{YoukyuuOutouKekka.GetStringValue()} " +
            $"要求受理NG詳細:{YoukyuuJuriNgSyousai.GetStringValue()} " +
            $"利用者ID:{Id}";

        public NinshouYoukyuuOutouCommand
        (
            int idTanmatsuAddress,
            NyuutaishitsuHoukou nyuutaishitsuHoukou,
            YoukyuuOutouKekka youkyuuOutouKekka,
            YoukyuuJuriNgSyousai youkyuuJuriNgSyousai,
            string id,
            bool bccError = false
        ) : base(idTanmatsuAddress, nyuutaishitsuHoukou, bccError)
        {
            YoukyuuOutouKekka = youkyuuOutouKekka;
            YoukyuuJuriNgSyousai = youkyuuJuriNgSyousai;
            Id = id;
        }
    }
}
