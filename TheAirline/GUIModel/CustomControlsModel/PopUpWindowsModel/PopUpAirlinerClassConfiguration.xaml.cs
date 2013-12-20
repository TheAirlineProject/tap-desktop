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
using System.Windows.Shapes;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlinerModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirlinerClassConfiguration.xaml
    /// </summary>
    public partial class PopUpAirlinerClassConfiguration : PopUpWindow
    {
        private AirlinerClass AirlinerClass;
        private ListBox[] lbFacilities;
        public static object ShowPopUp(AirlinerClass aClass)
        {
            PopUpWindow window = new PopUpAirlinerClassConfiguration(aClass);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpAirlinerClassConfiguration(AirlinerClass aClass)
        {
            
            InitializeComponent();

            this.AirlinerClass = aClass;

            this.Title = new TextUnderscoreConverter().Convert(aClass.Type.ToString()).ToString();

            this.Width = 400;

            this.Height = 200;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel panelClassFacilities = new StackPanel();

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = string.Format("{0}", new TextUnderscoreConverter().Convert(aClass.Type, null, null, null));

            panelClassFacilities.Children.Add(txtHeader);

            lbFacilities = new ListBox[Enum.GetValues(typeof(AirlinerFacility.FacilityType)).Length];

            int i = 0;
            foreach (AirlinerFacility.FacilityType type in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
            {
                AirlinerFacility facility = aClass.getFacility(type);
                
                lbFacilities[i] = new ListBox();
                lbFacilities[i].ItemContainerStyleSelector = new ListBoxItemStyleSelector();
                lbFacilities[i].ItemTemplate = this.Resources["FleetFacilityItem"] as DataTemplate;

                panelClassFacilities.Children.Add(lbFacilities[i]);

                i++;

            }
            panelClassFacilities.Children.Add(createButtonsPanel());


            this.Content = panelClassFacilities;

            showFacilities();
        }
        //creates the buttons menu
        private WrapPanel createButtonsPanel()
        {
           WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);

            Button btnOk = new Button();
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = 16;
            btnOk.Width = Double.NaN;
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.Content = "OK";
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = 16;
            btnCancel.Width = Double.NaN;
            btnCancel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.Visibility = System.Windows.Visibility.Collapsed;
            btnCancel.Content = "Cancel";
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnCancel);

            return panelButtons;


        }
        //shows the facilities
        private void showFacilities()
        {
            
            int i = 0;
            foreach (AirlinerFacility.FacilityType type in Enum.GetValues(typeof(AirlinerFacility.FacilityType)))
            {
                AirlinerFacility facility = this.AirlinerClass.getFacility(type);

                lbFacilities[i].Items.Clear();
                lbFacilities[i].Items.Add(new AirlinerFacilityItem(this.AirlinerClass, facility));

                i++;
            }

        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = this.AirlinerClass;
            this.Close();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
           
                AirlinerFacilityItem item = (AirlinerFacilityItem)((Button)sender).Tag;

                AirlinerFacility facility = (AirlinerFacility)PopUpAirlinerFacility.ShowPopUp(item.AirlinerClass, item.Facility.Type);

                if (facility != null && item.AirlinerClass.getFacility(item.Facility.Type) != facility)
                {

                    if (facility.Type == AirlinerFacility.FacilityType.Seat)
                        item.AirlinerClass.SeatingCapacity = Convert.ToInt16(Convert.ToDouble(item.AirlinerClass.RegularSeatingCapacity) / facility.SeatUses);

                    item.AirlinerClass.setFacility(null, facility);

                    showFacilities();

                }
        }

        //the class for an item in the list
        private class AirlinerFacilityItem
        {
             public AirlinerFacility Facility { get; set; }
            public AirlinerClass AirlinerClass { get; set; }
            public string Image { get { return string.Format("/data/images/{0}.png", this.Facility.Type); } set { ;} }
            public AirlinerFacilityItem( AirlinerClass aClass, AirlinerFacility facility)
            {
                 this.AirlinerClass = aClass;
                this.Facility = facility;
            }
        }
    }

}
