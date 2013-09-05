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
            this.Airliner.Airliner.Airliner.clearAirlinerClasses();

            foreach (AirlinerClassMVVM aClass in this.Airliner.Classes)
            {
                AirlinerClass nClass = new AirlinerClass(aClass.Type,aClass.RegularSeatingCapacity);
                nClass.SeatingCapacity = aClass.Seating;

                foreach (AirlinerFacilityMVVM aFacility in aClass.Facilities)
                {
                    nClass.forceSetFacility(aFacility.SelectedFacility);      
 
                }
             
                this.Airliner.Airliner.Airliner.addAirlinerClass(nClass);
            }
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

             AirlinerClassMVVM tClass = this.Airliner.Classes[0];
  
             if (PopUpSingleElement.ShowPopUp("Select Class to Add", cbClasses) == PopUpSingleElement.ButtonSelected.OK && cbClasses.SelectedItem!=null)
             {
                 int maxseats = tClass.RegularSeatingCapacity - 1;

                 AirlinerClassMVVM aClass = new AirlinerClassMVVM((AirlinerClass.ClassType)cbClasses.SelectedItem, 1, 1,maxseats,true);

                 foreach (AirlinerFacilityMVVM aFacility in aClass.Facilities)
                 {
                     var facility = AirlinerFacilities.GetBasicFacility(aFacility.Type);
                     aFacility.SelectedFacility = facility;
                 }

                 this.Airliner.Classes.Add(aClass);
   
                 tClass.RegularSeatingCapacity -= aClass.RegularSeatingCapacity;

                 tClass.Seating = Convert.ToInt16(Convert.ToDouble(tClass.RegularSeatingCapacity) / tClass.Facilities.Find(f=>f.Type == AirlinerFacility.FacilityType.Seat).SelectedFacility.SeatUses);

                
             }

        }

        private void slSeats_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AirlinerClassMVVM aClass = (AirlinerClassMVVM)((Slider)sender).Tag;

            double diff = (e.NewValue - e.OldValue);// *aClass.Facilities.Find(f => f.Type == AirlinerFacility.FacilityType.Seat).SelectedFacility.SeatUses;

            AirlinerClassMVVM tClass = this.Airliner.Classes[0];

            tClass.RegularSeatingCapacity -= Convert.ToInt16(diff);

            tClass.Seating = Convert.ToInt16(Convert.ToDouble(tClass.RegularSeatingCapacity) / tClass.Facilities.Find(f => f.Type == AirlinerFacility.FacilityType.Seat).SelectedFacility.SeatUses);
            
        }

     
    }
}
