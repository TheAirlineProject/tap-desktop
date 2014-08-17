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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpShowSpecialContract.xaml
    /// </summary>
    public partial class PopUpShowSpecialContract : PopUpWindow
    {
        public List<RequirementMVVM> Requirements { get; set; }
        public List<SpecialRouteMVVM> Routes { get; set; }
        public string ContractName { get; set; }
        public static object ShowPopUp(SpecialContract contract)
        {
            PopUpWindow window = new PopUpShowSpecialContract(contract);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpShowSpecialContract(SpecialContract contract)
        {
            this.ContractName = contract.Type.Name;
            this.Content = contract;

            this.Requirements = new List<RequirementMVVM>();
            this.Routes = new List<SpecialRouteMVVM>();

            foreach (ContractRequirement requirement in contract.Type.Requirements)
            {   
                string text="";
                string value="";

                if (requirement.Type == ContractRequirement.RequirementType.ClassType)
                {
                    text = new TextUnderscoreConverter().Convert(requirement.ClassType).ToString();
                    value = string.Format("{0} passengers",requirement.MinSeats);
                }
                else if (requirement.Type == ContractRequirement.RequirementType.Destination)
                {
                    text = requirement.Departure.Profile.Name;
                    value = requirement.Destination.Profile.Name;
                }

                this.Requirements.Add(new RequirementMVVM(requirement.Type, text,value));
            }

            foreach (SpecialContractRoute route in contract.Type.Routes)
            {
                this.Routes.Add(new SpecialRouteMVVM(route.Departure, route.Destination));

                if (route.BothWays)
                    this.Routes.Add(new SpecialRouteMVVM(route.Destination, route.Departure));
            }

            InitializeComponent();
        }
        public class RequirementMVVM
        {
            public ContractRequirement.RequirementType Type { get; set; }
            public string Text { get; set; }
            public string Requirement { get; set; }

            public RequirementMVVM(ContractRequirement.RequirementType type, string text, string requirement)
            {
                this.Text = text;
                this.Requirement = requirement;
                this.Type = type;
            }
        }
        public class SpecialRouteMVVM
        {
            public Airport Departure { get; set; }
            public Airport Destination { get; set; }
            public SpecialRouteMVVM(Airport departure, Airport destination)
            {
                this.Departure = departure;
                this.Destination = destination;
            }
        }
    }

}
