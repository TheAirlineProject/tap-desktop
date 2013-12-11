using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    /// <summary>
    /// Interaction logic for PageShowAlliance.xaml
    /// </summary>
    public partial class PageShowAlliance : Page
    {
        private Alliance Alliance;
        public PageShowAlliance(Alliance alliance)
        {
            this.Alliance = alliance;
                      
            this.DataContext = this.Alliance;

            InitializeComponent();
        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2602"), string.Format(Translator.GetInstance().GetString("MessageBox", "2602", "message"), this.Alliance.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.Alliance.removeMember(GameObject.GetInstance().HumanAirline);
            }

            if (this.Alliance.Members.Count == 0 && this.Alliance.PendingMembers.Count == 0)
            {
                Alliances.RemoveAlliance(this.Alliance);
            }

            Frame frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowAlliances() { Tag = this.Tag });

            }
        }
        private void btnInvite_Click(object sender, RoutedEventArgs e)
        {
           object o = PopUpInviteAlliance.ShowPopUp(this.Alliance);

            if (o != null)
            {
                List<Airline> airlines = (List<Airline>)o;

                foreach (Airline airline in airlines)
                {
                    if (AIHelpers.DoAcceptAllianceInvitation(airline, this.Alliance))
                    {
                        WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2605"), string.Format(Translator.GetInstance().GetString("MessageBox", "2605", "message"), airline.Profile.Name, this.Alliance.Name), WPFMessageBoxButtons.Ok);
                        this.Alliance.addMember(new AllianceMember(airline, GameObject.GetInstance().GameTime));
                    }
                    else
                    {
                        WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2606"), string.Format(Translator.GetInstance().GetString("MessageBox", "2606", "message"), airline.Profile.Name, this.Alliance.Name), WPFMessageBoxButtons.Ok);

                    }


                }

        

            }
        }
        private void btnJoin_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2601"), string.Format(Translator.GetInstance().GetString("MessageBox", "2601", "message"), this.Alliance.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                if (AIHelpers.CanJoinAlliance(GameObject.GetInstance().HumanAirline, this.Alliance))
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2606"), string.Format(Translator.GetInstance().GetString("MessageBox", "2606", "message"), GameObject.GetInstance().HumanAirline.Profile.Name, this.Alliance.Name), WPFMessageBoxButtons.Ok);

                    this.Alliance.addMember(new AllianceMember(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime));
                }
                else
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2607"), string.Format(Translator.GetInstance().GetString("MessageBox", "2607", "message"), GameObject.GetInstance().HumanAirline.Profile.Name, this.Alliance.Name), WPFMessageBoxButtons.Ok);

                }
            }
        }
        private void btnRouteMap_Click(object sender, RoutedEventArgs e)
        {
            var routes = this.Alliance.Members.SelectMany(m => m.Airline.Routes);
            PopUpMap.ShowPopUp(routes.ToList());
  
        }
        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            PendingAllianceMember member = (PendingAllianceMember)((Button)sender).Tag;

            this.Alliance.addMember(new AllianceMember(member.Airline, GameObject.GetInstance().GameTime));
            this.Alliance.removePendingMember(member);

        }

        private void btnDecline_Click(object sender, RoutedEventArgs e)
        {
            PendingAllianceMember member = (PendingAllianceMember)((Button)sender).Tag;

            this.Alliance.removePendingMember(member);

        }
    }
}
