using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TheAirline.Model.GeneralModel.Helpers;

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
        private Timer Timer;
        public delegate void TimeChanged();
        public event TimeChanged OnTimeChanged;

        private GameTimer()
        {
            this.GameSpeed = GeneralHelpers.GameSpeedValue.Normal;
            this.Timer = new Timer();
            this.Timer.Interval = (int)this.GameSpeed;//1000;
            this.Timer.Tick += new EventHandler(Timer_Tick);
            this.OnTimeChanged += new TimeChanged(GameTimer_OnTimeChanged);
            this.Timer.Enabled = false;
        }
        //returns if the game is paused
        public Boolean isPaused()
        {
            return !this.Timer.Enabled;
        }
        //pause the game
        public void pause()
        {
            this.Timer.Enabled = false;
        }
        //(re)start the game 
        public void start()
        {
            this.Timer.Enabled = true;
        }
        //sets the speed of the game
        public void setGameSpeed(GeneralHelpers.GameSpeedValue gameSpeed)
        {
            this.GameSpeed = gameSpeed;
            this.Timer.Interval = (int)this.GameSpeed;
        }
        private void GameTimer_OnTimeChanged()
        {
            GameObjectHelpers.SimulateTurn();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (this.OnTimeChanged != null)
                this.OnTimeChanged();
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
        }



    }
}
