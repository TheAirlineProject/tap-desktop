namespace TheAirline.GUIModel.PagesModel.AlliancesPageModel
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
    using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;
    using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel.PopUpMapModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;

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
            this.Alliance = new AllianceMVVM(alliance);

            this.DataContext = this.Alliance;

            this.InitializeComponent();
        }

        #endregion

        #region Methods

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            var member = (PendingAllianceMember)((Button)sender).Tag;

            this.Alliance.addMember(new AllianceMember(member.Airline, GameObject.GetInstance().GameTime));
            this.Alliance.removePendingMember(member);
        }

        private void btnDecline_Click(object sender, RoutedEventArgs e)
        {
            var member = (PendingAllianceMember)((Button)sender).Tag;

            this.Alliance.removePendingMember(member);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2602"),
                string.Format(
                    Translator.GetInstance().GetString("MessageBox", "2602", "message"),
                    this.Alliance.Alliance.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.Alliance.removeMember(GameObject.GetInstance().HumanAirline);
            }

            if (this.Alliance.Members.Count == 0 && this.Alliance.PendingMembers.Count == 0)
            {
                Alliances.RemoveAlliance(this.Alliance.Alliance);
            }

            var frmContent = UIHelpers.FindChild<Frame>(this.Tag as Page, "frmContent");

            if (frmContent != null)
            {
                frmContent.Navigate(new PageShowAlliances { Tag = this.Tag });
            }
        }

        private void btnInvite_Click(object sender, RoutedEventArgs e)
        {
            object o = PopUpInviteAlliance.ShowPopUp(this.Alliance.Alliance);

            if (o != null)
            {
                var airlines = (List<Airline>)o;

                foreach (Airline airline in airlines)
                {
                    if (AIHelpers.DoAcceptAllianceInvitation(airline, this.Alliance.Alliance))
                    {
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2605"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2605", "message"),
                                airline.Profile.Name,
                                this.Alliance.Alliance.Name),
                            WPFMessageBoxButtons.Ok);
                        this.Alliance.addMember(new AllianceMember(airline, GameObject.GetInstance().GameTime));
                    }
                    else
                    {
                        WPFMessageBox.Show(
                            Translator.GetInstance().GetString("MessageBox", "2606"),
                            string.Format(
                                Translator.GetInstance().GetString("MessageBox", "2606", "message"),
                                airline.Profile.Name,
                                this.Alliance.Alliance.Name),
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
                    this.Alliance.Alliance.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                if (AIHelpers.CanJoinAlliance(GameObject.GetInstance().HumanAirline, this.Alliance.Alliance))
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2607"),
                        string.Format(
                            Translator.GetInstance().GetString("MessageBox", "2607", "message"),
                            GameObject.GetInstance().HumanAirline.Profile.Name,
                            this.Alliance.Alliance.Name),
                        WPFMessageBoxButtons.Ok);

                    this.Alliance.addMember(
                        new AllianceMember(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime));
                }
                else
                {
                    WPFMessageBox.Show(
                        Translator.GetInstance().GetString("MessageBox", "2608"),
                        string.Format(
                            Translator.GetInstance().GetString("MessageBox", "2608", "message"),
                            GameObject.GetInstance().HumanAirline.Profile.Name,
                            this.Alliance.Alliance.Name),
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
                this.Alliance.Alliance))
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2609"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2609", "message"),
                        member.Member.Airline.Profile.Name),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.Alliance.removeMember(member);
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
            IEnumerable<Route> routes = this.Alliance.Members.SelectMany(m => m.Member.Airline.Routes);
            PopUpMapControl.ShowPopUp(null,routes.ToList());
        }

        #endregion
    }
}