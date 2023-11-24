using RelayControllerForSHUR01A.Model.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RelayControllerForSHUR01A.Model.SerialCom
{
    public class SerialCom
    {
        private string comport;
        Task serialComTask;
        private CancellationTokenSource cancellationTokenSource;
        private SerialPort serialPort;
        ConcurrentQueue<byte> sendQueue = new ConcurrentQueue<byte>();
        ILogWriteRequester logWriteRequester;
        Action<byte[]> dataReceiveAction;

        public bool IsCommunicating
        {
            get
            {
                if (serialPort == null) return false;
                return serialPort.IsOpen;
            }
        }


        public SerialCom(
            string comport,
            Action<byte[]> dataReceiveAction,
            ILogWriteRequester logWriteRequester)
        {
            this.comport = comport;
            this.dataReceiveAction = dataReceiveAction;
            this.logWriteRequester = logWriteRequester;
        }

        public bool StartCom()
        {
            if (String.IsNullOrEmpty(comport))
            {
                logWriteRequester.WriteRequest(
                LogLevel.Error,
                $"comportが空白やnullのため通信を開始することができません");
                return false;
            }

            try
            {
                serialPort = new SerialPort(comport, 19200);
                serialPort.Open();
            }
            catch (Exception ex)
            {
                logWriteRequester.WriteRequest(
                LogLevel.Error,
                $"[例外-ﾒｯｾｰｼﾞ] " + $"{ex.Message} ");
                logWriteRequester.WriteRequest(
                LogLevel.Error,
                $"[例外-ｱﾌﾟﾘｹｰｼｮﾝ] " + $"{ex.Source} ");
                logWriteRequester.WriteRequest(
                LogLevel.Error,
                $"[例外-ｽﾀｯｸﾄﾚｰｽ] " + $"{ex.StackTrace} ");
                logWriteRequester.WriteRequest(
                LogLevel.Error,
                $"[例外-ﾒｿｯﾄﾞ] " + $"{ex.TargetSite} ");

                return false;
            }

            logWriteRequester.WriteRequest(
            LogLevel.Info,
            $"[COM-START1] " +
            $"PortName:{serialPort.PortName} " +
            $"OpenStatus:{serialPort.IsOpen} ");

            logWriteRequester.WriteRequest(
            LogLevel.Info,
            $"[COM-START2] " +
            $"DataBits:{serialPort.DataBits} " +
            $"BaudRate:{serialPort.BaudRate} " +
            $"Parity:{serialPort.Parity} " +
            $"StopBits:{serialPort.StopBits}");

            cancellationTokenSource = new CancellationTokenSource();

            serialComTask = Task.Run(async () =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (serialPort.BytesToRead > 0)
                    {
                        byte[] receivedBytes = new byte[serialPort.BytesToRead];
                        serialPort.Read(receivedBytes, 0, receivedBytes.Length);

                        // 受信データバイナリログ出力
                        logWriteRequester.WriteRequest(LogLevel.Debug, "[RCV] " + string.Join(" ", receivedBytes.Select(b => $"0x{b:X2}")));

                        dataReceiveAction?.Invoke(receivedBytes);
                    }

                    var sendList = new List<byte>();
                    // sendQueueを調べてデータがあればserialPortに送信する
                    while (!sendQueue.IsEmpty)
                    {
                        if (sendQueue.TryDequeue(out byte data))
                        {
                            sendList.Add(data);
                        }
                    }

                    if (sendList.Count != 0)
                    {
                        serialPort.Write(sendList.ToArray(), 0, sendList.Count);

                        // 送信データバイナリログ出力
                        logWriteRequester.WriteRequest(LogLevel.Debug, "[SND] " + string.Join(" ", sendList.ToArray().Select(b => $"0x{b:X2}")));

                        // 半二重の場合には、自分が送信したデータを受信してしまうため、同じ数データを受信した時点でこれを破棄する処理を行う
                        // 送信したデータを受信している間は外部からの受信はないという前提で行う
                        // 前二重でこのコードを実行した場合は無限待ちになってしまう可能性があるので、注意すること
                        while (true)
                        {
                            if (serialPort.BytesToRead >= sendList.Count)
                            {
                                byte[] receiveBuffer = new byte[sendList.Count];

                                // 送った分だけ読み取る(そして捨てる)
                                serialPort.Read(receiveBuffer, 0, sendList.Count);

                                break;
                            }
                        }

                    }

                    await Task.Delay(100); // Adjust the delay duration as needed
                }
            });

            return true;
        }

        public bool StopCom()
        {
            bool result = true;

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                try
                {
                    var waitResult = serialComTask.Wait(5000);
                    if (!waitResult) result = false;
                }
                catch
                {
                }
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                logWriteRequester.WriteRequest(
                LogLevel.Info,
                $"[COM-STOP ] " +
                $"PortName:{serialPort.PortName} " +
                $"OpenStatus:{serialPort.IsOpen} ");
                serialPort.Dispose();
                serialPort = null;
            }

            return result;
        }

        public void Send(IEnumerable<byte> datas)
        {
            datas.ToList().ForEach((data) => sendQueue.Enqueue(data));
        }
    }
}
