﻿using RelayControllerForSHUR01A.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelayControllerForSHUR01A.Model.SerialInterfaceProtocol
{
    public enum CommandType
    {
        [StringValue("ダミー")]
        DummyCommand = 0,
        [StringValue("リレーON")]
        RelayOn = 1,
        [StringValue("RelayOFF")]
        RelayOff = 2,
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

            List<byte> data = new List<byte>();

            var atc = "AT+CH1=1";

            data.AddRange(Encoding.ASCII.GetBytes(atc));

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
