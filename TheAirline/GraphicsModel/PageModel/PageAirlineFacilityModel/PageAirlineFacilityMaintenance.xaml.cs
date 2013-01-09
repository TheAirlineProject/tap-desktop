using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineFacilityModel
{
    /// <summary>
    /// Interaction logic for PageAirlineFacilityMaintenance.xaml
    /// </summary>
    public partial class PageAirlineFacilityMaintenance : StandardPage
    {
        private Button btnEquipped, btnApply;
        private AirlineFacility AirlineFacility;
        private List<FleetAirliner> AirlinersToMaintain;
        private List<AirlinerClass> Classes;
        private StackPanel panelCurrentClasses;
        public PageAirlineFacilityMaintenance(AirlineFacility facility)
        {
            this.AirlinersToMaintain = new List<FleetAirliner>();
            this.AirlineFacility = facility;
            this.Classes = new List<AirlinerClass>();

            InitializeComponent();

            StackPanel facilityPanel = new StackPanel();
            facilityPanel.Margin = new Thickness(10, 0, 10, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirlineFacilityMaintenance", txtHeader.Uid);

            facilityPanel.Children.Add(txtHeader);

            ContentControl ccHeader = new ContentControl();
            ccHeader.ContentTemplate = this.Resources["FleetHeader"] as DataTemplate;
            ccHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            facilityPanel.Children.Add(ccHeader);

            ListBox lbAirliners = new ListBox();
            lbAirliners.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbAirliners.ItemTemplate = this.Resources["AirlinerItem"] as DataTemplate;
            lbAirliners.MaxHeight = GraphicsHelpers.GetContentHeight() / 2;

            foreach (FleetAirliner airliner in GameObject.GetInstance().HumanAirline.DeliveredFleet)
                lbAirliners.Items.Add(airliner);

            facilityPanel.Children.Add(lbAirliners);
            facilityPanel.Children.Add(createClassesPanel());
            facilityPanel.Children.Add(createButtonsPanel());

            StandardContentPanel panelContent = new StandardContentPanel();

            panelContent.setContentPage(facilityPanel, StandardContentPanel.ContentLocation.Left);

            base.setContent(panelContent);

            base.setHeaderContent(this.AirlineFacility.Name);

            showPage(this);
        }
        //the panel for defining the classes
        private StackPanel createClassesPanel()
        {
            StackPanel panelClasses = new StackPanel();
            panelClasses.Margin = new Thickness(0, 10, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1002";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirlineFacilityMaintenance", txtHeader.Uid);

            panelClasses.Children.Add(txtHeader);

            panelCurrentClasses = new StackPanel();
            panelClasses.Children.Add(panelCurrentClasses);
            
            return panelClasses;
        }
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);

            btnEquipped = new Button();
            btnEquipped.Uid = "201";
            btnEquipped.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnEquipped.Height = Double.NaN;
            btnEquipped.Width = Double.NaN;
            btnEquipped.Content = Translator.GetInstance().GetString("PageAirlineFacilityMaintenance", btnEquipped.Uid);
            btnEquipped.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnEquipped.Click += btnEquipped_Click;
            btnEquipped.IsEnabled = false;

            panelButtons.Children.Add(btnEquipped);

            btnApply = new Button();
            btnApply.Uid = "202";
            btnApply.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnApply.Height = Double.NaN;
            btnApply.Width = Double.NaN;
            btnApply.Content = Translator.GetInstance().GetString("PageAirlineFacilityMaintenance", btnApply.Uid);
            btnApply.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnApply.IsEnabled = false;
            btnApply.Margin = new Thickness(5, 0, 0, 0);
            btnApply.Click += btnApply_Click;

            panelButtons.Children.Add(btnApply);

            return panelButtons;


        }

        //shows the classes
        private void showClasses()
        {
            panelCurrentClasses.Children.Clear();

            foreach (AirlinerClass aClass in this.Classes)
            {
                panelCurrentClasses.Children.Add(createAirlineClassLink(aClass));
            }

        }
        //creates a hyperlink for an airliner class
        private TextBlock createAirlineClassLink(AirlinerClass aClass)
        {
            TextBlock txtLink = new TextBlock();
            txtLink.Margin = new Thickness(0, 0, 20, 0);

            Hyperlink link = new Hyperlink();
            link.Tag = aClass;
            link.Click += link_Click;
            link.Inlines.Add(new TextUnderscoreConverter().Convert(aClass.Type).ToString());
            txtLink.Inlines.Add(link);

            return txtLink;

        }
        private void cbAirliner_Checked(object sender, RoutedEventArgs e)
        {
            int maxNumberOfAirliners = this.AirlineFacility.ServiceLevel;

            FleetAirliner airliner = (FleetAirliner)((CheckBox)sender).Tag;

            if (this.AirlinersToMaintain.Count == 0 || (this.AirlinersToMaintain.Exists(a => a.Airliner.Type == airliner.Airliner.Type) && this.AirlinersToMaintain.Count + 1 <= maxNumberOfAirliners))
            {
                this.AirlinersToMaintain.Add(airliner);

                AirlinerType type = this.AirlinersToMaintain[0].Airliner.Type;
                if (this.Classes == null || this.Classes.Count == 0)
                {
                    AirlinerClass eClass = new AirlinerClass(AirlinerClass.ClassType.Economy_Class, ((AirlinerPassengerType)type).MaxSeatingCapacity);
                    eClass.createBasicFacilities(null);
                    this.Classes.Add(eClass);

                    showClasses();
                }

                btnEquipped.IsEnabled = true;
                btnApply.IsEnabled = true;
            }
            else
            {
                if (this.AirlinersToMaintain.Count == maxNumberOfAirliners)
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2901"), string.Format(Translator.GetInstance().GetString("MessageBox", "2901","message"), maxNumberOfAirliners), WPFMessageBoxButtons.Ok);

                ((CheckBox)sender).IsChecked = false;
            }
        }

        private void cbAirliner_Unchecked(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((CheckBox)sender).Tag;

            this.AirlinersToMaintain.Remove(airliner);

            if (this.AirlinersToMaintain.Count == 0)
            {
                this.Classes.Clear();

                showClasses();

                btnEquipped.IsEnabled = false;
                btnApply.IsEnabled = false;
            }
        }
        private void link_Click(object sender, RoutedEventArgs e)
        {
            AirlinerType type = this.AirlinersToMaintain[0].Airliner.Type;
            if (this.Classes == null)
            {
                AirlinerClass eClass = new AirlinerClass(AirlinerClass.ClassType.Economy_Class, ((AirlinerPassengerType)type).MaxSeatingCapacity);
                eClass.createBasicFacilities(null);
                this.Classes.Add(eClass);
            }

            AirlinerClass aClass = (AirlinerClass)((Hyperlink)sender).Tag;

            AirlinerClass newClass = (AirlinerClass)PopUpAirlinerClassConfiguration.ShowPopUp(aClass);

            AirlinerClass economyClass = this.Classes.Find(c => c.Type == AirlinerClass.ClassType.Economy_Class);
            if (newClass != null)
            {
                AirlinerClass airlinerClass = new AirlinerClass(newClass.Type, newClass.SeatingCapacity);
                airlinerClass.RegularSeatingCapacity = newClass.RegularSeatingCapacity;

                int seatingDiff = ((AirlinerPassengerType)type).MaxSeatingCapacity;

                economyClass.RegularSeatingCapacity += seatingDiff;

                AirlinerFacility seatingFacility = economyClass.getFacility(AirlinerFacility.FacilityType.Seat);

                int extraSeats = (int)(seatingDiff / seatingFacility.SeatUses);

                economyClass.SeatingCapacity += extraSeats;

            }

        }
        private void btnEquipped_Click(object sender, RoutedEventArgs e)
        {
            AirlinerType type = this.AirlinersToMaintain[0].Airliner.Type;

            List<AirlinerClass> classes = (List<AirlinerClass>)PopUpAirlinerConfiguration.ShowPopUp(type, this.Classes);

            if (classes != null)
            {
                this.Classes = classes;

                showClasses();
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            int serviceDays = 3;
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2902"), string.Format(Translator.GetInstance().GetString("MessageBox", "2902","message"), serviceDays), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                foreach (FleetAirliner airliner in this.AirlinersToMaintain)
                {
                    serviceDays = airliner.Airliner.Classes.Count == this.Classes.Count ? 1 : 3;
                    
                    airliner.Airliner.clearAirlinerClasses();

                    foreach (AirlinerClass aClass in this.Classes)
                    {
                        AirlinerClass tClass = new AirlinerClass(aClass.Type, aClass.SeatingCapacity);
                        tClass.RegularSeatingCapacity = aClass.RegularSeatingCapacity;

                        foreach (AirlinerFacility facility in aClass.getFacilities())
                            tClass.setFacility(GameObject.GetInstance().HumanAirline, facility);

                        airliner.Airliner.addAirlinerClass(tClass);
                    }
                    airliner.GroundedToDate = GameObject.GetInstance().GameTime.AddDays(serviceDays);
                }
            }

        }
    }
}
