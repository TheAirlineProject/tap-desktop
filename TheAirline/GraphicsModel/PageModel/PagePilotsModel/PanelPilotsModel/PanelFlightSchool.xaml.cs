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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.PilotModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel.CountryModel.TownModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;

namespace TheAirline.GraphicsModel.PageModel.PagePilotsModel.PanelPilotsModel
{
    /// <summary>
    /// Interaction logic for PanelFlightSchool.xaml
    /// </summary>
    public partial class PanelFlightSchool : Page
    {
        private PagePilots ParentPage;
        private FlightSchool FlightSchool;
        private ListBox lbInstructors, lbStudents;
        private TextBlock txtStudents;
        public PanelFlightSchool(PagePilots parent, FlightSchool flighschool)
        {
            this.ParentPage = parent;
            this.FlightSchool = flighschool;

            InitializeComponent();

            StackPanel panelFlightSchool = new StackPanel();

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PanelFlightSchool", txtHeader.Uid);

            panelFlightSchool.Children.Add(txtHeader);

            ListBox lbFSInformation = new ListBox();
            lbFSInformation.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            lbFSInformation.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

            lbFSInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelFlightSchool", "1002"), UICreator.CreateTextBlock(this.FlightSchool.Name)));
            lbFSInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelFlightSchool", "1003"), UICreator.CreateTextBlock(this.FlightSchool.NumberOfInstructors.ToString())));

            txtStudents = UICreator.CreateTextBlock(this.FlightSchool.NumberOfStudents.ToString());
            lbFSInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelFlightSchool", "1006"), txtStudents));

            panelFlightSchool.Children.Add(lbFSInformation);
            panelFlightSchool.Children.Add(createInstructorsPanel());
            panelFlightSchool.Children.Add(createStudentsPanel());

            panelFlightSchool.Children.Add(createButtonsPanel());

            this.Content = panelFlightSchool;

            showInstructors();
            showStudents();
        }
        //creates the students panel
        private StackPanel createStudentsPanel()
        {
            StackPanel panelStudents = new StackPanel();
            panelStudents.Margin = new Thickness(0, 5, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1004";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PanelFlightSchool", txtHeader.Uid);

            panelStudents.Children.Add(txtHeader);

            lbStudents = new ListBox();
            lbStudents.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbStudents.ItemTemplate = this.Resources["StudentItem"] as DataTemplate;
            lbStudents.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 2;

            panelStudents.Children.Add(lbStudents);

            return panelStudents;
        }
        //creates the instructors panel
        private StackPanel createInstructorsPanel()
        {
            StackPanel panelInstructors = new StackPanel();
            panelInstructors.Margin = new Thickness(0, 5, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1007";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PanelFlightSchool", txtHeader.Uid);

            panelInstructors.Children.Add(txtHeader);

            lbInstructors = new ListBox();
            lbInstructors.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbInstructors.ItemTemplate = this.Resources["InstructorItem"] as DataTemplate;
            lbInstructors.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 2;

            panelInstructors.Children.Add(lbInstructors);

            return panelInstructors;


        }
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            Button btnHire = new Button();
            btnHire.Uid = "200";
            btnHire.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnHire.Height = Double.NaN;
            btnHire.Width = Double.NaN;
            btnHire.Content = Translator.GetInstance().GetString("PanelFlightSchool", btnHire.Uid);
            btnHire.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnHire.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnHire.Click += new RoutedEventHandler(btnHire_Click);
            btnHire.IsEnabled = this.FlightSchool.Instructors.Count > 0;

            buttonsPanel.Children.Add(btnHire);

            return buttonsPanel;
        }
        //shows the students
        private void showStudents()
        {
            lbStudents.Items.Clear();

            foreach (PilotStudent student in this.FlightSchool.Students)
                lbStudents.Items.Add(student);
        }
        //shows the instructors
        private void showInstructors()
        {
            lbInstructors.Items.Clear();

            foreach (Instructor instructor in this.FlightSchool.Instructors)
                lbInstructors.Items.Add(instructor);
        }
        private void btnHire_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();

            ComboBox cbInstructor = new ComboBox();
            cbInstructor.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbInstructor.Width = 200;
            cbInstructor.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbInstructor.DisplayMemberPath = "Profile.Name";
            cbInstructor.SelectedValuePath = "Profile.Name";

            foreach (Instructor instructor in this.FlightSchool.Instructors.Where(i => i.Students.Count < FlightSchool.MaxNumberOfStudentsPerInstructor))
                cbInstructor.Items.Add(instructor);

            cbInstructor.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PanelFlightSchool", "1005"), cbInstructor) == PopUpSingleElement.ButtonSelected.OK && cbInstructor.SelectedItem != null)
            {

                List<Town> towns = Towns.GetTowns();

                Town town = towns[rnd.Next(towns.Count)];
                DateTime birthdate = MathHelpers.GetRandomDate(GameObject.GetInstance().GameTime.AddYears(-55), GameObject.GetInstance().GameTime.AddYears(-23));
                PilotProfile profile = new PilotProfile(Names.GetInstance().getRandomFirstName(), Names.GetInstance().getRandomLastName(), birthdate, town);

                PilotStudent student = new PilotStudent(profile, GameObject.GetInstance().GameTime, (Instructor)cbInstructor.SelectedItem);

                this.FlightSchool.addStudent(student);
                ((Instructor)cbInstructor.SelectedItem).addStudent(student);

                showStudents();

                this.ParentPage.updatePage();

                txtStudents.Text = this.FlightSchool.NumberOfStudents.ToString();
            }

        }
    }
}
