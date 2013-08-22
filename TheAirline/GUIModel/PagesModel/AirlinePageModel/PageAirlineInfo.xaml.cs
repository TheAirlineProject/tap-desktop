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
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirlineInfo.xaml
    /// </summary>
    public partial class PageAirlineInfo : Page
    {
        public AirlineMVVM Airline { get; set; }
        public PageAirlineInfo(AirlineMVVM airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;

            InitializeComponent();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvFleet.ItemsSource);
            view.GroupDescriptions.Clear();

            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Purchased");
            view.GroupDescriptions.Add(groupDescription);
   
        }

        private void btnCreateSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            SubsidiaryAirline airline = (SubsidiaryAirline)PopUpNewSubsidiary.ShowPopUp();

            if (airline != null)
            {
               this.Airline.addSubsidiaryAirline(airline);


            }
        }

        private void btnReleaseSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            SubsidiaryAirline airline = (SubsidiaryAirline)((Button)sender).Tag;

            if (airline == GameObject.GetInstance().HumanAirline)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2112"), string.Format(Translator.GetInstance().GetString("MessageBox", "2112", "message"), airline.Profile.Name), WPFMessageBoxButtons.Ok);
            }
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2118"), string.Format(Translator.GetInstance().GetString("MessageBox", "2118", "message"), airline.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {

                    AirlineHelpers.MakeSubsidiaryAirlineIndependent(airline);

                    this.Airline.removeSubsidiaryAirline(airline);

                }
            }
        }

        private void btnDeleteSubsidiary_Click(object sender, RoutedEventArgs e)
        {
            SubsidiaryAirline airline = (SubsidiaryAirline)((Button)sender).Tag;

            if (airline == GameObject.GetInstance().HumanAirline)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2112"), string.Format(Translator.GetInstance().GetString("MessageBox", "2112", "message"), airline.Profile.Name), WPFMessageBoxButtons.Ok);
            }
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2111"), string.Format(Translator.GetInstance().GetString("MessageBox", "2111", "message"), airline.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {

                    AirlineHelpers.CloseSubsidiaryAirline(airline);

                    this.Airline.removeSubsidiaryAirline(airline);

                }
            }
        }
    }
}
