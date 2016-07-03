using System;
using System.ComponentModel;
using TheAirline.Models.Airlines;

namespace TheAirline.Helpers.Workers
{
    internal class AIWorker
    {
        #region Static Fields

        private static AIWorker _instance;

        #endregion

        #region Fields

        private readonly BackgroundWorker _worker;

        #endregion

        #region Constructors and Destructors

        private AIWorker()
        {
            _worker = new BackgroundWorker {WorkerReportsProgress = true, WorkerSupportsCancellation = true};

            _worker.DoWork += bw_DoWork;
            //bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            _worker.RunWorkerCompleted += bw_RunWorkerCompleted;
        }

        #endregion

        #region Public Methods and Operators

        public static AIWorker GetInstance()
        {
            return _instance ?? (_instance = new AIWorker());
        }

        //cancels the worker
        public void Cancel()
        {
            if (_worker.WorkerSupportsCancellation)
            {
                _worker.CancelAsync();
            }
        }

        //starts the worker
        public void Start()
        {
            if (!_worker.IsBusy)
            {
                _worker.RunWorkerAsync();
            }
        }

        #endregion

        #region Methods

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (Airline airline in Airlines.GetAllAirlines())
            {
                if (_worker.CancellationPending)
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
                Console.WriteLine(@"Canceled!");
            }

            else if (e.Error != null)
            {
                Console.WriteLine(@"Error: " + e.Error.Message);
            }

            else
            {
                _worker.RunWorkerAsync();
            }
        }

        #endregion

        //returns the instance
    }
}