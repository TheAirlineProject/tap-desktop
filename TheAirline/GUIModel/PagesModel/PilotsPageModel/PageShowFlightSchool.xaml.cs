using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.CountryModel.TownModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.PilotModel;

namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    /// <summary>
    /// Interaction logic for PageShowFlightSchool.xaml
    /// </summary>
    public partial class PageShowFlightSchool : Page
    {
        public FlightSchoolMVVM FlightSchool { get; set; }
       
        public PageShowFlightSchool(FlightSchool fs)
        {
            this.FlightSchool = new FlightSchoolMVVM(fs);
            this.DataContext = this.FlightSchool;

            setHireStudentsStatus();

            InitializeComponent();
        }
        private void btnSellAircraft_Click(object sender, RoutedEventArgs e)
        {
            TrainingAircraft aircraft = (TrainingAircraft)((Button)sender).Tag;
            
            var aircrafts = new List<TrainingAircraft>(this.FlightSchool.Aircrafts);
            aircrafts.Remove(aircraft);

            Boolean canSellAircraft = aircrafts.Sum(a => a.Type.MaxNumberOfStudents) >= this.FlightSchool.Students.Count;

            if (canSellAircraft)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2809"), Translator.GetInstance().GetString("MessageBox", "2809", "message"), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                    this.FlightSchool.removeTrainingAircraft(aircraft);
            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2810"), Translator.GetInstance().GetString("MessageBox", "2810", "message"), WPFMessageBoxButtons.Ok);
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

            foreach (Instructor instructor in this.FlightSchool.Instructors.Where(i => i.Students.Count < Model.PilotModel.FlightSchool.MaxNumberOfStudentsPerInstructor))
                cbInstructor.Items.Add(instructor);

            cbInstructor.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PanelFlightSchool", "1005"), cbInstructor) == PopUpSingleElement.ButtonSelected.OK && cbInstructor.SelectedItem != null)
            {
                List<Town> towns = Towns.GetTowns(this.FlightSchool.FlightSchool.Airport.Profile.Country);

                Town town = towns[rnd.Next(towns.Count)];
                DateTime birthdate = MathHelpers.GetRandomDate(GameObject.GetInstance().GameTime.AddYears(-55), GameObject.GetInstance().GameTime.AddYears(-23));
                PilotProfile profile = new PilotProfile(Names.GetInstance().getRandomFirstName(), Names.GetInstance().getRandomLastName(), birthdate, town);

                PilotStudent student = new PilotStudent(profile, GameObject.GetInstance().GameTime, (Instructor)cbInstructor.SelectedItem);

                this.FlightSchool.addStudent(student);
                ((Instructor)cbInstructor.SelectedItem).addStudent(student);

                setHireStudentsStatus();

                double studentPrice = GeneralHelpers.GetInflationPrice(PilotStudent.StudentCost);

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -studentPrice);
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
                
                }
            }
        }
        private void btnBuyAircraft_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cbAircraft = new ComboBox();
            cbAircraft.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
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

                this.FlightSchool.addTrainingAircraft(new TrainingAircraft(aircraft, GameObject.GetInstance().GameTime, this.FlightSchool.FlightSchool));

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -price);

                setHireStudentsStatus();

            }
        }
        private void btnChangeInstructor_Click(object sender, RoutedEventArgs e)
        {
            PilotStudent student = (PilotStudent)((Hyperlink)sender).Tag;

            ComboBox cbInstructor = new ComboBox();
            cbInstructor.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbInstructor.Width = 200;
            cbInstructor.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbInstructor.DisplayMemberPath = "Profile.Name";
            cbInstructor.SelectedValuePath = "Profile.Name";

            foreach (Instructor instructor in this.FlightSchool.Instructors.Where(i => i.Students.Count <  Model.PilotModel.FlightSchool.MaxNumberOfStudentsPerInstructor && i != student.Instructor))
                cbInstructor.Items.Add(instructor);

            cbInstructor.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PanelFlightSchool", "1005"), cbInstructor) == PopUpSingleElement.ButtonSelected.OK && cbInstructor.SelectedItem != null)
            {
                student.Instructor.removeStudent(student);
                student.Instructor = (Instructor)cbInstructor.SelectedItem;

                ICollectionView view = CollectionViewSource.GetDefaultView(lvStudents.ItemsSource);
                view.Refresh();
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

                setHireStudentsStatus();
                
            }
        }
        //sets the status for hiring of students
        private void setHireStudentsStatus()
        {
            double studentPrice = GeneralHelpers.GetInflationPrice(PilotStudent.StudentCost);

            int studentsCapacity = Math.Min(this.FlightSchool.Instructors.Count * Model.PilotModel.FlightSchool.MaxNumberOfStudentsPerInstructor, this.FlightSchool.FlightSchool.TrainingAircrafts.Sum(f => f.Type.MaxNumberOfStudents));

            this.FlightSchool.HireStudents = studentsCapacity > this.FlightSchool.Students.Count && GameObject.GetInstance().HumanAirline.Money > studentPrice;

        }
       
    }
}
