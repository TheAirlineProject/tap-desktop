namespace TheAirline.Model.GeneralModel.Helpers.WorkersModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    //the worker class for updating game object (non-graphics)
    public class GameObjectWorker : INotifyPropertyChanged
    {
        #region Static Fields

        private static GameObjectWorker Instance;

        #endregion

        #region Fields

        private readonly BackgroundWorker Worker;

        private Boolean _isPaused;

        #endregion

        #region Constructors and Destructors

        private GameObjectWorker()
        {
            this.Worker = new BackgroundWorker();

            this.Worker.WorkerReportsProgress = true;
            this.Worker.WorkerSupportsCancellation = true;
            this.Worker.DoWork += this.bw_DoWork;
            //bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            //this.Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            this.IsPaused = false;
            this.IsFinish = false;
            this.IsError = false;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Boolean IsError { get; set; }

        public Boolean IsFinish { get; set; }

        public Boolean IsPaused
        {
            get
            {
                return this._isPaused;
            }
            set
            {
                this._isPaused = value;
                this.NotifyPropertyChanged("IsPaused");
            }
        }

        #endregion

        //returns the instance

        #region Public Methods and Operators

        public static GameObjectWorker GetInstance()
        {
            if (Instance == null)
            {
                Instance = new GameObjectWorker();
            }

            return Instance;
        }

        //cancels the worker
        public void cancel()
        {
            this.Worker.CancelAsync();

            while (this.Worker.IsBusy && !this.IsFinish)
            {
            }
        }

        public Boolean isBusy()
        {
            return this.Worker.IsBusy;
        }

        public Boolean isPaused()
        {
            return this.IsPaused;
        }

        //pause the worker
        public void pause()
        {
            this.cancel();

            this.IsPaused = true;
        }

        //restarts the worker
        public void restart()
        {
            this.start();

            this.IsPaused = false;
        }

        //starts the worker
        public void start()
        {
            if (!this.Worker.IsBusy)
            {
                this.IsPaused = false;

                this.Worker.RunWorkerAsync();
            }
        }

        //starts the worker paused
        public void startPaused()
        {
            if (!this.Worker.IsBusy)
            {
                this.IsPaused = true;

                this.Worker.RunWorkerAsync();
            }
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //returns if the worker is paused

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!this.IsPaused && !this.Worker.CancellationPending)
            {
                try
                {
                    this.IsFinish = false;

                    var sw = new Stopwatch();

                    sw.Start();
                    GameObjectHelpers.SimulateTurn();
                    sw.Stop();

                    long waittime = (int)Settings.GetInstance().GameSpeed - (sw.ElapsedMilliseconds);
                  
                    if (waittime > 0)
                    {
                        Thread.Sleep((int)waittime);
                    }
                }
                catch (Exception ex)
                {
                    System.IO.StreamWriter file = new System.IO.StreamWriter(AppSettings.getCommonApplicationDataPath() + "\\theairline.log");
                    file.WriteLine("{0}: {1} {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), ex.StackTrace);
                    file.WriteLine(ex.Message);
                    file.WriteLine("---------GAME INFORMATION----------");
                    file.Write("Gametime: {0}, human airline: {1}", GameObject.GetInstance().GameTime.ToShortDateString(), GameObject.GetInstance().HumanAirline.Profile.Name);
                    file.Close();

                    this.IsError = true;

                   
                }
            }

            this.IsFinish = true;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.Worker.CancellationPending)
            {
                Console.WriteLine("Canceled!");

                this.IsFinish = true;
            }

            else if (!(e.Error == null))
            {
                //this.Cancelled = true;

                Console.WriteLine("Error: " + e.Error.Message);

                var l_CurrentStack = new StackTrace(true);

                var file = new StreamWriter(AppSettings.getCommonApplicationDataPath() + "\\theairlinepause.log");
                file.WriteLine(l_CurrentStack.ToString());
                file.WriteLine("------------ERROR MESSAGE--------------");
                file.WriteLine(e.Error.Message);
                file.WriteLine(e.Error.StackTrace);
                file.Close();

                this.IsError = true;

         
            }

            this.IsFinish = true;
        }

        #endregion
    }
}