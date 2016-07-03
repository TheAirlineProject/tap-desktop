using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Helpers;
using TheAirline.Models.Airlines;
using TheAirline.Models.General;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    /// <summary>
    ///     Interaction logic for PageShowAlliance.xaml
    /// </summary>
    public partial class PageShowAlliance : Page
    {
        #region Fields

        private readonly AllianceMVVM Alliance;

        #endregion

        #region Constructors and Destructors

        public PageShowAlliance(Alliance alliance)
        {
            Alliance = new AllianceMVVM(alliance);

            DataContext = Alliance;

            InitializeComponent();
        }

        #endregion

        #region Methods

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            var member = (PendingAllianceMember)((Button)sender).Tag;

            Alliance.addMember(new AllianceMember(member.Airline, GameObject.GetInstance().GameTime));
            Alliance.removePendingMember(member);
        }

        private void btnDecline_Click(object sender, RoutedEventArgs e)
        {
            var member = (PendingAllianceMember)((Button)sender).Tag;

            Alliance.removePendingMember(member);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2602"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2602", "message"),
                    Alliance.Alliance.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                Alliance.removeMember(GameObject.GetInstance().HumanAirline);
            }

            if (Alliance.Members.Count == 0 && Alliance.PendingMembers.Count == 0)
            {
                Alliances.RemoveAlliance(Alliance.Alliance);
            }

            var frmContent = UIHelpers.FindChild<Frame>(Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowAlliances { Tag = Tag });
            }
        }

        private void btnInvite_Click(object sender, RoutedEventArgs e)
        {
            object o = PopUpInviteAlliance.ShowPopUp(Alliance.Alliance);

            if (o != null)
            {
                var airlines = (List<Airline>)o;

                foreach (Airline airline in airlines)
                {
                    if (AIHelpers.DoAcceptAllianceInvitation(airline, Alliance.Alliance))
                    {
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2605"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2605", "message"),
                                airline.Profile.Name,
                                Alliance.Alliance.Name),
                            WPFMessageBoxButtons.Ok);
                        Alliance.addMember(new AllianceMember(airline, GameObject.GetInstance().GameTime));
                    }
                    else
                    {
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2606"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2606", "message"),
                                airline.Profile.Name,
                                Alliance.Alliance.Name),
                            WPFMessageBoxButtons.Ok);
                    }
                }
            }
        }

        private void btnJoin_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2601"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2601", "message"),
                    Alliance.Alliance.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                if (AIHelpers.CanJoinAlliance(GameObject.GetInstance().HumanAirline, Alliance.Alliance))
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2607"),
                        string.Format(
                            Translator.GetInstance().GetString("MessageBox", "2607", "message"),
                            GameObject.GetInstance().HumanAirline.Profile.Name,
                            Alliance.Alliance.Name),
                        WPFMessageBoxButtons.Ok);

                    Alliance.addMember(
                        new AllianceMember(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime));
                }
                else
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2608"),
                        string.Format(
                            Translator.GetInstance().GetString("MessageBox", "2608", "message"),
                            GameObject.GetInstance().HumanAirline.Profile.Name,
                            Alliance.Alliance.Name),
                        WPFMessageBoxButtons.Ok);
                }
            }
        }

        private void btnRemoveFromAlliance_Click(object sender, RoutedEventArgs e)
        {
            var member = (AllianceMemberMVVM)((Button)sender).Tag;

            if (AIHelpers.CanRemoveFromAlliance(
                GameObject.GetInstance().HumanAirline,
                member.Member.Airline,
                Alliance.Alliance))
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2609"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2609", "message"),
                        member.Member.Airline.Profile.Name),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    Alliance.removeMember(member);
                }
            }
            else
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2610"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2610", "message"),
                        member.Member.Airline.Profile.Name),
                    WPFMessageBoxButtons.Ok);
            }
        }

        private void btnRouteMap_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Route> routes = Alliance.Members.SelectMany(m => m.Member.Airline.Routes);
            PopUpMap.ShowPopUp(routes.ToList());
        }

        #endregion
    }
}