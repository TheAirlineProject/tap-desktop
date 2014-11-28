namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;
    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.Model.GeneralModel;

    /// <summary>
    ///     Interaction logic for PageCreateDifficulty.xaml
    /// </summary>
    public partial class PageCreateDifficulty : Page
    {
        #region Constructors and Destructors

        public PageCreateDifficulty()
        {
            this.Difficulties = new ObservableCollection<DifficultyMVVM>();

            DifficultyLevel easyLevel = DifficultyLevels.GetDifficultyLevel("Easy");
            DifficultyLevel normalLevel = DifficultyLevels.GetDifficultyLevel("Normal");
            DifficultyLevel hardLevel = DifficultyLevels.GetDifficultyLevel("Hard");

            this.Difficulties.Add(
                new DifficultyMVVM(
                    "money",
                    Translator.GetInstance().GetString("PageCreateDifficulty", "1000"),
                    easyLevel.MoneyLevel,
                    normalLevel.MoneyLevel,
                    hardLevel.MoneyLevel));
            this.Difficulties.Add(
                new DifficultyMVVM(
                    "price",
                    Translator.GetInstance().GetString("PageCreateDifficulty", "1001"),
                    easyLevel.PriceLevel,
                    normalLevel.PriceLevel,
                    hardLevel.PriceLevel));
            this.Difficulties.Add(
                new DifficultyMVVM(
                    "loan",
                    Translator.GetInstance().GetString("PageCreateDifficulty", "1002"),
                    easyLevel.LoanLevel,
                    normalLevel.LoanLevel,
                    hardLevel.LoanLevel));
            this.Difficulties.Add(
                new DifficultyMVVM(
                    "passengers",
                    Translator.GetInstance().GetString("PageCreateDifficulty", "1003"),
                    easyLevel.PassengersLevel,
                    normalLevel.PassengersLevel,
                    hardLevel.PassengersLevel));
            this.Difficulties.Add(
                new DifficultyMVVM(
                    "AI",
                    Translator.GetInstance().GetString("PageCreateDifficulty", "1004"),
                    easyLevel.AILevel,
                    normalLevel.AILevel,
                    hardLevel.AILevel));
            this.Difficulties.Add(
                new DifficultyMVVM(
                    "startdata",
                    Translator.GetInstance().GetString("PageCreateDifficulty", "1005"),
                    easyLevel.StartDataLevel,
                    normalLevel.StartDataLevel,
                    hardLevel.StartDataLevel));

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public ObservableCollection<DifficultyMVVM> Difficulties { get; set; }

        #endregion

        #region Methods

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            var slMoney = UIHelpers.FindChild<Slider>(this, "money");
            var slLoan = UIHelpers.FindChild<Slider>(this, "loan");
            var slPrice = UIHelpers.FindChild<Slider>(this, "price");
            var slPassengers = UIHelpers.FindChild<Slider>(this, "passengers");
            var slAI = UIHelpers.FindChild<Slider>(this, "AI");
            var slStartData = UIHelpers.FindChild<Slider>(this, "startdata");

            double money = slMoney.Value;
            double loan = slLoan.Value;
            double passengers = slPassengers.Value;
            double price = slPrice.Value;
            double AI = slAI.Value;
            double startData = slStartData.Value;

            var level = new DifficultyLevel("Custom", money, loan, passengers, price, AI, startData);

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2406"),
                Translator.GetInstance().GetString("MessageBox", "2406", "message"),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                DifficultyLevels.AddDifficultyLevel(level);

                PageNavigator.NavigateTo(new PageNewGame());
            }
        }

        #endregion
    }
}