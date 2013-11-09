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
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirlinerFacility.xaml
    /// </summary>
    public partial class PopUpAirlinerFacility : PopUpWindow
    {
        private ComboBox cbFacility;
        //private Airliner Airliner;
        private AirlinerFacility.FacilityType Type;
        private AirlinerClass AirlinerClass;
        public static object ShowPopUp(AirlinerClass airlinerClass, AirlinerFacility.FacilityType type)
        {
            

            PopUpWindow window = new PopUpAirlinerFacility(airlinerClass, type);
         
            window.ShowDialog();

            return window.Selected == null ? null : window.Selected;
        }
        public PopUpAirlinerFacility(AirlinerClass airlinerClass, AirlinerFacility.FacilityType type)
        {
            InitializeComponent();

            this.AirlinerClass = airlinerClass;
            this.Type = type;

            this.Title = "Select " + type.ToString().ToLower(); 

            this.Width = 400;

            this.Height = 120;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);
          
            cbFacility = new ComboBox();
            cbFacility.ItemTemplate = this.Resources["AirlinerFacilityItem"] as DataTemplate;
            cbFacility.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
        
            foreach (AirlinerFacility facility in AirlinerFacilities.GetFacilities(this.Type,GameObject.GetInstance().GameTime.Year))
                cbFacility.Items.Add(facility);

            cbFacility.SelectedItem = this.AirlinerClass.getFacility(this.Type);

            mainPanel.Children.Add(cbFacility);

            mainPanel.Children.Add(createButtonsPanel());

            this.Content = mainPanel;
           // int serviceLevel, double percentOfSeats, double pricePerSeat

        }
        //creates the button panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);

            Button btnOk = new Button();
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = "OK";
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            panelButtons.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = Double.NaN;
            btnCancel.Width = Double.NaN;
            btnCancel.Content = "Cancel";
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);

            panelButtons.Children.Add(btnCancel);

            return panelButtons;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = cbFacility.SelectedItem;
            this.Close();
        }
    }
}
