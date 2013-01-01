using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace TheAirline.Model.GeneralModel.Helpers.WorkersModel
{
    //the worker class for updating game object (non-graphics)
    public class GameObjectWorker
    {
        private static GameObjectWorker Instance;
        private BackgroundWorker Worker;
        private Boolean Cancelled;
        private Boolean CancelWorker;
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
        }
        //starts the worker
        public void start()
        {
            if (!this.Worker.IsBusy)
            {
                this.Cancelled = false;
                this.CancelWorker = false;
  
                this.Worker.RunWorkerAsync();
            }
        }
        //returns if the worker is busy
        public Boolean isBusy()
        {
            return this.Cancelled;
        }
        private void bw_DoWork(object sender, DoWorkEventArgs e)
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
                Thread.Sleep((int)waittime);

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
                this.Cancelled = true;
                Console.WriteLine("Error: " + e.Error.Message);
            }

            else
            {
                this.Worker.RunWorkerAsync();
            }
        }

    }
}
