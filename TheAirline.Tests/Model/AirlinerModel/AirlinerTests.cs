namespace TheAirline.Tests.Model.AirlinerModel
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using Ninject.MockingKernel.Moq;

    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirlinerModel.RouteModel;
    using TheAirline.Model.GeneralModel;

    [TestClass]
    public class AirlinerTests
    {
        #region Fields

        private Airliner target;

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void ReturnBusinessClassOnlyConfigurationForPassangerAircraft()
        {
            target.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Business_Class, 24));

            const string Expected = "0F | 24C | 0Y";
            string actual = this.target.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnEconomyClassOnlyConfigurationForPassangerAircraft()
        {
            target.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Economy_Class, 98));

            const string Expected = "0F | 0C | 98Y";
            string actual = this.target.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnEmptyConfigurationForPassangerAircraft()
        {
            const string Expected = "0F | 0C | 0Y";
            string actual = this.target.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnFirstClassOnlyConfigurationForPassangerAircraft()
        {
            target.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.First_Class, 12));

            const string Expected = "12F | 0C | 0Y";
            string actual = this.target.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnThreeClassConfigurationForPassangerAircraft()
        {
            target.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.First_Class, 12));
            target.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Business_Class, 32));
            target.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Economy_Class, 264));

            const string Expected = "12F | 32C | 264Y";
            string actual = this.target.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void ReturnTwoClassConfigurationForPassangerAircraft()
        {
            target.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Business_Class, 12));
            target.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Economy_Class, 132));

            const string Expected = "0F | 12C | 132Y";
            string actual = this.target.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.target = null;
            Setup.ClearLists();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Setup.LoadAirlinerFacilities();
            Setup.CreateAdvertisementTypes();
            Setup.LoadInflationYears();

            target = new Airliner("0", GetNewDummyPassengerAirliner(), "AP-REG", new DateTime(1990, 8, 25))
                     {
                         Airline
                             =
                             this
                             .CreateNewDummyairline
                             ()
                     };

        }

        private Airline CreateNewDummyairline()
        {
            return new Airline(
                new AirlineProfile("test Airline", "TS", "Red", "TestName testSurname", true, 2000, 2199),
                Airline.AirlineMentality.Moderate,
                Airline.AirlineFocus.Global,
                Airline.AirlineLicense.Long_Haul, Route.RouteType.Passenger);
        }

        private AirlinerType GetNewDummyPassengerAirliner()
        {
            //var country = new Moq.Mock<Country>();
            //var manufacturer = new Moq.Mock<Manufacturer>();
            //var airlinerPassengerType = new Moq.Mock<AirlinerPassengerType>();

            return new AirlinerPassengerType(
                null,
                "Test Airliner",
                "Test",
                0,
                2,
                2,
                535,
                3040,
                0,
                0,
                0.051,
                90000000,
                3,
                2000,
                20000,
                AirlinerType.BodyType.Narrow_Body,
                AirlinerType.TypeRange.Long_Range,
                AirlinerType.EngineType.Jet,
                new Period<DateTime>(new DateTime(1990, 1, 1), new DateTime(2010, 1, 1)),
                5,
                true,
                true);
        }


        #endregion
    }
}