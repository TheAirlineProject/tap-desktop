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
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GraphicsModel.PageModel.PagePilotsModel.PanelPilotsModel
{
    /// <summary>
    /// Interaction logic for PanelFlightSchool.xaml
    /// </summary>
    public partial class PanelFlightSchool : Page
    {
        private PagePilots ParentPage;
        private FlightSchool FlightSchool;
        private ListBox lbInstructors, lbStudents, lbTrainingAircrafts;
        private TextBlock txtStudents, txtTrainingAircrafts;
        private Button btnHire;
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

            txtTrainingAircrafts = UICreator.CreateTextBlock(this.FlightSchool.TrainingAircrafts.Count.ToString());
            lbFSInformation.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelFlightSchool", "1009"), txtTrainingAircrafts));

            panelFlightSchool.Children.Add(lbFSInformation);
            panelFlightSchool.Children.Add(createInstructorsPanel());
            panelFlightSchool.Children.Add(createTrainingAircraftsPanel());
            panelFlightSchool.Children.Add(createStudentsPanel());

            panelFlightSchool.Children.Add(createButtonsPanel());

            this.Content = panelFlightSchool;

            showInstructors();
            showStudents();
            showTrainingAircrafts();
        }
        //creates the training aircrafts panel 
        private StackPanel createTrainingAircraftsPanel()
        {
            StackPanel panelAircrafts = new StackPanel();
            panelAircrafts.Margin = new Thickness(0, 5, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1009";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PanelFlightSchool", txtHeader.Uid);

            panelAircrafts.Children.Add(txtHeader);

            lbTrainingAircrafts = new ListBox();
            lbTrainingAircrafts.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbTrainingAircrafts.ItemTemplate = this.Resources["TrainingAircraftItem"] as DataTemplate;
            lbTrainingAircrafts.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 6;

            panelAircrafts.Children.Add(lbTrainingAircrafts);

            return panelAircrafts;
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
            lbStudents.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 3;

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
            lbInstructors.MaxHeight = (GraphicsHelpers.GetContentHeight() - 100) / 3;

            panelInstructors.Children.Add(lbInstructors);

            return panelInstructors;


        }
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
            int studentsCapacity = Math.Min(this.FlightSchool.Instructors.Count * FlightSchool.MaxNumberOfStudentsPerInstructor, this.FlightSchool.TrainingAircrafts.Sum(f=>f.Type.MaxNumberOfStudents));

            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            btnHire = new Button();
            btnHire.Uid = "200";
            btnHire.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnHire.Height = Double.NaN;
            btnHire.Width = Double.NaN;
            btnHire.Content = Translator.GetInstance().GetString("PanelFlightSchool", btnHire.Uid);
            btnHire.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnHire.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnHire.Click += new RoutedEventHandler(btnHire_Click);
            btnHire.IsEnabled = studentsCapacity > this.FlightSchool.Students.Count && GameObject.GetInstance().HumanAirline.Money > GeneralHelpers.GetInflationPrice(PilotStudent.StudentCost);

            buttonsPanel.Children.Add(btnHire);

            Button btnAircraft = new Button();
            btnAircraft.Uid = "201";
            btnAircraft.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnAircraft.Height = Double.NaN;
            btnAircraft.Width = Double.NaN;
            btnAircraft.Content = Translator.GetInstance().GetString("PanelFlightSchool", btnAircraft.Uid);
            btnAircraft.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnAircraft.Margin = new Thickness(5, 0, 0, 0);
            btnAircraft.Click += btnAircraft_Click;

            buttonsPanel.Children.Add(btnAircraft);


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
        //shows the training aircrafts
        private void showTrainingAircrafts()
        {
            lbTrainingAircrafts.Items.Clear();

            foreach (TrainingAircraft aircraft in this.FlightSchool.TrainingAircrafts)
                lbTrainingAircrafts.Items.Add(aircraft);
        }
        private void btnAircraft_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cbAircraft = new ComboBox();
            cbAircraft.SetResourceReference(ComboBox.StyleProperty,"ComboBoxTransparentStyle");
            cbAircraft.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAircraft.ItemTemplate = this.Resources["TrainingAircraftTypeItem"] as DataTemplate;
            cbAircraft.Width = 300;

            foreach (TrainingAircraftType type in TrainingAircraftTypes.GetAircraftTypes().FindAll(t => GeneralHelpers.GetInflationPrice(t.Price) < GameObject.GetInstance().HumanAirline.Money))
                cbAircraft.Items.Add(type);

            cbAircraft.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PanelFlightSchool", "1005"), cbAircraft) == PopUpSingleElement.ButtonSelected.OK && cbAircraft.SelectedItem != null)
            {
              
                TrainingAircraftType aircraft = (TrainingAircraftType)cbAircraft.SelectedItem;
                double price = aircraft.Price;

                this.FlightSchool.addTrainingAircraft(new TrainingAircraft(aircraft, GameObject.GetInstance().GameTime, this.FlightSchool));

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -price);

                txtTrainingAircrafts.Text = this.FlightSchool.TrainingAircrafts.Count.ToString();

                showTrainingAircrafts();

                int studentsCapacity = Math.Min(this.FlightSchool.Instructors.Count * FlightSchool.MaxNumberOfStudentsPerInstructor, this.FlightSchool.TrainingAircrafts.Sum(f => f.Type.MaxNumberOfStudents));

                btnHire.IsEnabled = studentsCapacity > this.FlightSchool.Students.Count && GameObject.GetInstance().HumanAirline.Money > GeneralHelpers.GetInflationPrice(PilotStudent.StudentCost);


            }
        }
        private void btnDeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            PilotStudent student = (PilotStudent)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2807"), string.Format(Translator.GetInstance().GetString("MessageBox", "2807", "message"), student.Profile.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.FlightSchool.removeStudent(student);
                student.Instructor.removeStudent(student);
                student.Instructor = null;

                showStudents();

                this.ParentPage.updatePage();

            }
        }
        private void btnDeleteInstructor_Click(object sender, RoutedEventArgs e)
        {
            Instructor instructor = (Instructor)((Button)sender).Tag;

            if (instructor.Students.Count > 0)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2805"), string.Format(Translator.GetInstance().GetString("MessageBox", "2805", "message"), instructor.Profile.Name), WPFMessageBoxButtons.Ok);
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2806"), string.Format(Translator.GetInstance().GetString("MessageBox", "2806", "message"), instructor.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.FlightSchool.removeInstructor(instructor);

                    instructor.FlightSchool = null;
                    showInstructors();

                    this.ParentPage.updatePage();

                }
            }
        }
        private void lnkStudent_Click(object sender, RoutedEventArgs e)
        {
            PilotStudent student = (PilotStudent)((Hyperlink)sender).Tag;
             
            ComboBox cbInstructor = new ComboBox();
            cbInstructor.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbInstructor.Width = 200;
            cbInstructor.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbInstructor.DisplayMemberPath = "Profile.Name";
            cbInstructor.SelectedValuePath = "Profile.Name";

            foreach (Instructor instructor in this.FlightSchool.Instructors.Where(i => i.Students.Count < FlightSchool.MaxNumberOfStudentsPerInstructor && i!=student.Instructor))
                cbInstructor.Items.Add(instructor);

            cbInstructor.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PanelFlightSchool", "1005"), cbInstructor) == PopUpSingleElement.ButtonSelected.OK && cbInstructor.SelectedItem != null)
            {
                student.Instructor.removeStudent(student);
                student.Instructor = (Instructor)cbInstructor.SelectedItem;

                showStudents();
            }
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
              
                List<Town> towns = Towns.GetTowns(this.FlightSchool.Airport.Profile.Country);

                Town town = towns[rnd.Next(towns.Count)];
                DateTime birthdate = MathHelpers.GetRandomDate(GameObject.GetInstance().GameTime.AddYears(-55), GameObject.GetInstance().GameTime.AddYears(-23));
                PilotProfile profile = new PilotProfile(Names.GetInstance().getRandomFirstName(), Names.GetInstance().getRandomLastName(), birthdate, town);

                PilotStudent student = new PilotStudent(profile, GameObject.GetInstance().GameTime, (Instructor)cbInstructor.SelectedItem);

                this.FlightSchool.addStudent(student);
                ((Instructor)cbInstructor.SelectedItem).addStudent(student);

                showStudents();

                this.ParentPage.updatePage();

                txtStudents.Text = this.FlightSchool.NumberOfStudents.ToString();

                double studentPrice = GeneralHelpers.GetInflationPrice(PilotStudent.StudentCost);

                int studentsCapacity = Math.Min(this.FlightSchool.Instructors.Count * FlightSchool.MaxNumberOfStudentsPerInstructor, this.FlightSchool.TrainingAircrafts.Sum(f => f.Type.MaxNumberOfStudents));

                btnHire.IsEnabled = studentsCapacity > this.FlightSchool.Students.Count && GameObject.GetInstance().HumanAirline.Money > studentPrice;

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -studentPrice);
            }

        }
    }
}
