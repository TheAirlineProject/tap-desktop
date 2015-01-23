using Seatplanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpSeatConfiguration.xaml
    /// </summary>
    public partial class PopUpSeatConfiguration : PopUpWindow
    {
        public SeatPlannerAirliner Airliner { get; set; }
        public string Text { get; set; }
        public static void ShowPopUp(Airliner airliner)
        {
            PopUpWindow window = new PopUpSeatConfiguration(airliner);
            window.ShowDialog();
        }
        public PopUpSeatConfiguration(Airliner airliner)
        {
            SeatPlannerModel model = AirlinerSeatModels.GetSeatModel(airliner.Type);

            List<SeatPlannerAirlinerClass> classes = new List<SeatPlannerAirlinerClass>();

            foreach (AirlinerClass aClass in airliner.Classes)
            {
                double seatspace = 1;

                SeatPlannerAirlinerClass.ClassType classType = SeatPlannerAirlinerClass.ClassType.Economy_Class;

                if (aClass.Type == AirlinerClass.ClassType.Business_Class)
                    classType = SeatPlannerAirlinerClass.ClassType.Business_Class;

                if (aClass.Type == AirlinerClass.ClassType.Premium_Economy_Class)
                    classType = SeatPlannerAirlinerClass.ClassType.Premium_Economy_Class;

                if (aClass.Type == AirlinerClass.ClassType.First_Class)
                    classType = SeatPlannerAirlinerClass.ClassType.First_Class;

                if (aClass.getFacility(AirlinerFacility.FacilityType.Seat).SeatUses > 1 && aClass.getFacility(AirlinerFacility.FacilityType.Seat).SeatUses < 3)
                    seatspace = 2;

                if (aClass.getFacility(AirlinerFacility.FacilityType.Seat).SeatUses >= 3)
                    seatspace = 3;

                SeatPlannerAirlinerClass plannerClass = new SeatPlannerAirlinerClass(classType, aClass.SeatingCapacity,seatspace);

                classes.Add(plannerClass);
            }

      
            SeatPlannerAirliner airlinerModel = new SeatPlannerAirliner(classes, model);

            this.Airliner = airlinerModel;

            this.Text = string.Format("{0} ({1})", airliner.Type.Name, airliner.CabinConfiguration);

            InitializeComponent();
        }
    }
}
