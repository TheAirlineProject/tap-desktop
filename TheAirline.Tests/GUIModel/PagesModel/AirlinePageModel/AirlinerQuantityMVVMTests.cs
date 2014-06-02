namespace TheAirline.Tests.GUIModel.PagesModel.AirlinePageModel
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using Ninject.MockingKernel.Moq;

    using TheAirline.GUIModel.PagesModel.AirlinePageModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.GeneralModel;

    [TestClass]
    public class AirlinerQuantityMVVMTests
    {
        #region Public Methods and Operators

        [TestMethod]
        public void PassCabinConfigurationToConstructor()
        {
            const string Expected = "12F | 24C | 180Y";

            var target = new AirlinerQuantityMVVM(this.GetNewDummyPassengerAirliner(), Expected, 1);

            Assert.AreEqual(Expected, target.CabinConfiguration);
        }

        #endregion

        #region Methods

        private AirlinerType GetNewDummyPassengerAirliner()
        {
            //var country = new Moq.Mock<Country>();
            //var manufacturer = new Moq.Mock<Manufacturer>();
            //var airlinerPassengerType = new Moq.Mock<AirlinerPassengerType>();

            return new AirlinerPassengerType(
                null,
                "Test Airliner",
                "Test",
                200,
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