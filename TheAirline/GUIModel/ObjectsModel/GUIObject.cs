namespace TheAirline.GUIModel.ObjectsModel
{
    using System;
    using System.ComponentModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers.WorkersModel;

    //the class for the GUI object
    public class GUIObject : INotifyPropertyChanged
    {
        public GUIObject()
        {
            this.IsPaused = GameObjectWorker.GetInstance().IsPaused;
        }
        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        #region private properties
        private Boolean _ispaused;
        #endregion
        #region Public Properties
        public Boolean IsPaused
        {
            get
            {
                return this._ispaused;
            }
            set
            {
                this._ispaused = value;
                this.NotifyPropertyChanged("IsPaused");
            }
        }
        public GameObject GameObject
        {
            get
            {
                return this.getGameObject();
            }
            private set
            {
                ;
            }
        }
       
        public GameObjectWorker GameWorker
        {
            get
            {
                return GameObjectWorker.GetInstance();
            }
            private set
            {
                ;
            }
        }

        public Boolean NavigatorCanGoBack
        {
            get
            {
                return PageNavigator.CanGoBack();
            }
            private set
            {
                ;
            }
        }

        #endregion

        #region Methods
      public void setPaused(Boolean paused)
        {
          this.IsPaused = paused;

          if (paused)
              GameObjectWorker.GetInstance().pause();
          else
              GameObjectWorker.GetInstance().restart();
        }
        private GameObject getGameObject()
        {
            return GameObject.GetInstance();
        }
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}