using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.PassengerModel
{
    /*! PassengersTimer.
* This is used for the timer for managing the passengers
* The class needs no parameters
*/
    public class PassengersTimer
    {
        private static PassengersTimer WorkerInstance;

        private BackgroundWorker Worker;
        private PassengersTimer()
        {
            this.Worker = new BackgroundWorker();
            this.Worker.WorkerSupportsCancellation = true;
            this.Worker.DoWork += new DoWorkEventHandler(Timer_DoWork);
            this.Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Timer_RunWorkerCompleted);
        }

        private void Timer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
           // if (!this.Worker.IsBusy)
             //   this.Worker.RunWorkerAsync();
        }

        private void Timer_DoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
            while (!this.Worker.CancellationPending && i<100)
            {
                PassengerHelpers.CreatePassengers(1);
            }
            e.Cancel = true;
        }

       
        //returns the instance of the timer
        public static PassengersTimer GetInstance()
        {
            if (WorkerInstance == null)
                WorkerInstance = new PassengersTimer();
            return WorkerInstance;
        }

        //cancels the worker
        public void cancel()
        {
            this.Worker.CancelAsync();
        }
        //starts the worker 
        public void start()
        {
            if (!this.Worker.IsBusy)
                this.Worker.RunWorkerAsync();
        }
    }
}
