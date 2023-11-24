using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RelayControllerForSHUR01A.Model.Common
{
    static class Common
    {
        static public int ExtractNumber(string input)
        {
            string numericString = Regex.Replace(input, "[^0-9]", "");
            int extractedNumber = int.Parse(numericString);
            return extractedNumber;
        }

        // パディング処理
        static public string PaddingInBytes(string value, PadType type, int byteCount)
        {
            Encoding enc = Encoding.GetEncoding("Shift_JIS");

            if (byteCount < enc.GetByteCount(value))
            {
                // valueが既定のバイト数を超えている場合は、切り落とし
                value = value.Substring(0, byteCount);
            }

            switch (type)
            {
                case PadType.Char:
                    // 文字列の場合　左寄せ＋空白埋め
                    var a1 = byteCount;
                    var a2 = enc.GetByteCount(value);
                    var a3 = 0; // value.Length;
                    return value.PadRight(a1 - (a2 - a3));
                case PadType.Number:
                    // 数値の場合　右寄せ＋0埋め
                    return value.PadLeft(byteCount, '0');
                default:
                    // 上記以外は全部空白
                    return value.PadLeft(byteCount);
            }
        }
    }

    // 項目属性の列挙体
    public enum PadType
    {
        Char
        , Number
    }
}
