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
        private GameObjectWorker()
        {
            this.Worker = new BackgroundWorker();

            this.Worker.WorkerReportsProgress = true;
            this.Worker.WorkerSupportsCancellation = true;
            this.Worker.DoWork += new DoWorkEventHandler(bw_DoWork);
            //bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            this.Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

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
            if (this.Worker.WorkerSupportsCancellation)
                this.Worker.CancelAsync();

        }
        //starts the worker
        public void start()
        {
            if (!this.Worker.IsBusy)
                this.Worker.RunWorkerAsync();
        }
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            GameObjectHelpers.SimulateTurn();
            sw.Stop();

            long waittime = 1000 - (sw.ElapsedMilliseconds);

            if (waittime>0 && !e.Cancel)
                Thread.Sleep((int)waittime);

        }
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                Console.WriteLine("Canceled!");
            }

            else if (!(e.Error == null))
            {
                Console.WriteLine("Error: " + e.Error.Message);
            }

            else
            {
                this.Worker.RunWorkerAsync();
            }
        }
      
    }
}
