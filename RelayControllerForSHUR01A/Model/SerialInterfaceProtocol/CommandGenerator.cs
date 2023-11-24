using RelayControllerForSHUR01A.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayControllerForSHUR01A.Model.SerialInterfaceProtocol
{
    public enum ByteCheckResult
    {

        [StringValue("OK")]
        Ok,
        [StringValue("データなし")]
        NgNoByte,
        [StringValue("先頭がSTXでない")]
        NgNoStx,
        [StringValue("messageの長さフィールドを持っていない(3バイトより短い)")]
        NgHasNoLengthField,
        [StringValue("長さフィールド分のメッセージを持っていない")]
        NgMessageIncompleted,
        [StringValue("終端がETXでない")]
        NgNoEtx,
        [StringValue("BCCエラー")]
        NgBccError,
    }


    public static class CommandGenerator
    {

        public static ByteCheckResult ByteCheck(byte[] data)
        {
            if (data.Length == 0) return ByteCheckResult.NgNoByte;

            if (data[0] != 0x02 /* STX */ ) return ByteCheckResult.NgNoStx;

            if (data.Length < 3) return ByteCheckResult.NgHasNoLengthField;

            var messageLength = (data[1] << 8) | data[2];

            if (data.Length < messageLength + 5) return ByteCheckResult.NgMessageIncompleted;

            if (data[3 + messageLength] != 0x03 /* ETX */ ) return ByteCheckResult.NgNoEtx;

            byte bcc = 0;
            for (int index = 1; index < (3 + messageLength + 1); index++)
            {
                bcc ^= data[index];
            }

            if (data[3 + messageLength + 1] != bcc) return ByteCheckResult.NgBccError;

            return ByteCheckResult.Ok;
        }

        public static int? GetCommandByteLength(byte[] data)
        {
            if (data.Length < 3) return null;

            var messageLength = (data[1] << 8) | data[2];

            if (data.Length < messageLength + 5) return null;

            return messageLength + 5;
        }

        public static Command CommandGenerate(byte[] data)
        {
            // フォーマットはあっている前提
            var idTanmatsuAddress = ((data[3] - 0x30) * 10) + (data[4] - 0x30);

            var commandType = ((data[6] - 0x30) * 10) + (data[7] - 0x30);
            var nyuutaishitsuHoukou = (NyuutaishitsuHoukou)data[5] - 0x30;

            if (commandType == (int)CommandType.NinshouYoukyuu)
            {
                var idLength = ((data[8] - 0x30) * 10) + (data[9] - 0x30);

                return new NinshouYoukyuuCommand(
                    idTanmatsuAddress,
                    nyuutaishitsuHoukou,
                    ExtractAndConvert(data, 10, idLength)
                    );
            }
            else if (commandType == (int)CommandType.NinshouYoukyuuOutou)
            {
                var youkyuuOutouKekka = (YoukyuuOutouKekka)(data[8] - 0x30);
                var youkyuuJuriNgSyousai = (YoukyuuJuriNgSyousai)(data[9] - 0x30);
                var idLength = ((data[10] - 0x30) * 10) + (data[11] - 0x30);

                return new NinshouYoukyuuOutouCommand(
                    idTanmatsuAddress,
                    nyuutaishitsuHoukou,
                    youkyuuOutouKekka,
                    youkyuuJuriNgSyousai,
                    ExtractAndConvert(data, 12, idLength)
                    );
            }
            else if (commandType == (int)CommandType.NinshouJoutaiYoukyuu)
            {
                return new NinshouJoutaiYoukyuuCommand(
                    idTanmatsuAddress,
                    nyuutaishitsuHoukou
                    );
            }
            else if (commandType == (int)CommandType.NinshouJoutaiYoukyuuOutou)
            {
                var ninshouJoutai = (NinshouJoutai)(data[8] - 0x30);
                var ninshouKanryouJoutai = (NinshouKanryouJoutai)(data[9] - 0x30);
                var ninshouKekkaNgShousai = (NinshouKekkaNgShousai)(data[10] - 0x30);
                var idLength = ((data[11] - 0x30) * 10) + (data[12] - 0x30);

                return new NinshouJoutaiYoukyuuOutouCommand(
                    idTanmatsuAddress,
                    nyuutaishitsuHoukou,
                    ninshouJoutai,
                    ninshouKanryouJoutai,
                    ninshouKekkaNgShousai,
                    ExtractAndConvert(data, 13, idLength)
                    );
            }

            return new DummyCommand(idTanmatsuAddress, nyuutaishitsuHoukou); ;

        }

        public static NinshouYoukyuuOutouCommand ResponseGenerate(
            NinshouYoukyuuCommand ninshouYoukyuuCommand,
            YoukyuuOutouKekka youkyuuOutouKekka,
            YoukyuuJuriNgSyousai youkyuuJuriNgSyousai,
             bool idtAdrError = false,
             bool inoutDirError = false,
             bool riyoushaIdError = false,
             bool bccError = false
        )
        {
            return new NinshouYoukyuuOutouCommand(
                FixIdTanmatsuAddress(ninshouYoukyuuCommand.IdTanmatsuAddress, idtAdrError),
                FixNyuutaishitsuHoukou(ninshouYoukyuuCommand.NyuutaishitsuHoukou, inoutDirError),
                youkyuuOutouKekka,
                youkyuuJuriNgSyousai,
                FixRiyoushaId(ninshouYoukyuuCommand.Id, riyoushaIdError),
                bccError
                );
        }

        public static NinshouJoutaiYoukyuuOutouCommand ResponseGenerate(
            NinshouJoutaiYoukyuuCommand ninshouJoutaiYoukyuuCommand,
            NinshouJoutai ninshouJoutai,
            NinshouKanryouJoutai ninshouKanryouJoutai,
            NinshouKekkaNgShousai ninshouKekkaNgShousai,
            string id,
            bool idtAdrError = false,
            bool inoutDirError = false,
            bool riyoushaIdError = false,
            bool bccError = false
            )
        {
            return new NinshouJoutaiYoukyuuOutouCommand(
                FixIdTanmatsuAddress(ninshouJoutaiYoukyuuCommand.IdTanmatsuAddress, idtAdrError),
                FixNyuutaishitsuHoukou(ninshouJoutaiYoukyuuCommand.NyuutaishitsuHoukou, inoutDirError),
                ninshouJoutai,
                ninshouKanryouJoutai,
                ninshouKekkaNgShousai,
                FixRiyoushaId(id, riyoushaIdError),
                bccError);
        }

        static string ExtractAndConvert(byte[] byteArray, int startIndex, int length)
        {
            if (startIndex < 0 || startIndex >= byteArray.Length || length <= 0)
            {
                return string.Empty; // 無効な引数の場合は空文字列を返す
            }

            int endIndex = startIndex + length - 1;
            endIndex = Math.Min(endIndex, byteArray.Length - 1);

            byte[] convertedChars = new byte[endIndex - startIndex + 1];

            for (int i = startIndex; i <= endIndex; i++)
            {
                convertedChars[i - startIndex] = byteArray[i];
            }

            var str = System.Text.Encoding.GetEncoding("shift_jis").GetString(convertedChars);
            return str;
        }

        static private int FixIdTanmatsuAddress(int idTanmatsuAddress, bool idtAdrError) =>
            (idtAdrError) ? idTanmatsuAddress + 1 : idTanmatsuAddress;

        static private NyuutaishitsuHoukou FixNyuutaishitsuHoukou(NyuutaishitsuHoukou nyuutaishitsuHoukou, bool inoutDirError)
        {
            NyuutaishitsuHoukou fixNyuutaishitsuHoukou = nyuutaishitsuHoukou;

            if (inoutDirError)
            {
                if (fixNyuutaishitsuHoukou == NyuutaishitsuHoukou.Nyuushitsu) fixNyuutaishitsuHoukou = NyuutaishitsuHoukou.Taishitsu;
                else fixNyuutaishitsuHoukou = NyuutaishitsuHoukou.Nyuushitsu;
            }

            return fixNyuutaishitsuHoukou;
        }

        static private string FixRiyoushaId(string riyoushaId, bool riyoushaIdError)
        {
            // 文字列を整数に変換して1増やす
            long riyoushaNumber = long.Parse(riyoushaId);
            if (riyoushaIdError) riyoushaNumber++;

            // 数値を文字列に戻す時、桁数を保持するためにPadLeftを使用
            return riyoushaNumber.ToString().PadLeft(riyoushaId.Length, '0');

        }
    }
}
