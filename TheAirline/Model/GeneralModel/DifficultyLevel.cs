using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TheAirline.Model.GeneralModel
{
    //the class for a difficulty level
    [Serializable]
    public class DifficultyLevel : BaseModel
    {
        #region Constructors and Destructors

        public DifficultyLevel(
            string name,
            double money,
            double loan,
            double passengers,
            double price,
            double ai,
            double startdata)
        {
            Name = name;
            MoneyLevel = money;
            LoanLevel = loan;
            PassengersLevel = passengers;
            PriceLevel = price;
            AILevel = ai;
            StartDataLevel = startdata;
        }

        private DifficultyLevel(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("ailevel")]
        public double AILevel { get; set; }

        [Versioning("loanlevel")]
        public double LoanLevel { get; set; }

        [Versioning("moneylevel")]
        public double MoneyLevel { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        [Versioning("passengerlevel")]
        public double PassengersLevel { get; set; }

        [Versioning("pricelevel")]
        public double PriceLevel { get; set; }

        [Versioning("startdatalevel")]
        public double StartDataLevel { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }

    //the list of diffiulty levels
    public class DifficultyLevels
    {
        #region Static Fields

        private static List<DifficultyLevel> _levels = new List<DifficultyLevel>();

        #endregion

        #region Public Methods and Operators

        public static void AddDifficultyLevel(DifficultyLevel level)
        {
            _levels.Add(level);
        }

        public static void Clear()
        {
            _levels = new List<DifficultyLevel>();
        }

        //returns the list of levels

        //returns a difficulty level based on name
        public static DifficultyLevel GetDifficultyLevel(string name)
        {
            return _levels.Find(l => l.Name == name);
        }

        public static List<DifficultyLevel> GetDifficultyLevels()
        {
            return _levels;
        }

        //removes a difficulty level
        public static void RemoveDifficultyLevel(DifficultyLevel level)
        {
            _levels.Remove(level);
        }

        #endregion

        //adds a difficulty level to the level

        //clears the list of difficulties
    }
}