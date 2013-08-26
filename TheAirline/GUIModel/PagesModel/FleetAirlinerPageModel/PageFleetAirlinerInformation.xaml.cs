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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel
{
    /// <summary>
    /// Interaction logic for PageFleetAirlinerInformation.xaml
    /// </summary>
    public partial class PageFleetAirlinerInformation : Page
    {
        public FleetAirlinerMVVM Airliner { get; set; }
        public PageFleetAirlinerInformation(FleetAirlinerMVVM airliner)
        {
            this.Airliner = airliner;

            this.DataContext = this.Airliner;
            this.Loaded += PageFleetAirlinerInformation_Loaded;
            InitializeComponent();
        }

        private void PageFleetAirlinerInformation_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (AirlinerClassMVVM aClass in this.Airliner.Classes)
            {
                foreach (AirlinerFacilityMVVM aFacility in aClass.Facilities)
                {
                    var facility = this.Airliner.Airliner.Airliner.getAirlinerClass(aClass.Type).getFacility(aFacility.Type);
                    aFacility.SelectedFacility = facility;
                }

            }
        }

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnUndoChanges_Click(object sender, RoutedEventArgs e)
        {
            var classes = new List<AirlinerClassMVVM>(this.Airliner.Classes);

            foreach (AirlinerClassMVVM aClass in classes.Where(c=>!this.Airliner.Airliner.Airliner.Classes.Exists(ac=>ac.Type == c.Type)))
            {
                this.Airliner.Classes.Remove(aClass);
            }

            foreach (AirlinerClassMVVM aClass in this.Airliner.Classes)
            {
                foreach (AirlinerFacilityMVVM aFacility in aClass.Facilities)
                {
                    var facility = this.Airliner.Airliner.Airliner.getAirlinerClass(aClass.Type).getFacility(aFacility.Type);
                    aFacility.SelectedFacility = facility;
                }

            }
        }

        private void btnAddClass_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cbClasses = new ComboBox();
            cbClasses.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbClasses.ItemTemplate = this.Resources["AirlinerClassItem"] as DataTemplate;
            cbClasses.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbClasses.Width = 200;

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                Boolean hasClass = this.Airliner.Airliner.Airliner.Classes.Exists(c => c.Type == type);
                if ((int)type <= GameObject.GetInstance().GameTime.Year && !hasClass)
                {
                    cbClasses.Items.Add(type);
                }
            }

             cbClasses.SelectedIndex = 0;


             if (PopUpSingleElement.ShowPopUp("Select Class to Add", cbClasses) == PopUpSingleElement.ButtonSelected.OK && cbClasses.SelectedItem!=null)
             {
                 AirlinerClassMVVM aClass = new AirlinerClassMVVM((AirlinerClass.ClassType)cbClasses.SelectedItem, 1, 1);
                 this.Airliner.Classes.Add(aClass);

                 foreach (AirlinerFacilityMVVM aFacility in aClass.Facilities)
                 {
                     var facility = AirlinerFacilities.GetBasicFacility(aFacility.Type);
                     aFacility.SelectedFacility = facility;
                 }
             }

        }
    }
}
