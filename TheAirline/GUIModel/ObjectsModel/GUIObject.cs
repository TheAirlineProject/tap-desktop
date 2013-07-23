using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.ObjectsModel
{
    //the class for the GUI object
    public class GUIObject
    {
        public GameObject GameObject { get { return getGameObject(); } private set { ;} }
        private GameObject getGameObject()
        {
            return GameObject.GetInstance();

        }
    }
}
