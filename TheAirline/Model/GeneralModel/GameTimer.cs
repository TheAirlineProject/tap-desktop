using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TheAirline.Model.GeneralModel.Helpers;
using System.Windows.Threading;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;
using System.ComponentModel;


namespace TheAirline.Model.GeneralModel
{

    /*! GameTimer.
* This is used for the game timer which simulates a game "round"
* The class needs no parameters
*/
   
     
    public class GameTimer 
    {
        private static GameTimer gameTimer;

        public GeneralHelpers.GameSpeedValue GameSpeed { get; private set; }
        private DispatcherTimer Timer;
        public delegate void TimeChanged();
        public event TimeChanged OnTimeChanged;
        public event TimeChanged OnTimeChangedForced; //will be forced to update eventhough the game is paused
        private Boolean _isPaused;
        public Boolean IsPaused
        {
            get { return _isPaused; }
            set { _isPaused = value; }
        }
        private GameTimer()
        {

            this.GameSpeed = GeneralHelpers.GameSpeedValue.Normal;
            this.Timer = new DispatcherTimer();
            this.Timer.Interval = new TimeSpan(0, 0, 0, 0,100);//(int)this.GameSpeed;
            this.Timer.Tick += new EventHandler(Timer_Tick);
            this.OnTimeChanged += new TimeChanged(GameTimer_OnTimeChanged);
            this.Timer.IsEnabled = false;
            this.IsPaused = false;
        }
        //returns if the game is paused
        public Boolean isPaused()
        {
            return this.IsPaused;//!this.Timer.Enabled;
        }
        //pause the game
        public void pause()
        {
           this.IsPaused = true;
        }
        //(re)start the game 
        public void start()
        {
            if (!this.Timer.IsEnabled)
                this.Timer.IsEnabled = true;

            this.IsPaused = false;
        }
        //sets the speed of the game
        public void setGameSpeed(GeneralHelpers.GameSpeedValue gameSpeed)
        {
            this.GameSpeed = gameSpeed;
        }
        private void GameTimer_OnTimeChanged()
        {
            if (!GameObjectWorker.GetInstance().IsStarted)
                GameObjectWorker.GetInstance().start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!IsPaused)
            {
                if (this.OnTimeChanged != null && !this.IsPaused)
                    this.OnTimeChanged();

                if (this.OnTimeChangedForced != null)
                    this.OnTimeChangedForced();
            }
        }
        //starts the timer
        public static void StartTimer()
        {
            if (gameTimer == null)
                gameTimer = new GameTimer();
        }
        //returns the instance of the timer
        public static GameTimer GetInstance()
        {
            if (gameTimer == null)
                gameTimer = new GameTimer();
            return gameTimer;
        }
        //restarts the instance
        public static void RestartInstance()
        {
            GetInstance().pause();
            gameTimer = new GameTimer();
            GetInstance().start();
        }
          
       


    }
     
}

