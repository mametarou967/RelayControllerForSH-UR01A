using RelayControllerForSHUR01A.Model.Common;
using RelayControllerForSHUR01A.Model.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RelayControllerForSHUR01A.Model.SerialInterfaceProtocol
{
    public enum RelayStatus
    {
        [StringValue("On")]
        On,
        [StringValue("Off")]
        Off = 1,
    }
    public class UsbRelayProtocolManager
    {
        RelayControllerForSHUR01A.Model.SerialCom.SerialCom serialCom;
        Queue<byte> receiveDataQueue = new Queue<byte>();
        ILogWriteRequester logWriteRequester;
        private readonly object sendLock = new object(); // ロックオブジェクト
        RelayStatus relayStatus = RelayStatus.Off;
        private CancellationTokenSource cancellationTokenSource;
        Task serialComTask;


        // 要求応答プロパティ
        public uint ReleyOnJikanMs = 2000;
        public uint ReleyOffJikanMs = 2000;

        public UsbRelayProtocolManager(ILogWriteRequester logWriteRequester)
        {
            this.logWriteRequester = logWriteRequester;
        }

        public void ComStart(string comPort)
        {
            if (IsComStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "通信中のため、新しい通信は開始しません");
                return;
            }

            serialCom = new SerialCom.SerialCom(comPort, DataReceiveAction, logWriteRequester);
            serialCom.StartCom();
        }

        public void ComStop()
        {
            if (!IsComStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "通信が開始されていないため、通信の停止処理は行いません");
                return;
            }

            if (IsRenzokuDousaStarted()) StopRenzokuDousa();

            serialCom?.StopCom();
        }

        private bool IsComStarted()
        {
            if((serialCom == null) || !serialCom.IsCommunicating)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        void Send(ICommand command)
        {
            if (!IsComStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "通信が開始されていないため、コマンドは送信しません");
                return;
            }

            lock (sendLock) // ロックを獲得
            {

                try
                {

                    if (serialCom != null)
                    {
                        var byteArray = command.ByteArray();
                        string asciiString = System.Text.Encoding.ASCII.GetString(byteArray);
                        logWriteRequester.WriteRequest(LogLevel.Info, $"[送信]{asciiString}");

                        // 送信前に状態を覚えておく
                        if (command.CommandType == CommandType.RelayOn) relayStatus = RelayStatus.On;
                        else if (command.CommandType == CommandType.RelayOff) relayStatus = RelayStatus.Off;

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

        public void RelayOn()
        {
            if (!IsComStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "通信が開始されていないため、リレーON操作は行いません");
                return;
            }

            if (IsRenzokuDousaStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "連続動作処理が開始されているため、リレーON操作は行いません");
                return;
            }

            Send(new RelayOnCommand());
        }

        public void RelayOff()
        {
            if (!IsComStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "通信が開始されていないため、リレーOFF操作は行いません");
                return;
            }

            if (IsRenzokuDousaStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "連続動作処理が開始されているため、リレーOFF操作は行いません");
                return;
            }

            Send(new RelayOffCommand());
        }

        public void Toggle()
        {
            if (!IsComStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "通信が開始されていないため、トグル操作は行いません");
                return;
            }

            if (IsRenzokuDousaStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "連続動作処理が開始されているため、トグル操作は行いません");
                return;
            }

            ICommand command = new RelayOffCommand();
            if (relayStatus == RelayStatus.Off) command = new RelayOnCommand();

            Send(command);
        }


        public bool StartRenzokuDousa()
        {
            // 例外処理
            if (!IsComStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "通信が開始されていないため、連続動作開始処理は行いません");
                return false;
            }

            if (IsRenzokuDousaStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "連続動作処理が開始されているため、連続動作開始処理は行いません");
                return false;
            }


            cancellationTokenSource = new CancellationTokenSource();
            logWriteRequester.WriteRequest(LogLevel.Info, "連続動作処理開始");

            serialComTask = Task.Run(async () =>
            {
                while (true)
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested) break;
                    Send(new RelayOnCommand());

                    await Task.Delay(TimeSpan.FromMilliseconds(ReleyOnJikanMs)); // Adjust the delay duration as needed

                    if (cancellationTokenSource.Token.IsCancellationRequested) break;
                    Send(new RelayOffCommand());
                    
                    await Task.Delay(TimeSpan.FromMilliseconds(ReleyOffJikanMs)); // Adjust the delay duration as needed

                }

                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
                logWriteRequester.WriteRequest(LogLevel.Info, "連続動作処理終了");
            });


            return true;
        }

        bool IsRenzokuDousaStarted()
        {
            if(cancellationTokenSource == null)
            {
                return false;
            }
            return true;
        }

        public bool StopRenzokuDousa()
        {
            bool result = true;

            if (!IsComStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "通信が開始されていないため、連続動作停止処理は行いません");
                return false;
            }

            if (!IsRenzokuDousaStarted())
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "連続動作処理が開始されていないため、連続動作停止処理は行いません");
                return false;
            }

            if(cancellationTokenSource.Token.IsCancellationRequested)
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "すでに連続動作停止処理実施要求済のため、連続動作停止処理は行いません");
                return false;
            }

            logWriteRequester.WriteRequest(LogLevel.Info, "連続動作処理の停止を行います");
            cancellationTokenSource.Cancel();

            return result;
        }

        private void DataReceiveAction(byte[] datas)
        {
            byte byteToRemove = 0x0A; // 改行コードは除去
            if (datas.Length != 0)
            {
                string asciiString = System.Text.Encoding.ASCII.GetString(datas.Where(b => b != byteToRemove).ToArray());
                logWriteRequester.WriteRequest(LogLevel.Info, $"[受信]{ asciiString }");
                
            }
        }
    }
}
