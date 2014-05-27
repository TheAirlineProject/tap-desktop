namespace TheAirline.Model.GeneralModel.Helpers.WorkersModel
{
    using System;
    using System.ComponentModel;

    using TheAirline.Model.AirlineModel;

    internal class AIWorker
    {
        #region Static Fields

        private static AIWorker Instance;

        #endregion

        #region Fields

        private readonly BackgroundWorker Worker;

        #endregion

        #region Constructors and Destructors

        private AIWorker()
        {
            this.Worker = new BackgroundWorker();

            this.Worker.WorkerReportsProgress = true;
            this.Worker.WorkerSupportsCancellation = true;
            this.Worker.DoWork += this.bw_DoWork;
            //bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            this.Worker.RunWorkerCompleted += this.bw_RunWorkerCompleted;
        }

        #endregion

        //returns the instance

        #region Public Methods and Operators

        public static AIWorker GetInstance()
        {
            if (Instance == null)
            {
                Instance = new AIWorker();
            }

            return Instance;
        }

        //cancels the worker
        public void cancel()
        {
            if (this.Worker.WorkerSupportsCancellation)
            {
                this.Worker.CancelAsync();
            }
        }

        //starts the worker
        public void start()
        {
            if (!this.Worker.IsBusy)
            {
                this.Worker.RunWorkerAsync();
            }
        }

        #endregion

        #region Methods

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                if (this.Worker.CancellationPending)
                {
                    e.Cancel = true;
                }
                else
                {
                    if (!airline.IsHuman)
                    {
                        AIHelpers.UpdateCPUAirline(airline);
                    }
                }
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
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

        #endregion
    }
}