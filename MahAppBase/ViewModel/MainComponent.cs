using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using AngleSharp;
using AngleSharp.Dom;
using MahAppBase.Command;
using Notifications.Wpf;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;
using Application = System.Windows.Application;
using Visibility = System.Windows.Visibility;

namespace MahAppBase.ViewModel
{
    public class MainComponent : ViewModelBase
    {
        #region Declarations
        System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();
        private WindowState _State = WindowState.Normal;
        private Visibility _Render = Visibility.Visible;
        private int _FlyOutWidth = 500;
        private Thickness _WebMargin = new Thickness(0, 0, 0, 0);
        private string _ErrorMessage = string.Empty;
        private string _CurrentTab = string.Empty;
        private bool _FlyOutScheduleIsOpen = false;
        Notifier Notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.BottomRight,
                offsetX: 10,
                offsetY: 20);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(3),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });
        private ObservableCollection<DataModel> _DataSource = new ObservableCollection<DataModel>();
        private int _TotalFileCount = 0;
        private int _CurrentDownloadCount = 0;
        string localExecuteResult = string.Empty;
        private string _TargetPath = string.Empty;
        private string _Source = string.Empty;
        public delegate void ReadUrlDataEvent(object sender, EventArgs e);
        LeoQueue<QueueModel> messageQueue = new LeoQueue<QueueModel>();
        int _scheduleID = 0;
        private string ConfigPath = $"{System.AppDomain.CurrentDomain.BaseDirectory}Setting.config";
        private Visibility _MainWindowVisibly = Visibility.Visible;
        private bool _ShowInToolBar = true;
        #endregion

        #region Property
        public NoParameterCommand MainWindowClosed { get; set; }
        public NoParameterCommand ButtonScheduleClick { get; set; }
        public NoParameterCommand FlyOutSettingClose { get; set; }
        public NoParameterCommand ChooseSourceButtonClick { get; set; }
        public NoParameterCommand ChooseTargetButtonClick { get; set; }
        public bool FlyOutScheduleIsOpen
        {
            get
            {
                return _FlyOutScheduleIsOpen;
            }
            set
            {
                if (value)
                    _FlyOutScheduleIsOpen = value;
                else
                    _FlyOutScheduleIsOpen = value;
                OnPropertyChanged();
            }
        }
        public string ErrorMessage
        {
            get
            {
                return _ErrorMessage;
            }
            set
            {
                _ErrorMessage += value;
                OnPropertyChanged();
            }
        }
        public string CurrentTab
        {
            get { return _CurrentTab; }
            set { _CurrentTab = value; OnPropertyChanged(); }
        }
        public int FlyOutWidth
        {
            get
            {
                return _FlyOutWidth;
            }
            set
            {
                _FlyOutWidth = value;
                OnPropertyChanged();
                OnPropertyChanged();
            }
        }
        public Visibility Render
        {
            get { return _Render; }
            set
            {
                _Render = value;
                OnPropertyChanged();
            }
        }
        public Thickness WebMargin
        {
            get
            {
                return _WebMargin;
            }
            set
            {
                _WebMargin = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<DataModel> DataSource
        {
            get { return _DataSource; }
            private set { _DataSource = value; OnPropertyChanged(); }
        }
        public NoParameterCommand ButtonDownloadClick { get; set; }
        public int TotalFileCount
        {
            get
            {
                return _TotalFileCount;
            }
            set
            {
                _TotalFileCount = value;
                OnPropertyChanged();
            }
        }
        public int CurrentDownloadCount
        {
            get
            {
                return _CurrentDownloadCount;
            }
            set
            {
                _CurrentDownloadCount = value;
                OnPropertyChanged();
            }
        }
        public string Source
        {
            get
            {
                return _Source;
            }
            set
            {
                _Source = value;
                OnPropertyChanged();
            }
        }
        public string TargetPath
        {
            get
            {
                return _TargetPath;
            }
            set
            {
                _TargetPath = value;
                OnPropertyChanged();
            }
        }
        public WindowState State
        {
            get
            {
                return _State;
            }
            set
            {
                _State = value;
                switch (_State)
                {
                    case WindowState.Minimized:
                        MainWindowVisibly = Visibility.Hidden;
                        nIcon.Visible = true;
                        ShowInToolBar = false;
                        break;
                    case WindowState.Normal:
                        MainWindowVisibly = Visibility.Visible;
                        nIcon.Visible = false;
                        ShowInToolBar = true;
                        break;
                    case WindowState.Maximized:
                        MainWindowVisibly = Visibility.Visible;
                        nIcon.Visible = false;
                        ShowInToolBar = true;
                        break;
                }
                OnPropertyChanged();
            }
        }
        public Visibility MainWindowVisibly
        {
            get
            {
                return _MainWindowVisibly;
            }
            set
            {
                _MainWindowVisibly = value;
                OnPropertyChanged();
            }
        }
        public bool ShowInToolBar
        {
            get
            {
                return _ShowInToolBar;
            }
            set
            {
                _ShowInToolBar = value;
                OnPropertyChanged();
            }
        }
        #endregion
        
        #region MemberFunction

        public MainComponent()
        {
            
            messageQueue.OnEnqueue += MessageQueue_OnEnqueue;
            MainWindowClosed = new NoParameterCommand(MainWindowClosedAction);
            ButtonScheduleClick = new NoParameterCommand(ButtonScheduleClickAction);
            FlyOutSettingClose = new NoParameterCommand(FlyOutSettingCloseAction);
            ChooseSourceButtonClick = new NoParameterCommand(ChooseSourceButtonClickAction);
            ChooseTargetButtonClick = new NoParameterCommand(ChooseTargetButtonClickAction);
            ButtonDownloadClick = new NoParameterCommand(ButtonDownloadClickAction);
            CheckAndReadConfig();
            GetData();
            InitIcon();
        }

        private void MessageQueue_OnEnqueue(object sender, EventArgs e)
        {
        }

        public void InitIcon()
        {
            nIcon.Icon = new Icon("Icon.ico");
            nIcon.Visible = false;
            nIcon.MouseDoubleClick += NIcon_MouseDoubleClick;
            #region Init Contextmenu
            ContextMenu cm = new ContextMenu();
            
            MenuItem miMax = new MenuItem();
            miMax.Text = "放大";
            miMax.Click += Mi_Click;
            cm.MenuItems.Add(miMax);

            MenuItem miClose = new MenuItem();
            miClose.Text = "關閉";
            miClose.Click += Mi_Click;
            cm.MenuItems.Add(miClose);
            #endregion
            nIcon.ContextMenu = cm;
        }

        private void NIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            State = WindowState.Normal;
        }

        private void Mi_Click(object sender, EventArgs e)
        {
            if ((sender as MenuItem) is null)
                return;

            switch((sender as MenuItem).Text)
            {
                case "放大":
                    State = WindowState.Normal;
                    break;
                case "關閉":
                    System.Windows.Application.Current.Shutdown();
                    break;
            }
        }

        private void MainComponent_DataReadFinish(object sender, EventArgs e)
        {
            var notificationManager = new NotificationManager();
            notificationManager.Show(new NotificationContent
            {
                Title = "更新結果",
                Message = "更新完成",
                Type = NotificationType.Success,
            });
            Notifier.ShowSuccess($"更新完成");
        }

        private void ChooseTargetButtonClickAction()
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            TargetPath = path.SelectedPath;
        }

        private void ChooseSourceButtonClickAction()
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            Source = path.SelectedPath;
        }

        private async void ButtonDownloadClickAction()
        {
            new NotificationManager().Show(new NotificationContent
            {
                Title = "更新通知",
                Message = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}開始檢查更新",
                Type = NotificationType.Information,
            });
            Notifier.ShowSuccess($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}開始檢查更新");
            FlyOutScheduleIsOpen = true;
            var newSchedule = new DataModel();
            newSchedule.ThreadID = ++_scheduleID;
            newSchedule.LogDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            newSchedule.Status = ScheduleStatus.Wait;
            newSchedule.Schedule = $"檢查更新{DateTime.Now.ToString("HHmmss")}";
            DataSource.Add(newSchedule);

            await Task.Run(() => GetUrl(_scheduleID));
            //取得執行完的訊息顯示到UI
            var dt = messageQueue.Dequeue();
            switch (dt.Type)
            {
                case QueueModel.TypeDatail.Success:
                    Notifier.ShowSuccess(dt.Message);
                    new NotificationManager().Show(new MahAppBase.View.Test("更新通知", dt.Message, TargetPath));
                    //new NotificationManager().Show(new NotificationContent
                    //{
                    //    Title = "更新通知",
                    //    Message = dt.Message,
                    //    Type = NotificationType.Success,
                    //});
                    break;
                case QueueModel.TypeDatail.Fail :
                    Notifier.ShowError(dt.Message);

                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "更新通知",
                        Message = dt.Message,
                        Type = NotificationType.Error,
                    });
                    break;
            }
        }
        
        public void GetUrl(int scheduleID)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                DataSource.Where(x => x.ThreadID == scheduleID).FirstOrDefault().Status = ScheduleStatus.Running;

                Recursive rr = new Recursive(Source);
                rr.ThreadID = scheduleID;
                rr.DataCollection = DataSource;
                rr.GetFile(TargetPath);
                
                DataSource.Where(x => x.ThreadID == scheduleID).FirstOrDefault().Status = ScheduleStatus.Done;
                sw.Stop();
                messageQueue.Enqueue(new QueueModel() { Message= $"{DataSource.Where(x => x.ThreadID == scheduleID).FirstOrDefault().Schedule}\r\n下載更新成功。共花費時間{sw.Elapsed}", Type = QueueModel.TypeDatail.Success });
            }
            catch (Exception ie)
            {
                DataSource.Where(x => x.ThreadID == scheduleID).FirstOrDefault().Status = ScheduleStatus.Error;
                messageQueue.Enqueue(new QueueModel() { Message = $"{DataSource.Where(x => x.ThreadID == scheduleID).FirstOrDefault().Schedule}\r\n下載更新失敗。\r\n{ie.Message}\r\n{ie.StackTrace}", Type = QueueModel.TypeDatail.Fail });
            }
        }
       
        public void FlyOutSettingCloseAction()
        {
            //Notifier.ShowError("儲存變更失敗。");
            //Notifier.ShowWarning("資料儲存中...");
            //Notifier.ShowSuccess("變更儲存成功。");
            //Notifier.ShowInformation("變更已儲存。");

            WebMargin = new Thickness(0, 0, 0, 0);
            FlyOutScheduleIsOpen = false;
        }

        public void MainWindowClosedAction()
        {
            //寫入Config
            try
            {
                StreamWriter sw = new StreamWriter(ConfigPath);
                sw.WriteLine($"Source|{Source}");
                sw.WriteLine($"TargetPath|{TargetPath}");
                sw.Close();
            }
            catch (Exception ie)
            {
                Notifier.ShowError($"寫入Config發生例外:\r\n{ie.Message}\r\n{ie.StackTrace}");
            }
            System.Windows.Application.Current.Shutdown();
        }

        public void CheckAndReadConfig()
        {
            //檢查檔案是否存在，不存在>產生檔案
            try
            {
                if (!System.IO.File.Exists(ConfigPath))
                {
                    FileStream fs = File.Create(ConfigPath);
                    fs.Close();
                    //寫入Config
                    try
                    {
                        StreamWriter sw = new StreamWriter(ConfigPath);
                        sw.WriteLine($"Source|{Source}");
                        sw.WriteLine($"TargetPath|{TargetPath}");
                        sw.Close();
                    }
                    catch (Exception ie)
                    {
                        Notifier.ShowError($"寫入Config發生例外:\r\n{ie.Message}\r\n{ie.StackTrace}");
                    }
                }

            }
            catch(Exception ie)
            {
                Notifier.ShowError($"檢查檔案是否存在發生例外:\r\n{ie.Message}\r\n{ie.StackTrace}");
            }

            //讀取Config
            try
            {
                StreamReader sr = new StreamReader(ConfigPath);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var result = line.Substring(line.IndexOf("|") + 1, line.Length - (line.IndexOf("|") + 1));
                    if (line.Contains("Source"))
                        Source = result;
                    else if (line.Contains("TargetPath"))
                        TargetPath = result;
                }
                sr.Close();
            }
            catch(Exception ie)
            {
                Notifier.ShowError($"讀取檔案發生例外:\r\n{ie.Message}\r\n{ie.StackTrace}");
            }
            
        }

        public bool GetData()
        {
            try
            {
                return true;
            }
            catch (Exception ie)
            {
                //localExecuteResult = $"取得資料失敗。/r/n{ie.Message}/r/n{ie.StackTrace}";
                return false;
            }
        }

        public void ButtonScheduleClickAction()
        {
            FlyOutScheduleIsOpen = true;
            WebMargin = new Thickness(500, 0, 0, 0);
        }
        #endregion

        #region DataModel
        public class DataModel:INotifyPropertyChanged
        {
            #region Declarations
            private int _ThreadID = 999999;
            private string _Schedule = string.Empty;
            private string _LogDate = string.Empty;
            private ScheduleStatus _Status = ScheduleStatus.Wait;
            private string _Message = "0 / 0";
            #endregion

            #region Property
            public int ThreadID
            {
                get
                {
                    return _ThreadID;
                }
                set
                {
                    _ThreadID = value;
                    OnPropertyChanged();
                }
            }

            public string Schedule
            {
                get
                {
                    return _Schedule;
                }
                set
                {
                    _Schedule = value;
                    OnPropertyChanged();
                }
            }

            public string LogDate
            {
                get
                {
                    return _LogDate;
                }
                set
                {
                    _LogDate = value;
                    OnPropertyChanged();
                }
            }

            public ScheduleStatus Status
            {
                get
                {
                    return _Status;
                }
                set
                {
                    _Status = value;
                    OnPropertyChanged();
                }
            }

            public string Message
            {
                get
                {
                    return _Message;
                }
                set
                {
                    _Message = value;
                    OnPropertyChanged();
                }
            }
            #endregion

            #region Memberfunction
            public event PropertyChangedEventHandler PropertyChanged;

            public void OnPropertyChanged([CallMemberName]string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }

        public class QueueModel
        {
            public string Message { get; set; }
            public TypeDatail Type { get; set; }

            public enum TypeDatail
            {
                Success = 0,
                Fail = 1
            }
        }

        public class Recursive
        {

            #region Declarations
            List<string> data = new List<string>();
            private string _Source = string.Empty;
            private string _Target = string.Empty;
            #endregion

            #region Property

            private int _ThreadID;
            public int ThreadID
            {
                get
                {
                    return _ThreadID;
                }
                set
                {
                    _ThreadID = value;
                }
            }


            private ObservableCollection<DataModel> _DataCollection = new ObservableCollection<DataModel>();
            public ObservableCollection<DataModel> DataCollection
            {
                get
                {
                    return _DataCollection;
                }
                set
                {
                    _DataCollection = value;
                }
            }
            #endregion

            #region Memberfunction
            #endregion


            public Recursive(string topPath)
            {
                _Source = topPath;
                test(topPath);
            }

            private void test(string path)
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    if (!data.Contains(file))
                        data.Add(file);
                }
                foreach (var dir in Directory.GetDirectories(path))
                {
                    foreach (var file in Directory.GetFiles(dir))
                    {
                        if (!data.Contains(file))
                            data.Add(file);
                    }
                    test(dir);
                }
            }

            public List<string> GetFileList()
            {
                return data;
            }

            public void GetFile(string target)
            {
                _Target = target;
                if (data.Count > 0)
                {
                    int tempCount = 0;
                    DataCollection.Where(x => x.ThreadID == ThreadID).FirstOrDefault().Message = $"{data.Count} / {tempCount}";

                    foreach (var item in data)
                    {
                        var subPath = item.Replace(_Source, "");
                        //File.Copy(item, _Target + subPath, true);
                        //檢查對應路徑是否存在
                        // \\192.168.1.155\LeoShare\Release
                        var fullPath = _Target + subPath;
                        var dirSplitIndex = fullPath.LastIndexOf(@"\");
                        if (!System.IO.Directory.Exists(fullPath.Substring(0, dirSplitIndex)))
                            System.IO.Directory.CreateDirectory(fullPath.Substring(0, dirSplitIndex));//不存在就建立目錄
                        File.Copy(item, _Target + subPath, true);
                        tempCount++;
                        DataCollection.Where(x => x.ThreadID == ThreadID).FirstOrDefault().Message = $"{data.Count} / {tempCount}";
                    }
                    //Parallel.ForEach(data, (item, loopState) =>
                    //{
                    //   var subPath = item.Replace(_Source, "");
                    //    //File.Copy(item, _Target + subPath, true);
                    //    //檢查對應路徑是否存在
                    //    // \\192.168.1.155\LeoShare\Release
                    //    var fullPath = _Target + subPath;
                    //    var dirSplitIndex = fullPath.LastIndexOf(@"\");
                    //    if (!System.IO.Directory.Exists(fullPath.Substring(0, dirSplitIndex)))
                    //        System.IO.Directory.CreateDirectory(fullPath.Substring(0, dirSplitIndex));//不存在就建立目錄
                    //    File.Copy(item, _Target + subPath, true);
                    //    tempCount++;
                    //    DataCollection.Where(x => x.ThreadID == ThreadID).FirstOrDefault().Message = $"{data.Count} / {tempCount}";
                    //});
                }
                
            }
        }
        #endregion

        #region Enum
        public enum ScheduleStatus
        {
            Wait = 0,
            Running =1,
            Done = 2, 
            Error = 3
        }
        #endregion

        #region Datastruct
        class LeoQueue<T> : Queue<T>
        {
            public event EventHandler OnEnqueue;

            public new void Enqueue(T data)
            {
                base.Enqueue(data);
                if (null != OnEnqueue)
                {
                    OnEnqueue(this, null);
                }
            }

        }
        #endregion
    }
}