namespace TheAirline.GUIModel.MasterPageModel
{
    using System.Windows;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.GUIModel.PagesModel.AirlinePageModel;
    using TheAirline.GUIModel.PagesModel.AirlinersPageModel;
    using TheAirline.GUIModel.PagesModel.AirlinesPageModel;
    using TheAirline.GUIModel.PagesModel.AirportsPageModel;
    using TheAirline.GUIModel.PagesModel.AlliancesPageModel;
    using TheAirline.GUIModel.PagesModel.GamePageModel;
    using TheAirline.GUIModel.PagesModel.OptionsPageModel;
    using TheAirline.GUIModel.PagesModel.PilotsPageModel;
    using TheAirline.GUIModel.PagesModel.RoutesPageModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers.WorkersModel;
    using TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel;

    public partial class standardEvents
    {
        #region Methods

        private void Airliners_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirliners());
        }

        private void Airlines_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirlines());
        }

        private void Airports_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirports());
        }

        private void Alliances_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAlliances());
        }

        private void Flights_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageFlights());
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageAirline(GameObject.GetInstance().HumanAirline));
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageOptions());
        }

        private void Pilots_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PagePilotsFS());
        }

        private void RoutesManager_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageRoutes());
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateBack();
        }
        private void btnEvent_Click(object sender, RoutedEventArgs e)
        {
            PopUpSpecialContracts.ShowPopUp();
 
        }

        private void btnCalendar_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageCalendar());
        }

        private void btnExitGame_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "1003"),
                Translator.GetInstance().GetString("MessageBox", "1003", "message"),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                PageNavigator.MainWindow.Close();
            }
        }

        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            GameObjectWorker.GetInstance().cancel();

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "1001"),
                Translator.GetInstance().GetString("MessageBox", "1001", "message"),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                GameObjectWorker.GetInstance().cancel();

                while (GameObjectWorker.GetInstance().isBusy())
                {
                }
                PageNavigator.NavigateTo(new PageNewGame());
                GameObject.RestartInstance();

                Setup.SetupGame();
            }
        }

        private void btnNews_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageNews());
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            GameObjectWorker.GetInstance().pause();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            GameObjectWorker.GetInstance().restart();
        }

        #endregion
    }
}