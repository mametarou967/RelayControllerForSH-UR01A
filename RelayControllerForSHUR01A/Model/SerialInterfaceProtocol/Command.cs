using RelayControllerForSHUR01A.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelayControllerForSHUR01A.Model.SerialInterfaceProtocol
{
    public enum CommandType
    {
        [StringValue("ダミー")]
        DummyCommand = 0,
        [StringValue("認証要求")]
        NinshouYoukyuu = 1,
        [StringValue("認証要求応答")]
        NinshouYoukyuuOutou = 2,
        [StringValue("認証状態要求")]
        NinshouJoutaiYoukyuu = 3,
        [StringValue("認証状態要求応答")]
        NinshouJoutaiYoukyuuOutou = 4
    }

    public enum NyuutaishitsuHoukou
    {
        [StringValue("入室")]
        Nyuushitsu = 1,
        [StringValue("退室")]
        Taishitsu = 2
    }

    public abstract class Command : ICommand
    {
        public bool BccError { get; private set; }

        public int IdTanmatsuAddress { get; private set; }
        public NyuutaishitsuHoukou NyuutaishitsuHoukou { get; private set; }

        public Command(
            int idTanmatsuAddress,
            NyuutaishitsuHoukou nyuutaishitsuHoukou,
            bool bccError = false
           )
        {
            IdTanmatsuAddress = idTanmatsuAddress;
            NyuutaishitsuHoukou = nyuutaishitsuHoukou;
            BccError = bccError;
        }

        public abstract CommandType CommandType { get; }

        // public override abstract string ToString();

        public override string ToString() => BaseHeaderString + CommadString + BaseFooterString;

        protected abstract string CommadString { get; }

        private string BaseHeaderString =>
            Common.Common.PaddingInBytes($"CMD: {CommandType.GetStringValue()}", PadType.Char, 36) +
            $"ID端末ｱﾄﾞﾚｽ:{IdTanmatsuAddress} " +
            $"入退室方向:{NyuutaishitsuHoukou.GetStringValue()} ";

        private string BaseFooterString =>
            $" BCCｴﾗｰ:{BccError}";


        protected abstract byte[] CommandPayloadByteArray { get; }

        public byte[] ByteArray()
        {

            List<byte> data_tmp1 = new List<byte>();

            var idTanmatsuAddress = IdTanmatsuAddress;
            data_tmp1.AddRange(ByteArrayToAsciiArray(SplitIntInto2ByteDigitsArray(idTanmatsuAddress)));

            var nyuutaishitsuHoukou = NyuutaishitsuHoukou;
            data_tmp1.Add((byte)(0x30 + nyuutaishitsuHoukou));
            data_tmp1.AddRange(ByteArrayToAsciiArray(SplitIntInto2ByteDigitsArray((int)CommandType)));
            data_tmp1.AddRange(CommandPayloadByteArray);

            List<byte> data_tmp2 = new List<byte>();

            data_tmp2.AddRange(IntTo2ByteArray(data_tmp1.Count));
            data_tmp2.AddRange(data_tmp1);
            data_tmp2.Add(0x03); // ETX

            List<byte> data = new List<byte>();

            data.Add(0x02); // STX
            data.AddRange(data_tmp2);

            var bcc = XorBytes(data_tmp2.ToArray());
            if (BccError)
            {
                byte all1 = 0xFF;
                bcc = (byte)(bcc ^ all1);
            }
            data.Add(bcc); // BCC

            return data.ToArray();
        }

        protected byte[] IntTo2ByteArray(int value)
        {
            byte[] byteArray = new byte[2];
            byteArray[0] = (byte)((value >> 8) & 0xFF); // 上位バイト
            byteArray[1] = (byte)(value & 0xFF);        // 下位バイト
            return byteArray;
        }

        protected byte[] SplitIntInto2ByteDigitsArray(int value)
        {
            byte[] byteArray = new byte[2];
            byteArray[0] = (byte)(value / 10); // 上位バイト
            byteArray[1] = (byte)(value % 10); // 下位バイト
            return byteArray;
        }

        protected byte[] ByteArrayToAsciiArray(byte[] data) => data.Select(x => (byte)(x + 0x30)).ToArray();

        protected byte[] ConvertDigitsToAsciiArray(string input)
        {
            byte[] result = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]))
                {
                    result[i] = (byte)(input[i]);
                }
                else
                {
                    throw new ArgumentException("Input string contains non-digit characters.");
                }
            }

            return result;
        }

        protected byte XorBytes(byte[] byteArray)
        {
            byte result = 0;
            foreach (byte b in byteArray)
            {
                result ^= b;
            }
            return result;
        }

    }
}
