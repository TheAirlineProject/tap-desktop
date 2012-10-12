using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using TheAirline.Model.AirlineModel;

namespace TheAirline.Model.GeneralModel.Helpers.WorkersModel
{
    class AIWorker
    {
        private static AIWorker Instance;
        private BackgroundWorker Worker;
        private AIWorker()
        {
            this.Worker = new BackgroundWorker();

            this.Worker.WorkerReportsProgress = true;
            this.Worker.WorkerSupportsCancellation = true;
            this.Worker.DoWork += new DoWorkEventHandler(bw_DoWork);
            //bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

        }
        //returns the instance
        public static AIWorker GetInstance()
        {
            if (Instance == null)
                Instance = new AIWorker();

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
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                if ((this.Worker.CancellationPending == true))
                {
                    e.Cancel = true;

                }
                else
                {
                    if (!airline.IsHuman)
                        AIHelpers.UpdateCPUAirline(airline);

                }
            }
                 
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
