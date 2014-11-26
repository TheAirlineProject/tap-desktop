namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageSelectLanguage.xaml
    /// </summary>
    public partial class PageSelectLanguage : Page
    {
        #region Constructors and Destructors

        public PageSelectLanguage()
        {
            this.AllLanguages = new ObservableCollection<Language>(Languages.GetLanguages().FindAll(l => l.IsEnabled));

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<Language> AllLanguages { get; set; }

        #endregion

        #region Methods

        private void Language_Click(object sender, RoutedEventArgs e)
        {
            var language = (Language)((Hyperlink)sender).Tag;
            AppSettings.GetInstance().setLanguage(language);

            PageNavigator.NavigateTo(new PageStartMenu());
        }

        #endregion
    }
}