using TheAirline.Models.Pilots;

namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class FlightSchoolMVVM : INotifyPropertyChanged
    {
        #region Fields

        private Boolean _hireStudents;

        private int _numberofinstructors;

        private int _numberofstudents;

        #endregion

        #region Constructors and Destructors

        public FlightSchoolMVVM(FlightSchool fs)
        {
            this.FlightSchool = fs;

            this.Students = new ObservableCollection<PilotStudent>();
            this.Aircrafts = new ObservableCollection<TrainingAircraft>();
            this.Instructors = new ObservableCollection<Instructor>();

            this.FlightSchool.Students.ForEach(s => this.Students.Add(s));
            this.FlightSchool.TrainingAircrafts.ForEach(a => this.Aircrafts.Add(a));
            this.FlightSchool.Instructors.ForEach(i => this.Instructors.Add(i));
            this.NumberOfStudents = this.Students.Count;
            this.NumberOfInstructors = this.Instructors.Count;
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ObservableCollection<TrainingAircraft> Aircrafts { get; set; }

        public FlightSchool FlightSchool { get; set; }

        public Boolean HireStudents
        {
            get
            {
                return this._hireStudents;
            }
            set
            {
                this._hireStudents = value;
                this.NotifyPropertyChanged("HireStudents");
            }
        }

        public ObservableCollection<Instructor> Instructors { get; set; }

        public int NumberOfInstructors
        {
            get
            {
                return this._numberofinstructors;
            }
            set
            {
                this._numberofinstructors = value;
                this.NotifyPropertyChanged("NumberOfInstructors");
            }
        }

        public int NumberOfStudents
        {
            get
            {
                return this._numberofstudents;
            }
            set
            {
                this._numberofstudents = value;
                this.NotifyPropertyChanged("NumberOfStudents");
            }
        }

        public ObservableCollection<PilotStudent> Students { get; set; }

        #endregion

        //adds a student to the object

        //adds an instructor to the object

        #region Public Methods and Operators

        public void addInstructor(Instructor instructor)
        {
            this.Instructors.Add(instructor);
            this.FlightSchool.AddInstructor(instructor);
            this.NumberOfInstructors = this.Instructors.Count;
        }

        public void addStudent(PilotStudent student)
        {
            this.Students.Add(student);
            this.FlightSchool.AddStudent(student);
            this.NumberOfStudents = this.Students.Count;
        }

        //removes an instructor from the object

        //adds an aircraft to the object
        public void addTrainingAircraft(TrainingAircraft aircraft)
        {
            this.Aircrafts.Add(aircraft);
            this.FlightSchool.AddTrainingAircraft(aircraft);
        }

        public void removeInstructor(Instructor instructor)
        {
            this.Instructors.Remove(instructor);
            this.FlightSchool.RemoveInstructor(instructor);
            this.NumberOfInstructors = this.Instructors.Count;
        }

        public void removeStudent(PilotStudent student)
        {
            this.Students.Remove(student);
            this.FlightSchool.RemoveStudent(student);
            this.NumberOfStudents = this.Students.Count;
        }

        //remove an aircraft from the object
        public void removeTrainingAircraft(TrainingAircraft aircraft)
        {
            this.Aircrafts.Remove(aircraft);
            this.FlightSchool.RemoveTrainingAircraft(aircraft);
        }

        #endregion

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}