namespace TheAirline.Tests.Model.AirlinerModel
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.GeneralModel;

    [TestClass]
    public class CargoAirlinerTests
    {
        #region Fields

        private Airliner cargoAirliner;

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void ConfigurationShowsCargoCapacity()
        {
            const string Expected = "40 t";
            Assert.AreEqual(Expected, this.cargoAirliner.CabinConfiguration);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Setup.ClearLists();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Setup.LoadAirlinerFacilities();
            Setup.CreateAdvertisementTypes();
            Setup.LoadInflationYears();

            this.cargoAirliner = new Airliner(
                "0",
                Fakes.GetNewDummyCargoAirliner(),
                "AC-REG",
                new DateTime(2000, 8, 25)) { Airline = Fakes.CreateNewDummyAirline() };
        }

        #endregion
    }
}