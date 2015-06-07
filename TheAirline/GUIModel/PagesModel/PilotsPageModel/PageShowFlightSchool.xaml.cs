using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using TheAirline.General.Models.Countries.Towns;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Helpers;
using TheAirline.Models.Airliners;
using TheAirline.Models.General;
using TheAirline.Models.General.Finances;
using TheAirline.Models.Pilots;

namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    /// <summary>
    ///     Interaction logic for PageShowFlightSchool.xaml
    /// </summary>
    public partial class PageShowFlightSchool : Page
    {
        #region Fields

        private readonly Random rnd = new Random();

        #endregion

        #region Constructors and Destructors

        public PageShowFlightSchool(FlightSchool fs)
        {
            FlightSchool = new FlightSchoolMVVM(fs);
            Instructors = new ObservableCollection<Instructor>();
            AirlinerFamilies =
                AirlinerTypes.GetTypes(
                    t =>
                        t.Produced.From.Year <= GameObject.GetInstance().GameTime.Year
                        && t.Produced.To > GameObject.GetInstance().GameTime.AddYears(-30))
                    .Select(t => t.AirlinerFamily)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList();

            DataContext = FlightSchool;

            setHireStudentsStatus();

            InitializeComponent();
        }

        #endregion

        #region Public Properties

        public List<string> AirlinerFamilies { get; set; }

        public FlightSchoolMVVM FlightSchool { get; set; }

        public ObservableCollection<Instructor> Instructors { get; set; }

        #endregion

        #region Methods

        private void btnBuyAircraft_Click(object sender, RoutedEventArgs e)
        {
            var cbAircraft = new ComboBox();
            cbAircraft.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbAircraft.HorizontalAlignment = HorizontalAlignment.Left;
            cbAircraft.ItemTemplate = Resources["TrainingAircraftTypeItem"] as DataTemplate;
            cbAircraft.Width = 300;

            foreach (
                TrainingAircraftType type in
                    TrainingAircraftTypes.GetAircraftTypes()
                        .FindAll(
                            t => GeneralHelpers.GetInflationPrice(t.Price) < GameObject.GetInstance().HumanAirline.Money)
                )
            {
                cbAircraft.Items.Add(type);
            }

            cbAircraft.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PageShowFlightSchool", "1014"),
                cbAircraft) == PopUpSingleElement.ButtonSelected.OK && cbAircraft.SelectedItem != null)
            {
                var aircraft = (TrainingAircraftType)cbAircraft.SelectedItem;
                double price = aircraft.Price;

                FlightSchool.addTrainingAircraft(
                    new TrainingAircraft(aircraft, GameObject.GetInstance().GameTime, FlightSchool.FlightSchool));

                AirlineHelpers.AddAirlineInvoice(
                    GameObject.GetInstance().HumanAirline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.AirlineExpenses,
                    -price);

                setHireStudentsStatus();
            }
        }

        private void btnChangeInstructor_Click(object sender, RoutedEventArgs e)
        {
            var student = (PilotStudent)((Hyperlink)sender).Tag;

            var cbInstructor = new ComboBox();
            cbInstructor.SetResourceReference(StyleProperty, "ComboBoxTransparentStyle");
            cbInstructor.Width = 200;
            cbInstructor.HorizontalAlignment = HorizontalAlignment.Left;
            cbInstructor.DisplayMemberPath = "Profile.Name";
            cbInstructor.SelectedValuePath = "Profile.Name";

            foreach (
                Instructor instructor in
                    FlightSchool.Instructors.Where(
                        i =>
                            i.Students.Count < Models.Pilots.FlightSchool.MaxNumberOfStudentsPerInstructor
                            && i != student.Instructor))
            {
                cbInstructor.Items.Add(instructor);
            }

            cbInstructor.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(
                Translator.GetInstance().GetString("PanelFlightSchool", "1005"),
                cbInstructor) == PopUpSingleElement.ButtonSelected.OK && cbInstructor.SelectedItem != null)
            {
                student.Instructor.RemoveStudent(student);
                student.Instructor = (Instructor)cbInstructor.SelectedItem;

                ICollectionView view = CollectionViewSource.GetDefaultView(lvStudents.ItemsSource);
                view.Refresh();
            }
        }

        private void btnDeleteInstructor_Click(object sender, RoutedEventArgs e)
        {
            var instructor = (Instructor)((Button)sender).Tag;

            if (instructor.Students.Count > 0)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2805"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2805", "message"),
                        instructor.Profile.Name),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2806"),
                    string.Format(
                        Translator.GetInstance().GetString("MessageBox", "2806", "message"),
                        instructor.Profile.Name),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    FlightSchool.removeInstructor(instructor);

                    instructor.FlightSchool = null;
                }
            }
        }

        private void btnDeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            var student = (PilotStudent)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2807"),
                string.Format(Translator.GetInstance().GetString("MessageBox", "2807", "message"), student.Profile.Name),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                FlightSchool.removeStudent(student);
                student.Instructor.RemoveStudent(student);
                student.Instructor = null;

                setHireStudentsStatus();
            }
        }

        private void btnHire_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<TrainingAircraftType> aircraftsTypesFree = FlightSchool.Aircrafts.Select(a => a.Type);

            Dictionary<TrainingAircraftType, int> types =
                FlightSchool.Aircrafts.GroupBy(a => a.Type)
                    .Select(group => new { Type = group.Key, Count = group.Sum(g => g.Type.MaxNumberOfStudents) })
                    .ToDictionary(g => g.Type, g => g.Count);
            ;

            foreach (PilotStudent student in FlightSchool.Students)
            {
                TrainingAircraftType firstAircraft =
                    student.Rating.Aircrafts.OrderBy(a => a.TypeLevel)
                        .FirstOrDefault(a => types.ContainsKey(a) && types[a] > 0);

                if (firstAircraft != null && types.ContainsKey(firstAircraft))
                {
                    types[firstAircraft]--;
                }
            }

            var possibleRatings = new List<PilotRating>();

            foreach (PilotRating rating in PilotRatings.GetRatings())
            {
                if (rating.Aircrafts.Exists(a => types.ContainsKey(a) && types[a] > 0))
                {
                    possibleRatings.Add(rating);
                }
            }

            WPFMessageBoxResult result = WPFMessageBox.Show(
                Translator.GetInstance().GetString("MessageBox", "2811"),
                string.Format(Translator.GetInstance().GetString("MessageBox", "2811", "message")),
                WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                List<Town> towns = Towns.GetTowns(FlightSchool.FlightSchool.Airport.Profile.Country);

                Town town = towns[rnd.Next(towns.Count)];
                DateTime birthdate = MathHelpers.GetRandomDate(
                    GameObject.GetInstance().GameTime.AddYears(-35),
                    GameObject.GetInstance().GameTime.AddYears(-23));
                var profile = new PilotProfile(
                    Names.GetInstance().GetRandomFirstName(town.Country),
                    Names.GetInstance().GetRandomLastName(town.Country),
                    birthdate,
                    town);

                var instructor = (Instructor)cbInstructor.SelectedItem;
                string airlinerFamily = cbTrainAircraft.SelectedItem.ToString();

                var student = new PilotStudent(
                    profile,
                    GameObject.GetInstance().GameTime,
                    instructor,
                    GeneralHelpers.GetPilotStudentRating(instructor, possibleRatings),
                    airlinerFamily);

                TrainingAircraft aircraft = getStudentAircraft(student);

                student.Aircraft = aircraft;

                FlightSchool.addStudent(student);
                instructor.AddStudent(student);

                setHireStudentsStatus();

                double studentPrice = GeneralHelpers.GetInflationPrice(PilotStudent.StudentCost);

                AirlineHelpers.AddAirlineInvoice(
                    GameObject.GetInstance().HumanAirline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.AirlineExpenses,
                    -studentPrice);
            }
        }

        private void btnSellAircraft_Click(object sender, RoutedEventArgs e)
        {
            var aircraft = (TrainingAircraft)((Button)sender).Tag;

            var aircrafts = new List<TrainingAircraft>(FlightSchool.Aircrafts);
            aircrafts.Remove(aircraft);

            Dictionary<TrainingAircraftType, int> types =
                FlightSchool.Aircrafts.GroupBy(a => a.Type)
                    .Select(group => new { Type = group.Key, Count = group.Sum(g => g.Type.MaxNumberOfStudents) })
                    .ToDictionary(g => g.Type, g => g.Count);
            ;

            foreach (PilotStudent student in FlightSchool.Students)
            {
                TrainingAircraftType firstAircraft =
                    student.Rating.Aircrafts.OrderBy(a => a.TypeLevel).First(a => types.ContainsKey(a) && types[a] > 0);

                if (types.ContainsKey(firstAircraft))
                {
                    types[firstAircraft]--;
                }
            }

            Boolean canSellAircraft = aircrafts.Sum(a => a.Type.MaxNumberOfStudents) >= FlightSchool.Students.Count
                                      && types[aircraft.Type] > 1;

            if (canSellAircraft)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2809"),
                    Translator.GetInstance().GetString("MessageBox", "2809", "message"),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    FlightSchool.removeTrainingAircraft(aircraft);

                    double price = aircraft.Type.Price * 0.75;
                    AirlineHelpers.AddAirlineInvoice(
                        GameObject.GetInstance().HumanAirline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.AirlineExpenses,
                        price);
                }
            }
            else
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2810"),
                    Translator.GetInstance().GetString("MessageBox", "2810", "message"),
                    WPFMessageBoxButtons.Ok);
            }
        }

        //sets the status for hiring of students

        //returns the aircraft for a student
        private TrainingAircraft getStudentAircraft(PilotStudent student)
        {
            Dictionary<TrainingAircraftType, int> types =
                FlightSchool.Aircrafts.GroupBy(a => a.Type)
                    .Select(group => new { Type = group.Key, Count = group.Sum(g => g.Type.MaxNumberOfStudents) })
                    .ToDictionary(g => g.Type, g => g.Count);
            ;

            foreach (PilotStudent ps in FlightSchool.Students)
            {
                types[ps.Aircraft.Type]--;
            }

            IOrderedEnumerable<TrainingAircraftType> freeTypes =
                types.Where(t => t.Value > 0).Select(t => t.Key).OrderBy(t => t.TypeLevel);

            return FlightSchool.Aircrafts.First(a => a.Type == freeTypes.First());
        }

        private void setHireStudentsStatus()
        {
            double studentPrice = GeneralHelpers.GetInflationPrice(PilotStudent.StudentCost);

            int studentsCapacity =
                Math.Min(
                    FlightSchool.Instructors.Count * Models.Pilots.FlightSchool.MaxNumberOfStudentsPerInstructor,
                    FlightSchool.FlightSchool.TrainingAircrafts.Sum(f => f.Type.MaxNumberOfStudents));

            FlightSchool.HireStudents = studentsCapacity > FlightSchool.Students.Count
                                             && GameObject.GetInstance().HumanAirline.Money > studentPrice;

            Instructors.Clear();

            foreach (
                Instructor instructor in
                    FlightSchool.Instructors.Where(
                        i => i.Students.Count < Models.Pilots.FlightSchool.MaxNumberOfStudentsPerInstructor))
            {
                Instructors.Add(instructor);
            }
        }

        #endregion
    }
}