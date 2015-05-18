using TheAirline.Helpers.Workers;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.ObjectsModel
{
    using System;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;

    //the class for the GUI object
    public class GUIObject
    {
        #region Public Properties

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

        private GameObject getGameObject()
        {
            return GameObject.GetInstance();
        }

        #endregion
    }
}