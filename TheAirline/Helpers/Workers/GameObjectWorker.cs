using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using TheAirline.Infrastructure;
using TheAirline.Models.General;

namespace TheAirline.Helpers.Workers
{
    //the worker class for updating game object (non-graphics)
    public class GameObjectWorker : INotifyPropertyChanged
    {
        #region Static Fields

        private static GameObjectWorker _instance;

        #endregion

        #region Fields

        private readonly BackgroundWorker _worker;

        private bool _isPaused;

        #endregion

        #region Constructors and Destructors

        private GameObjectWorker()
        {
            _worker = new BackgroundWorker {WorkerReportsProgress = true, WorkerSupportsCancellation = true};

            _worker.DoWork += bw_DoWork;
            //bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            //this.Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            IsPaused = false;
            IsFinish = false;
            IsError = false;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public bool IsError { get; set; }

        public bool IsFinish { get; set; }

        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                _isPaused = value;
                NotifyPropertyChanged("IsPaused");
            }
        }

        #endregion

        #region Public Methods and Operators

        public static GameObjectWorker GetInstance()
        {
            return _instance ?? (_instance = new GameObjectWorker());
        }

        //cancels the worker
        public void Cancel()
        {
            _worker.CancelAsync();

            while (_worker.IsBusy && !IsFinish)
            {
            }
        }

        public bool IsBusy()
        {
            return _worker.IsBusy;
        }

        //pause the worker
        public void Pause()
        {
            Cancel();

            IsPaused = true;
        }

        //restarts the worker
        public void Restart()
        {
            Start();

            IsPaused = false;
        }

        //starts the worker
        public void Start()
        {
            if (!_worker.IsBusy)
            {
                IsPaused = false;

                _worker.RunWorkerAsync();
            }
        }

        //starts the worker paused
        public void StartPaused()
        {
            if (!_worker.IsBusy)
            {
                IsPaused = true;

                _worker.RunWorkerAsync();
            }
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //returns if the worker is paused

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!IsPaused && !_worker.CancellationPending)
            {
                try
                {
                    IsFinish = false;

                    var sw = new Stopwatch();

                    sw.Start();
                    GameObjectHelpers.SimulateTurn();
                    sw.Stop();

                    long waittime = (int) Settings.GetInstance().GameSpeed - (sw.ElapsedMilliseconds);

                    if (waittime > 0)
                    {
                        Thread.Sleep((int) waittime);
                    }
                }
                catch (Exception ex)
                {
                    var file = new StreamWriter(AppSettings.GetCommonApplicationDataPath() + "\\theairline.log");
                    file.WriteLine("{0}: {1} {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), ex.StackTrace);
                    file.WriteLine(ex.ToString());
                    file.WriteLine("---------GAME INFORMATION----------");
                    file.Write("Gametime: {0}, human airline: {1}", GameObject.GetInstance().GameTime.ToShortDateString(), GameObject.GetInstance().HumanAirline.Profile.Name);
                    file.Close();

                    IsError = true;
                }
            }

            IsFinish = true;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_worker.CancellationPending)
            {
                Console.WriteLine(@"Canceled!");

                IsFinish = true;
            }

            else if (e.Error != null)
            {
                //this.Cancelled = true;

                Console.WriteLine(@"Error: " + e.Error.Message);

                var lCurrentStack = new StackTrace(true);

                var file = new StreamWriter(AppSettings.GetCommonApplicationDataPath() + "\\theairlinepause.log");
                file.WriteLine(lCurrentStack.ToString());
                file.WriteLine("------------ERROR MESSAGE--------------");
                file.WriteLine(e.Error.Message);
                file.WriteLine(e.Error.StackTrace);
                file.Close();

                IsError = true;
            }

            IsFinish = true;
        }

        #endregion

        //returns the instance
    }
}