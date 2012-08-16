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
        private Boolean IsFinished;
        private BackgroundWorker Worker;
        private PassengersTimer()
        {
            this.Worker = new BackgroundWorker();
            this.Worker.WorkerSupportsCancellation = true;
            this.Worker.DoWork += new DoWorkEventHandler(Timer_DoWork);
        }

  
        private void Timer_DoWork(object sender, DoWorkEventArgs e)
        {
            this.IsFinished = false;

            int i = 0;
            while (!this.Worker.CancellationPending && i<100 && Passengers.Count() < Passengers.GetMaxPassengers())
            {
                //PassengerHelpers.CreatePassengers(1);

                i++;
            }

            //if (!this.Worker.CancellationPending)
              //  PassengerHelpers.UpdatePassengers();

         
           
            e.Cancel = true;

            this.IsFinished = true;
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
            PassengerHelpers.CreateDestinationPassengers();
            /*
            if (!this.Worker.IsBusy)
                this.Worker.RunWorkerAsync();
             * */
        }
        //returns if the worker is busy
        public Boolean isBusy()
        {
            return this.Worker.IsBusy;
        }
        //returns if the worker is finished
        public Boolean isFinished()
        {
            return this.IsFinished;
        }
    }
}
