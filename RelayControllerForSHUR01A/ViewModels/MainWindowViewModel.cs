using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using RelayControllerForSHUR01A.Model.Common;
using RelayControllerForSHUR01A.Model.Logging;
using RelayControllerForSHUR01A.Model.SerialInterfaceProtocol;
using RelayControllerForSHUR01A.Model.SerialPortManager;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace RelayControllerForSHUR01A.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "RelayControllerForSHUR01A";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        IEventAggregator _ea;
        LogWriter logWriter;


        public MainWindowViewModel(IEventAggregator ea)
        {
            serialInterfaceProtocolManager = new UsbRelayProtocolManager(new LogWriteRequester(ea));
            // コマンドの準備
            SerialStartButton = new DelegateCommand(SerialStartButtonExecute);
            SerialStopButton = new DelegateCommand(SerialStopButtonExecute);
            RelayOnButton = new DelegateCommand(RelayOnButtonExecute);
            RelayOffButton = new DelegateCommand(RelayOffButtonExecute);
            RelayToggleButton = new DelegateCommand(RelayToggleButtonExecute);
            RenzokuDousaKaishiButton = new DelegateCommand(RenzokuDousaKaishiButtonExecute);
            RenzokuDousaTeishiButton = new DelegateCommand(RenzokuDousaTeishiButtonExecute);

            LogClearButton = new DelegateCommand(LogClearButtonExecute);
            PortListSelectionChanged = new DelegateCommand<object[]>(PortListChangedExecute);
            IncrementRelayOnJikanMsMsCommand = new DelegateCommand(IncrementRelayOnJikanMsValueExecute);
            DecrementRelayOnJikanMsMsCommand = new DelegateCommand(DecrementRelayOnJikanMsValueExecute);
            IncrementRelayOffJikanMsMsCommand = new DelegateCommand(IncrementRelayOffJikanMsValueExecute);
            DecrementRelayOffJikanMsMsCommand = new DelegateCommand(DecrementRelayOffJikanMsValueExecute);

            // コンボボックスの準備
            SerialPortManager.GetAvailablePortNames().ForEach(serialPort =>
            {
                if (!string.IsNullOrEmpty(serialPort))
                {
                    _serialPortList.Add(new ComboBoxViewModel(Common.ExtractNumber(serialPort), serialPort));
                }
            });

            _ea = ea;
            logWriter = new LogWriter(_ea, (Log log) =>
             {
                 SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Black);

                 if (log.logLevel == LogLevel.Error) solidColorBrush = new SolidColorBrush(Colors.Red);
                 else if (log.logLevel == LogLevel.Warning) solidColorBrush = new SolidColorBrush(Colors.DarkOrange);
                 else if (log.logLevel == LogLevel.Info) solidColorBrush = new SolidColorBrush(Colors.MediumBlue);
                 else if (log.logLevel == LogLevel.Debug) solidColorBrush = new SolidColorBrush(Colors.Gray);

                 _logItems.Add(
                         new LogItem()
                         {
                             Timestamp = log.dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                             Content = log.content,
                             ForegroundColor = solidColorBrush
                         });
             });
        }

        /// ボタン関係
        /// シリアル通信関係
        public DelegateCommand SerialStartButton { get; }

        private void SerialStartButtonExecute()
        {
            serialInterfaceProtocolManager.ComStart(selectedSerialComPort);
        }

        public DelegateCommand SerialStopButton { get; }

        private void SerialStopButtonExecute()
        {
            serialInterfaceProtocolManager.ComStop();
        }

        public DelegateCommand RelayOnButton { get; }

        // 認証要求関係

        private string _ninshouYoukyuuRiyoushaId = "00043130";

        public string NinshouYoukyuuRiyoushaId
        {
            get { return _ninshouYoukyuuRiyoushaId; }
            set { SetProperty(ref _ninshouYoukyuuRiyoushaId, value); }
        }

        private void RelayOnButtonExecute()
        {
            serialInterfaceProtocolManager.RelayOn();
        }

        public DelegateCommand RelayOffButton { get; }

        private void RelayOffButtonExecute()
        {
            serialInterfaceProtocolManager.RelayOff();
        }

        public DelegateCommand RelayToggleButton { get; }

        private void RelayToggleButtonExecute()
        {
            serialInterfaceProtocolManager.Toggle();
        }

        public DelegateCommand RenzokuDousaKaishiButton { get; }

        private void RenzokuDousaKaishiButtonExecute()
        {
            serialInterfaceProtocolManager.StartRenzokuDousa();
        }

        public DelegateCommand RenzokuDousaTeishiButton { get; }

        private void RenzokuDousaTeishiButtonExecute()
        {
            serialInterfaceProtocolManager.StopRenzokuDousa();
        }

        public DelegateCommand LogClearButton { get; }

        private void LogClearButtonExecute()
        {
            LogItems = new ObservableCollection<LogItem>();
        }

        public bool LogScroll
        {
            get { return logWriter.LogUpdatedEventFlag; }
            set { logWriter.LogUpdatedEventFlag = value; }
        }

        /// Combo Box
        private ObservableCollection<ComboBoxViewModel> _serialPortList =
            new ObservableCollection<ComboBoxViewModel>();

        public ObservableCollection<ComboBoxViewModel> SerialPortList
        {
            get { return _serialPortList; }
            set { SetProperty(ref _serialPortList, value); }
        }

        public DelegateCommand<object[]> PortListSelectionChanged { get; }

        private void PortListChangedExecute(object[] selectedItems)
        {
            try
            {
                var selectedItem = selectedItems[0] as ComboBoxViewModel;
                selectedSerialComPort = selectedItem.DisplayValue;
            }
            catch { }
        }

        UsbRelayProtocolManager serialInterfaceProtocolManager;
        String selectedSerialComPort = "";

        ObservableCollection<LogItem> _logItems =
            new ObservableCollection<LogItem>();
        public ObservableCollection<LogItem> LogItems
        {
            get { return _logItems; }
            set { SetProperty(ref _logItems, value); }
        }


        // -------------------------------------------------------------------------------


        // 要求状態応答時間
        public uint RelayOnJikanMs
        {
            get { return serialInterfaceProtocolManager.ReleyOnJikanMs; }
            set { SetProperty(ref serialInterfaceProtocolManager.ReleyOnJikanMs, value); }
        }

        public DelegateCommand IncrementRelayOnJikanMsMsCommand { get; private set; }
        public DelegateCommand DecrementRelayOnJikanMsMsCommand { get; private set; }

        private void IncrementRelayOnJikanMsValueExecute()
        {
            if (RelayOnJikanMs <= 9900)
            {
                RelayOnJikanMs = RelayOnJikanMs + 100;
            }
        }

        private void DecrementRelayOnJikanMsValueExecute()
        {
            if (RelayOnJikanMs >= 100)
            {
                RelayOnJikanMs = RelayOnJikanMs - 100;
            }
        }


        // 要求状態応答時間
        public uint RelayOffJikanMs
        {
            get { return serialInterfaceProtocolManager.ReleyOffJikanMs; }
            set { SetProperty(ref serialInterfaceProtocolManager.ReleyOffJikanMs, value); }
        }

        public DelegateCommand IncrementRelayOffJikanMsMsCommand { get; private set; }
        public DelegateCommand DecrementRelayOffJikanMsMsCommand { get; private set; }

        private void IncrementRelayOffJikanMsValueExecute()
        {
            if (RelayOffJikanMs <= 9900)
            {
                RelayOffJikanMs = RelayOffJikanMs + 100;
            }
        }

        private void DecrementRelayOffJikanMsValueExecute()
        {
            if (RelayOffJikanMs >= 100)
            {
                RelayOffJikanMs = RelayOffJikanMs - 100;
            }
        }
    }
}
