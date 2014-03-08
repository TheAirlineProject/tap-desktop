using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers.WorkersModel;

namespace TheAirline.GUIModel.ObjectsModel
{
    //the class for the GUI object
    public class GUIObject
    {
        
        public GameObjectWorker GameWorker { get { return GameObjectWorker.GetInstance(); } private set { ;} }
        public GameObject GameObject { get { return getGameObject(); } private set { ;} }
        public Boolean NavigatorCanGoBack { get { return PageNavigator.CanGoBack();} private set { ;} }
        private GameObject getGameObject()
        {
            return GameObject.GetInstance();
        

        }
    }
}
