using RelayControllerForSHUR01A.Model.Common;
using RelayControllerForSHUR01A.Model.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RelayControllerForSHUR01A.Model.SerialInterfaceProtocol
{
    public class SerialInterfaceProtocolManager
    {
        RelayControllerForSHUR01A.Model.SerialCom.SerialCom serialCom;
        Queue<byte> receiveDataQueue = new Queue<byte>();
        ILogWriteRequester logWriteRequester;
        private readonly object sendLock = new object(); // ロックオブジェクト


        // 要求応答プロパティ
        public bool IsResponseEnableYoukyuuOutou = true;
        public bool IsIdtAdrErrorYoukyuuOutou = false;
        public bool IsInoutDirErrorYoukyuuOutou = false;
        public bool IsRiyoushaIdErrorYoukyuuOutou = false;
        public bool IsBccErrorYoukyuuOutou = false;
        public uint YoukyuuOutouJikanMs = 200;
        public YoukyuuOutouKekka YoukyuuOutouKekka = YoukyuuOutouKekka.YoukyuuJuriOk;

        // 要求状態応答プロパティ
        public bool IsResponseEnableYoukyuuJoutaiOutou = true;
        public bool IsIdtAdrErrorYoukyuuJoutaiOutou = false;
        public bool IsInoutDirErrorYoukyuuJoutaiOutou = false;
        public bool IsRiyoushaIdErrorYoukyuuJoutaiOutou = false;
        public bool IsBccErrorYoukyuuJoutaiOutou = false;
        public uint YoukyuuJoutaiOutouJikanMs = 200;
        public NinshouJoutai NinshouJoutai = NinshouJoutai.Syorikanryou;
        public NinshouKanryouJoutai NinshouKanryouJoutai = NinshouKanryouJoutai.NinshouKekkaOk;
        public NinshouKekkaNgShousai NinshouKekkaNgShousai = NinshouKekkaNgShousai.Nashi;
        public string RiyoushaId = "00043130";

        public SerialInterfaceProtocolManager(ILogWriteRequester logWriteRequester)
        {
            this.logWriteRequester = logWriteRequester;
        }

        public void ComStart(string comPort)
        {
            if ((serialCom != null) && serialCom.IsCommunicating)
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "通信中のため、新しい通信は開始しません");
            }
            else
            {
                serialCom = new SerialCom.SerialCom(comPort, DataReceiveAction, logWriteRequester);
                serialCom.StartCom();
            }
        }

        public void ComStop()
        {
            if ((serialCom == null) || !(serialCom.IsCommunicating))
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "通信が開始されていないため、通信の停止処理は行いません");
            }
            else
            {
                serialCom?.StopCom();
            }
        }

        public void Send(ICommand command)
        {
            lock (sendLock) // ロックを獲得
            {

                try
                {
                    var byteArray = command.ByteArray();

                    if (serialCom != null)
                    {
                        logWriteRequester.WriteRequest(LogLevel.Info, "[送信] " + command.ToString());

                        // if (bccError) logWriteRequester.WriteRequest(LogLevel.Warning, "<i> bccError設定が有効のため、BCCエラーで送信します");

                        serialCom.Send(byteArray);
                    }
                }
                catch
               (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void DataReceiveAction(byte[] datas)
        {
            // キュー詰め
            datas.ToList().ForEach(receiveDataQueue.Enqueue);


            while (receiveDataQueue.ToArray().Length != 0)
            {
                // 受信データの評価
                var byteCheckResult = CommandGenerator.ByteCheck(receiveDataQueue.ToArray());

                if (byteCheckResult == ByteCheckResult.Ok)
                {
                    // サイズを調べる
                    var size = CommandGenerator.GetCommandByteLength(receiveDataQueue.ToArray());

                    if (!size.HasValue) continue;

                    List<byte> commandBytes = new List<byte>();

                    // サイズ分デキューする
                    for (int i = 0; i < size.Value; i++)
                    {
                        commandBytes.Add(receiveDataQueue.Dequeue());
                    }

                    // バイト列から受信コマンドを生成する
                    var receiveCommand = CommandGenerator.CommandGenerate(commandBytes.ToArray());

                    logWriteRequester.WriteRequest(LogLevel.Info, "[受信] " + receiveCommand.ToString());

                    bool isResponseEnable = true;
                    uint outouJikanMs = 500;
                    var idtAdrError = false;
                    var inoutDirError = false;
                    var riyoushaIdError = false;
                    var bccError = false;

                    if (receiveCommand.CommandType == CommandType.NinshouYoukyuu) isResponseEnable = IsResponseEnableYoukyuuOutou;
                    else if (receiveCommand.CommandType == CommandType.NinshouJoutaiYoukyuu) isResponseEnable = IsResponseEnableYoukyuuJoutaiOutou;

                    if (receiveCommand.CommandType == CommandType.NinshouYoukyuu)
                    {
                        outouJikanMs = YoukyuuOutouJikanMs;
                        idtAdrError = IsIdtAdrErrorYoukyuuOutou;
                        inoutDirError = IsInoutDirErrorYoukyuuOutou;
                        riyoushaIdError = IsRiyoushaIdErrorYoukyuuOutou;
                        bccError = IsBccErrorYoukyuuOutou;

                    }
                    else if (receiveCommand.CommandType == CommandType.NinshouJoutaiYoukyuu)
                    {
                        outouJikanMs = YoukyuuJoutaiOutouJikanMs;
                        idtAdrError = IsIdtAdrErrorYoukyuuJoutaiOutou;
                        inoutDirError = IsInoutDirErrorYoukyuuJoutaiOutou;
                        riyoushaIdError = IsRiyoushaIdErrorYoukyuuJoutaiOutou;
                        bccError = IsBccErrorYoukyuuJoutaiOutou;
                    }

                    if (idtAdrError) logWriteRequester.WriteRequest(LogLevel.Warning, $"ID端末アドレスエラー設定を反映して応答コマンドを作成します");
                    if (inoutDirError) logWriteRequester.WriteRequest(LogLevel.Warning, $"ID端末入退室方向エラー設定を反映して応答コマンドを作成します");
                    if (riyoushaIdError) logWriteRequester.WriteRequest(LogLevel.Warning, $"利用者IDエラー設定を反映して応答コマンドを作成します");
                    if (bccError) logWriteRequester.WriteRequest(LogLevel.Warning, $"BCCエラー設定を反映して応答コマンドを作成します");

                    var responseCommand = ResponseGenerate(receiveCommand, idtAdrError, inoutDirError, riyoushaIdError, bccError);

                    if (responseCommand.CommandType != CommandType.DummyCommand)
                    {
                        if (isResponseEnable)
                        {
                            // 有効な応答コマンドが生成されているので任意の時間経過後応答する
                            Task.Run(async () =>
                            {
                                await Task.Delay(TimeSpan.FromMilliseconds(outouJikanMs));

                                Send(responseCommand);
                            });
                        }
                        else
                        {
                            logWriteRequester.WriteRequest(LogLevel.Warning, $"応答設定が行われていないため{receiveCommand.CommandType} のコマンドは送信しません");
                        }
                    }

                }
                else if (byteCheckResult == ByteCheckResult.NgNoStx)
                {
                    // 先頭をdequeueして終了
                    receiveDataQueue.Dequeue();

                }
                else if (
                    (byteCheckResult == ByteCheckResult.NgNoByte) ||
                    (byteCheckResult == ByteCheckResult.NgHasNoLengthField) ||
                    (byteCheckResult == ByteCheckResult.NgMessageIncompleted))
                {
                    // データがたまるまで待つ
                    break;
                }
                else if (
                    (byteCheckResult == ByteCheckResult.NgNoEtx) ||
                    (byteCheckResult == ByteCheckResult.NgBccError))
                {
                    // サイズを調べる
                    var size = CommandGenerator.GetCommandByteLength(receiveDataQueue.ToArray());

                    if (!size.HasValue) continue;

                    // サイズ分デキューする
                    for (int i = 0; i < size.Value; i++)
                    {
                        receiveDataQueue.Dequeue();
                    }

                    logWriteRequester.WriteRequest(LogLevel.Error, $"{ byteCheckResult.GetStringValue()} のためメッセージを破棄します");
                }
            }
        }

        ICommand ResponseGenerate(
            ICommand command,
            bool idtAdrError = false,
            bool inoutDirError = false,
            bool riyoushaIdError = false,
            bool bccError = false)
        {
            if (command.CommandType == CommandType.NinshouYoukyuu)
            {
                // 受信コマンドの応答を生成
                var ninshouYoukyuuOutouCommand = CommandGenerator.ResponseGenerate(
                    command as NinshouYoukyuuCommand,
                    YoukyuuOutouKekka,
                    YoukyuuJuriNgSyousai.Nashi, // 一旦固定
                    idtAdrError,
                    inoutDirError,
                    riyoushaIdError,
                    bccError
                    );

                return ninshouYoukyuuOutouCommand;
            }
            else if (command.CommandType == CommandType.NinshouJoutaiYoukyuu)
            {
                // 受信コマンドの応答を生成
                var ninshouJoutaiYoukyuuOutouCommand = CommandGenerator.ResponseGenerate(
                    command as NinshouJoutaiYoukyuuCommand,
                    NinshouJoutai,
                    NinshouKanryouJoutai,
                    NinshouKekkaNgShousai,
                    RiyoushaId,
                    idtAdrError,
                    inoutDirError,
                    riyoushaIdError,
                    bccError
                    );

                return ninshouJoutaiYoukyuuOutouCommand;
            }

            return new DummyCommand(0, NyuutaishitsuHoukou.Nyuushitsu);
        }
    }
}
