using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Collections;

namespace TheAirline.Model.GeneralModel.Helpers.WorkersModel
{
    //the worker class for updating game object (non-graphics)
    public class GameObjectWorker : INotifyPropertyChanged
    {
        private static GameObjectWorker Instance;
        private BackgroundWorker Worker;
        private Boolean Cancelled;
        private Boolean CancelWorker;
        private Boolean _isPaused;
        public Boolean IsPaused
        {
            get { return _isPaused; }
            set { _isPaused = value; NotifyPropertyChanged("IsPaused"); }
        }
        public Boolean Sleeping { get; set; }
        public Boolean IsStarted { get; set; }
        private GameObjectWorker()
        {
            this.Worker = new BackgroundWorker();

            this.Worker.WorkerReportsProgress = true;
            this.Worker.WorkerSupportsCancellation = true;
            this.Worker.DoWork += new DoWorkEventHandler(bw_DoWork);
            //bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            this.Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            this.Cancelled = false;
            this.CancelWorker = false;
            this.IsPaused = false;
            this.IsStarted = true;
            this.Sleeping = false;
        }
        //returns the instance
        public static GameObjectWorker GetInstance()
        {
            if (Instance == null)
                Instance = new GameObjectWorker();

            return Instance;
        }
        //cancels the worker
        public void cancel()
        {
            //if (this.Worker.WorkerSupportsCancellation)
                //this.Worker.CancelAsync();
            this.CancelWorker = true;
            //this.Cancelled = true;
           
           
        }
        //pause the worker
        public void pause()
        {
            this.IsPaused = true;
        }
        //restarts the worker
        public void restart()
        {
            this.IsPaused = false;
        }
        //starts the worker
        public void start()
        {
            if (!this.Worker.IsBusy)
            {
                this.IsStarted = true;
                this.Cancelled = false;
                this.CancelWorker = false;
  
                this.Worker.RunWorkerAsync();
            }
        }
        //starts the worker paused
        public void startPaused()
        {
            if (!this.Worker.IsBusy)
            {
                this.IsStarted = false;
                this.IsPaused = true;
                this.Cancelled = false;
                this.CancelWorker = false;

                this.Worker.RunWorkerAsync();
            }
        }
        //returns if the worker is cancelled
        public Boolean isCancelled()
        {
            return this.Cancelled;
        }
        //returns if the worker is paused
        public Boolean isPaused()
        {
            return this.IsPaused;
        }
        //returns if the worker is busy
        public Boolean isBusy()
        {
            return !this.Cancelled && this.Worker.IsBusy;
        }
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            
            if (!IsPaused)
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();
                GameObjectHelpers.SimulateTurn();
                sw.Stop();

                long waittime = (int)GameTimer.GetInstance().GameSpeed - (sw.ElapsedMilliseconds);

              
                /*
                if ((this.Worker.CancellationPending == true))
                {
                    e.Cancel = true;
                
                }*/

                if (waittime > 0 && !this.CancelWorker)
                {
                    this.Sleeping = true;
                    Thread.Sleep((int)waittime);
                    this.Sleeping = false;
                }
            }

        }
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((this.CancelWorker))
            {
                this.Cancelled = true;
                Console.WriteLine("Canceled!");
            }

            else if (!(e.Error == null))
            {
                //this.Cancelled = true;
                
                Console.WriteLine("Error: " + e.Error.Message);

                var l_CurrentStack = new System.Diagnostics.StackTrace(true);

                System.IO.StreamWriter file = new System.IO.StreamWriter(AppSettings.getCommonApplicationDataPath() + "\\theairlinepause.log");
                file.WriteLine(l_CurrentStack.ToString());
                file.WriteLine("------------ERROR MESSAGE--------------");
                file.WriteLine(e.Error.Message);
                file.WriteLine(e.Error.StackTrace);
                file.Close();

          
                this.Worker.RunWorkerAsync();
            }

            else
            {
                this.Worker.RunWorkerAsync();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
