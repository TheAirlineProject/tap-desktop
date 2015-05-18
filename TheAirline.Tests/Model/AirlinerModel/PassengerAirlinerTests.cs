using TheAirline.Infrastructure;
using TheAirline.Models.Airliners;

namespace TheAirline.Tests.Model.AirlinerModel
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TheAirline.Model.GeneralModel;

    [TestClass]
    public class PassengerAirlinerTests
    {
        #region Fields

        private Airliner passengerAirliner;

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void ReturnBusinessClassOnlyConfigurationForPassangerAircraft()
        {
            this.passengerAirliner.AddAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.BusinessClass, 24));

            const string Expected = "0F | 24C | 0Y";
            string actual = this.passengerAirliner.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnEconomyClassOnlyConfigurationForPassangerAircraft()
        {
            this.passengerAirliner.AddAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.EconomyClass, 98));

            const string Expected = "0F | 0C | 98Y";
            string actual = this.passengerAirliner.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnEmptyConfigurationForPassangerAircraft()
        {
            const string Expected = "0F | 0C | 0Y";
            string actual = this.passengerAirliner.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnFirstClassOnlyConfigurationForPassangerAircraft()
        {
            this.passengerAirliner.AddAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.FirstClass, 12));

            const string Expected = "12F | 0C | 0Y";
            string actual = this.passengerAirliner.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnThreeClassConfigurationForPassangerAircraft()
        {
            this.passengerAirliner.AddAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.FirstClass, 12));
            this.passengerAirliner.AddAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.BusinessClass, 32));
            this.passengerAirliner.AddAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.EconomyClass, 264));

            const string Expected = "12F | 32C | 264Y";
            string actual = this.passengerAirliner.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnTwoClassConfigurationForPassangerAircraft()
        {
            this.passengerAirliner.AddAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.BusinessClass, 12));
            this.passengerAirliner.AddAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.EconomyClass, 132));

            const string Expected = "0F | 12C | 132Y";
            string actual = this.passengerAirliner.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.passengerAirliner = null;
            Setup.ClearLists();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Setup.LoadAirlinerFacilities();
            Setup.CreateAdvertisementTypes();
            Setup.LoadInflationYears();

            this.passengerAirliner = new Airliner(
                "0",
                Fakes.GetNewDummyPassengerAirliner(),
                "AP-REG",
                new DateTime(1990, 8, 25)) { Airline = Fakes.CreateNewDummyAirline() };
        }

        #endregion
    }
}