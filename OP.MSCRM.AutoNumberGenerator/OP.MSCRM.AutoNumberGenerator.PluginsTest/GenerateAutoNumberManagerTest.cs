using Microsoft.VisualStudio.TestTools.UnitTesting;
using OP.MSCRM.AutoNumberGenerator.Plugins.Managers;


namespace OP.MSCRM.AutoNumberGenerator.PluginsTest
{
    /// <summary>
    /// Auto Number generation manager test. Test Auto Number generation methods.
    /// </summary>
    [TestClass]
    public class GenerateAutoNumberManagerTest
    {

        private GenerateAutoNumberManager AutoNumberManager
        {
            get
            {
                return new GenerateAutoNumberManager();
            }
        }


        #region GenerateAutoNumber method test

        [TestMethod]
        public void GenerateAutoNumber_SimpleValid()
        {
            //Arrange
            string autoNumberFormat = "{0}";
            int? autoNumberFormatNumberLenght = null;
            int? autoNumberSequence = null;
            var expectedAutoNumber = "1";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberFormat, autoNumberFormatNumberLenght, autoNumberSequence);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_SimpleWhitFormatNumberValid()
        {
            //Arrange
            string autoNumberFormat = "{0}";
            int? autoNumberFormatNumberLenght = 5;
            int? autoNumberSequence = null;
            var expectedAutoNumber = "00001";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberFormat, autoNumberFormatNumberLenght, autoNumberSequence);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_SimpleWhitFormatNumberAndSeqValid()
        {
            //Arrange
            string autoNumberFormat = "{0}";
            int? autoNumberFormatNumberLenght = 4;
            int? autoNumberSequence = 2;
            var expectedAutoNumber = "0003";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberFormat, autoNumberFormatNumberLenght, autoNumberSequence);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_SuffixValid()
        {
            //Arrange
            string autoNumberFormat = "ASSA-{0}";
            int? autoNumberFormatNumberLenght = null;
            int? autoNumberSequence = null;
            var expectedAutoNumber = "ASSA-1";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberFormat, autoNumberFormatNumberLenght, autoNumberSequence);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_SuffixWhitFormatNumberValid()
        {
            //Arrange
            string autoNumberFormat = "ASSA-{0}";
            int? autoNumberFormatNumberLenght = 3;
            int? autoNumberSequence = null;
            var expectedAutoNumber = "ASSA-001";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberFormat, autoNumberFormatNumberLenght, autoNumberSequence);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_SuffixWhitFormatNumberAndSeqValid()
        {
            //Arrange
            string autoNumberFormat = "ASSA-{0}";
            int? autoNumberFormatNumberLenght = 3;
            int? autoNumberSequence = 5;
            var expectedAutoNumber = "ASSA-006";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberFormat, autoNumberFormatNumberLenght, autoNumberSequence);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_SuffixPrefixValid()
        {
            //Arrange
            string autoNumberFormat = "ASSA-{0}-BASS";
            int? autoNumberFormatNumberLenght = null;
            int? autoNumberSequence = null;
            var expectedAutoNumber = "ASSA-1-BASS";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberFormat, autoNumberFormatNumberLenght, autoNumberSequence);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_SuffixPrefixWhitFormatNumberValid()
        {
            //Arrange
            string autoNumberFormat = "ASSA-{0}-BASS";
            int? autoNumberFormatNumberLenght = 6;
            int? autoNumberSequence = null;
            var expectedAutoNumber = "ASSA-000001-BASS";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberFormat, autoNumberFormatNumberLenght, autoNumberSequence);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_SuffixPrefixWhitFormatNumberAndSeqValid()
        {
            //Arrange
            string autoNumberFormat = "ASSA-{0}-BASS";
            int? autoNumberFormatNumberLenght = 6;
            int? autoNumberSequence = 67;
            var expectedAutoNumber = "ASSA-000068-BASS";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberFormat, autoNumberFormatNumberLenght, autoNumberSequence);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }

        #endregion


        #region GetNumber method test

        [TestMethod]
        public void GetNumber_SimpleValid()
        {
            //Arrange
            string autoNumber = "1";
            int expectedNumber = 1;

            //Act
            int actualNumber= AutoNumberManager.GetNumber(autoNumber);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }


        [TestMethod]
        public void GetNumber_SimpleWhitNumberValid()
        {
            //Arrange
            string autoNumber = "000062";
            int expectedNumber = 62;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }


        [TestMethod]
        public void GetNumber_SimpleWhitPrefixValid()
        {
            //Arrange
            string autoNumber = "AAA-122";
            int expectedNumber = 122;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }


        [TestMethod]
        public void GetNumber_SimpleWhitSuffixValid()
        {
            //Arrange
            string autoNumber = "000120-BBB";
            int expectedNumber = 120;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }


        [TestMethod]
        public void GetNumber_SimpleWhitPrefixAndSuffixValid()
        {
            //Arrange
            string autoNumber = "AAA-000000900-BBB";
            int expectedNumber = 900;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }


        [TestMethod]
        public void GetNumber_SimpleWithoutNumberValid()
        {
            //Arrange
            string autoNumber = "AAA";
            int expectedNumber = 0;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }

        #endregion

    }
}
