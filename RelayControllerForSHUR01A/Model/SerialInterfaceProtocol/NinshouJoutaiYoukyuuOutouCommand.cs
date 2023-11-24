using RelayControllerForSHUR01A.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayControllerForSHUR01A.Model.SerialInterfaceProtocol
{
    public enum NinshouJoutai
    {
        [StringValue("処理なし")]
        Syorinashi = 0,
        [StringValue("処理中")]
        Syorichuu = 1,
        [StringValue("処理完了")]
        Syorikanryou = 2
    }

    public enum NinshouKanryouJoutai
    {
        [StringValue("処理結果なし")]
        NinshouKekkaNashi = 0,
        [StringValue("処理結果OK")]
        NinshouKekkaOk = 1,
        [StringValue("処理結果NG")]
        NinshouKekkaNg = 2
    }

    public enum NinshouKekkaNgShousai
    {
        [StringValue("なし")]
        Nashi = 0,
        [StringValue("認証NG")]
        NinishouNg = 1,
        [StringValue("認証タイムアウト")]
        NinshouTimeout = 2
    }


    public class NinshouJoutaiYoukyuuOutouCommand : Command
    {
        public NinshouJoutai NinshouJoutai { get; private set; }
        public NinshouKanryouJoutai NinshouKanryouJoutai { get; private set; }
        public NinshouKekkaNgShousai NinshouKekkaNgShousai { get; private set; }
        public string Id { get; private set; }

        public override CommandType CommandType => CommandType.NinshouJoutaiYoukyuuOutou;

        protected override byte[] CommandPayloadByteArray
        {
            get
            {
                List<byte> data = new List<byte>();

                data.Add((byte)(0x30 + NinshouJoutai));
                data.Add((byte)(0x30 + NinshouKanryouJoutai));
                data.Add((byte)(0x30 + NinshouKekkaNgShousai));
                data.AddRange(ByteArrayToAsciiArray(SplitIntInto2ByteDigitsArray(Id.Length)));
                data.AddRange(ConvertDigitsToAsciiArray(Id));

                return data.ToArray();
            }
        }

        protected override string CommadString =>
            $"認証状態:{NinshouJoutai.GetStringValue()} " +
            $"認証完了状態:{NinshouKanryouJoutai.GetStringValue()} " +
            $"認証結果NG詳細:{NinshouKekkaNgShousai.GetStringValue()} " +
            $"利用者ID:{Id}";

        public NinshouJoutaiYoukyuuOutouCommand(
            int idTanmatsuAddress,
            NyuutaishitsuHoukou nyuutaishitsuHoukou,
            NinshouJoutai ninshouJoutai,
            NinshouKanryouJoutai ninshouKanryouJoutai,
            NinshouKekkaNgShousai ninshouKekkaNgShousai,
            string id,
            bool bccError = false
            ) : base(idTanmatsuAddress, nyuutaishitsuHoukou, bccError)
        {
            NinshouJoutai = ninshouJoutai;
            NinshouKanryouJoutai = ninshouKanryouJoutai;
            NinshouKekkaNgShousai = ninshouKekkaNgShousai;
            Id = id;
        }
    }
}
