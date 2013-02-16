using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.ScenarioModel;

namespace TheAirline.GraphicsModel.PageModel.PageGameModel
{
    /// <summary>
    /// Interaction logic for PagePlayScenario.xaml
    /// </summary>
    public partial class PagePlayScenario : StandardPage
    {
        private TextBox txtDescription;
        private TextBlock txtName;
        private Button btnPlay;
        private Popup popUpSplash;
        public ScrollBar ScrollBar { get; set; }
        public PagePlayScenario()
        {
            InitializeComponent();
            
            popUpSplash = new Popup();

            popUpSplash.Child = UICreator.CreateSplashWindow();
            popUpSplash.Placement = PlacementMode.Center;
            popUpSplash.PlacementTarget = PageNavigator.MainWindow;
            popUpSplash.IsOpen = false;

            StackPanel panelContent = new StackPanel();
            panelContent.Margin = new Thickness(10, 0, 10, 0);
            panelContent.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            WrapPanel panelScenarios = new WrapPanel();
            panelContent.Children.Add(panelScenarios);

            StackPanel panelSelectScenarios = new StackPanel();
            panelScenarios.Children.Add(panelSelectScenarios);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Width = 200;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Uid = "1001";
            txtHeader.TextAlignment = TextAlignment.Center;
            txtHeader.Text = Translator.GetInstance().GetString("PagePlayScenario", txtHeader.Uid);
            panelSelectScenarios.Children.Add(txtHeader);

            ListBox lbScenarios= new ListBox();
            lbScenarios.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbScenarios.Height = GraphicsHelpers.GetContentHeight() / 2;
            lbScenarios.ItemTemplate = this.Resources["ScenarioItem"] as DataTemplate;
            lbScenarios.Width = 200;

        
            foreach (Scenario scenario in Scenarios.GetScenarios())
               lbScenarios.Items.Add(scenario);

            panelSelectScenarios.Children.Add(lbScenarios);
            
            panelScenarios.Children.Add(createScenarioPanel());

            Button btnExit = new Button();
            btnExit.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnExit.Height = double.NaN;
            btnExit.Width = double.NaN;
            btnExit.Uid = "200";
            btnExit.Content = Translator.GetInstance().GetString("PagePlayScenario", btnExit.Uid);
            btnExit.Margin = new Thickness(0, 10, 0, 0);
            btnExit.Click += btnExit_Click;
            btnExit.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnExit.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelContent.Children.Add(btnExit);



            base.setTopMenu(new PageTopMenu());

            base.hideNavigator();

            base.hideBottomMenu();

            base.setContent(panelContent);

            base.setHeaderContent(Translator.GetInstance().GetString("PagePlayScenario", "1000"));



            showPage(this);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageFrontMenu());

        }
        //creates the panel for the scenario information
        private StackPanel createScenarioPanel()
        {
            StackPanel panelScenario = new StackPanel();
            panelScenario.Margin = new Thickness(10, 0, 0, 0);
            panelScenario.Width = 400;

            txtName = UICreator.CreateTextBlock("");
            txtName.FontWeight = FontWeights.Bold;
            txtName.FontSize = 16;
            txtName.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            txtName.TextDecorations = TextDecorations.Underline;
            panelScenario.Children.Add(txtName);

            txtDescription = new TextBox();
            txtDescription.Background = Brushes.Transparent;
            txtDescription.TextWrapping = TextWrapping.Wrap;
            //txtDescription.FontStyle = FontStyles.Italic;
            txtDescription.FontSize = 12;
            txtDescription.BorderThickness = new Thickness(0);
            txtDescription.MaxHeight = 250;
            txtDescription.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            txtDescription.IsReadOnly = true;
            txtDescription.Text = "";
            panelScenario.Children.Add(txtDescription);
           

            btnPlay = new Button();
            btnPlay.Uid = "117";
            btnPlay.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnPlay.Height = Double.NaN;
            btnPlay.Width = Double.NaN;
            btnPlay.Content = Translator.GetInstance().GetString("General", btnPlay.Uid);
            btnPlay.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnPlay.Margin = new System.Windows.Thickness(0, 5, 0, 0);
            btnPlay.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            btnPlay.Visibility = System.Windows.Visibility.Collapsed;
            btnPlay.Click += btnPlay_Click;
            panelScenario.Children.Add(btnPlay);

            return panelScenario;

        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            popUpSplash.IsOpen = true;

            DoEvents();
 
            Scenario scenario = (Scenario)((Button)sender).Tag;
            ScenarioHelpers.SetupScenario(scenario);
            
            popUpSplash.IsOpen = false;
            
        }
        private void lnkScenario_Click(object sender, RoutedEventArgs e)
        {
            Scenario scenario = (Scenario)((Hyperlink)sender).Tag;

            txtDescription.Text = scenario.Description;
            txtName.Text = scenario.Name;
            btnPlay.Visibility = System.Windows.Visibility.Visible;
            btnPlay.Tag = scenario;
        }
        public void DoEvents()
        {
            DispatcherFrame f = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
            (SendOrPostCallback)delegate(object arg)
            {
                DispatcherFrame fr = arg as DispatcherFrame;
                fr.Continue = false;
            }, f);
            Dispatcher.PushFrame(f);
        }
    }
}
