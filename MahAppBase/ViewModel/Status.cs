using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MahAppBase.ViewModel
{
    /// <summary>
    /// 框架狀態，包含目前記憶體、CPU使用率
    /// </summary>
    public class Status : ViewModelBase
    {
        #region Declarations
        private float _Cpu;
        private float _Memory;
        private int _UpdateFrequence = 10;
        protected bool _IsGetInfo = false;
        private Task thUpdatStatus;
        #endregion

        #region Property
        /// <summary>
        /// 程式CPU使用率
        /// </summary>
        public float Cpu
        {
            get
            {
                return _Cpu;
            }
            set
            {
                _Cpu = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 程式使用記憶體
        /// </summary>
        public float Memory
        {
            get { return _Memory; }
            set { _Memory = value; OnPropertyChanged(); }
        }

        public bool IsGetInfo
        {
            get
            {
                return _IsGetInfo;
            }
            set
            {
                _IsGetInfo = value;
                OnPropertyChanged();
            }
        }

        public int UpdateFrequence
        {
            get
            {
                return _UpdateFrequence;
            }
            set
            {
                _UpdateFrequence = value;
                OnPropertyChanged("Cpu");
            }
        }

        private string _SoundSource = string.Empty;
        public string SoundSource
        {
            get
            {
                return "/Cat.wav";
            }
            private set { SoundSource = value; }
        }
        #endregion

        #region Memberfunction
        /// <summary>
        /// ViewModel建構子
        /// </summary>
        public Status()
        {
            //初始化、啟動獲取程式狀態Task
            this.thUpdatStatus = new Task(() => { CatchPcStatus(); });
            thUpdatStatus.Start();
        }

        public void CatchPcStatus()
        {
            var name = Process.GetCurrentProcess().ProcessName;
            var cpuCounter = new PerformanceCounter("Process", "% Processor Time", name);
            var ramCounter = new PerformanceCounter("Process", "Working Set", name);
            

            while (true)
            {
                if (IsGetInfo)
                {
                    try
                    {
                        Cpu = cpuCounter.NextValue();
                        Memory = float.Parse((ramCounter.NextValue() / 1e+6).ToString());
                    }
                    catch (Exception)
                    {
                        Cpu = 0;
                        Memory = 0;
                    }
                    finally
                    {
                        Thread.Sleep(UpdateFrequence*1000);
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }
        #endregion
    }
}
