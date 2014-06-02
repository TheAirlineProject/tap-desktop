namespace TheAirline.Tests.Model.AirlinerModel
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.GeneralModel;

    [TestClass]
    public class CombiAirlinerTests
    {
        #region Fields

        private Airliner combiAirliner;

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void ShowsOnlyCargoCapacityIfNoSeatsAreAdded()
        {
            const string Expected = "0F | 0C | 0Y | 40 t";
            Assert.AreEqual(Expected, this.combiAirliner.CabinConfiguration);
        }

        [TestMethod]
        public void EconomyClassAndCargo()
        {
            this.combiAirliner.addAirlinerClass(new AirlinerClass(AirlinerClass.ClassType.Economy_Class, 98));

            const string Expected = "0F | 0C | 98Y | 40 t";
            string actual = this.combiAirliner.CabinConfiguration;

            Assert.AreEqual(Expected, actual);
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

            this.combiAirliner = new Airliner(
                "0",
                Fakes.GetNewDummyCombiAirliner(),
                "AM-REG",
                new DateTime(2000, 8, 25)) { Airline = Fakes.CreateNewDummyAirline() };
        }

        #endregion
    }
}