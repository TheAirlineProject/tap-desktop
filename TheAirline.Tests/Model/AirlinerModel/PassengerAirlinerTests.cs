namespace TheAirline.Tests.Model.AirlinerModel
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using TheAirline.Model.AirlinerModel;
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
            this.passengerAirliner.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Business_Class, 24));

            const string Expected = "0F | 24C | 0Y";
            string actual = this.passengerAirliner.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnEconomyClassOnlyConfigurationForPassangerAircraft()
        {
            this.passengerAirliner.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Economy_Class, 98));

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
            this.passengerAirliner.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.First_Class, 12));

            const string Expected = "12F | 0C | 0Y";
            string actual = this.passengerAirliner.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnThreeClassConfigurationForPassangerAircraft()
        {
            this.passengerAirliner.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.First_Class, 12));
            this.passengerAirliner.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Business_Class, 32));
            this.passengerAirliner.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Economy_Class, 264));

            const string Expected = "12F | 32C | 264Y";
            string actual = this.passengerAirliner.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnTwoClassConfigurationForPassangerAircraft()
        {
            this.passengerAirliner.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Business_Class, 12));
            this.passengerAirliner.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Economy_Class, 132));

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