using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.PilotModel;

namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    public class FlightSchoolMVVM : INotifyPropertyChanged
    {
        public FlightSchool FlightSchool { get; set; }
        public ObservableCollection<PilotStudent> Students { get; set; }
        public ObservableCollection<TrainingAircraft> Aircrafts { get; set; }
        public ObservableCollection<Instructor> Instructors { get; set; }
        private Boolean _hireStudents;
        public Boolean HireStudents
        {
            get { return _hireStudents; }
            set { _hireStudents = value; NotifyPropertyChanged("HireStudents"); }
        }
        private int _numberofstudents;
        public int NumberOfStudents
        {
            get { return _numberofstudents; }
            set { _numberofstudents = value; NotifyPropertyChanged("NumberOfStudents"); }
        }
        private int _numberofinstructors;
        public int NumberOfInstructors
        {
            get { return _numberofinstructors; }
            set { _numberofinstructors = value; NotifyPropertyChanged("NumberOfInstructors"); }
        }
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
        //adds a student to the object
        public void addStudent(PilotStudent student)
        {
            this.Students.Add(student);
            this.FlightSchool.addStudent(student);
            this.NumberOfStudents = this.Students.Count;
        }
        //removes a student from the object
        public void removeStudent(PilotStudent student)
        {
            this.Students.Remove(student);
            this.FlightSchool.removeStudent(student);
            this.NumberOfStudents = this.Students.Count;
        }
        //adds an instructor to the object
        public void addInstructor(Instructor instructor)
        {
            this.Instructors.Add(instructor);
            this.FlightSchool.addInstructor(instructor);
            this.NumberOfInstructors = this.Instructors.Count;
        }
        //removes an instructor from the object
        public void removeInstructor(Instructor instructor)
        {
            this.Instructors.Remove(instructor);
            this.FlightSchool.removeInstructor(instructor);
            this.NumberOfInstructors = this.Instructors.Count;
        }
       
        //adds an aircraft to the object
        public void addTrainingAircraft(TrainingAircraft aircraft)
        {
            this.Aircrafts.Add(aircraft);
            this.FlightSchool.addTrainingAircraft(aircraft);
        }
        //remove an aircraft from the object
        public void removeTrainingAircraft(TrainingAircraft aircraft)
        {
            this.Aircrafts.Remove(aircraft);
            this.FlightSchool.removeTrainingAircraft(aircraft);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
