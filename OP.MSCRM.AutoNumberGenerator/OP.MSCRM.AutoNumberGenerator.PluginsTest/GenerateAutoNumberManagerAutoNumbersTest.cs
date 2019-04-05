using Microsoft.VisualStudio.TestTools.UnitTesting;
using OP.MSCRM.AutoNumberGenerator.Plugins.Managers;


namespace OP.MSCRM.AutoNumberGenerator.PluginsTest
{
    /// <summary>
    /// Auto Number generation manager test. Test Auto Number generation methods.
    /// </summary>
    [TestClass]
    public class GenerateAutoNumberManagerAutoNumbersTest
    {

        public GenerateAutoNumberManager AutoNumberManager
        {
            get
            {
                return new GenerateAutoNumberManager();
            }
        }


        #region GenerateAutoNumber method test

        [TestMethod]
        public void GenerateAutoNumber_MinLengthWithoutPrefixAndSuffix_Valid()
        {
            //Arrange
            int? autoNumberSequence = 1;
            int? autoNumberLenght = 1;
            string autoNumberPrefix = null;
            string autoNumberSuffix = null;

            var expectedAutoNumber = "1";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberLenght, autoNumberSequence, autoNumberPrefix, autoNumberSuffix);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_EncreasedLengthWithoutPrefixAndSuffix_Valid()
        {
            //Arrange
            int? autoNumberSequence = 2;
            int? autoNumberLenght = 4;
            string autoNumberPrefix = null;
            string autoNumberSuffix = null;

            var expectedAutoNumber = "0002";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberLenght, autoNumberSequence, autoNumberPrefix, autoNumberSuffix);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_EncreasedStepWithoutPrefixAndSuffix_Valid()
        {
            //Arrange
            int? autoNumberLenght = 5;
            int? autoNumberSequence = 12;
            string autoNumberPrefix = null;
            string autoNumberSuffix = null;

            var expectedAutoNumber = "00012";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberLenght, autoNumberSequence, autoNumberPrefix, autoNumberSuffix);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_WithPrefixAndWithoutSuffix_Valid()
        {
            //Arrange
            int? autoNumberLenght = 1;
            int? autoNumberSequence = 8;
            string autoNumberPrefix = "A-";
            string autoNumberSuffix = null;
            
            var expectedAutoNumber = "A-8";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberLenght, autoNumberSequence, autoNumberPrefix, autoNumberSuffix);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_WithPrefixAndEmptySuffix_Valid()
        {
            //Arrange
            int? autoNumberLenght = 6;
            int? autoNumberSequence = 20;
            string autoNumberPrefix = "A-";
            string autoNumberSuffix = "";

            var expectedAutoNumber = "A-000020";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberLenght, autoNumberSequence, autoNumberPrefix, autoNumberSuffix);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }

        [TestMethod]
        public void GenerateAutoNumber_WithWhiteSpacePrefixAndSuffix_Valid()
        {
            //Arrange
            int? autoNumberLenght = 4;
            int? autoNumberSequence = 11;
            string autoNumberPrefix = " ";
            string autoNumberSuffix = "-B";

            var expectedAutoNumber = "0011-B";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberLenght, autoNumberSequence, autoNumberPrefix, autoNumberSuffix);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }

        [TestMethod]
        public void GenerateAutoNumber_WithWhiteSpacePrefixAndWithoutSuffix_Valid()
        {
            //Arrange
            int? autoNumberLenght = 3;
            int? autoNumberSequence = 5;
            string autoNumberPrefix = " ";
            string autoNumberSuffix = null;

            var expectedAutoNumber = "005";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberLenght, autoNumberSequence, autoNumberPrefix, autoNumberSuffix);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_WithPrefixAndSuffix_Valid()
        {
            //Arrange
            int? autoNumberLenght = 7;
            int? autoNumberSequence = 17;
            string autoNumberPrefix = "A-";
            string autoNumberSuffix = "-B";

            var expectedAutoNumber = "A-0000017-B";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberLenght, autoNumberSequence, autoNumberPrefix, autoNumberSuffix);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_MinLengthWithPrefixAndSuffix_Valid()
        {
            //Arrange
            int? autoNumberLenght = 0;
            int? autoNumberSequence = 1;
            string autoNumberPrefix = "A-";
            string autoNumberSuffix = "-B";

            var expectedAutoNumber = "A-1-B";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberLenght, autoNumberSequence, autoNumberPrefix, autoNumberSuffix);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }


        [TestMethod]
        public void GenerateAutoNumber_WithLargeSequencePrefixAndSuffix_Valid()
        {
            //Arrange
            int? autoNumberLenght = 6;
            int? autoNumberSequence = 670;
            string autoNumberPrefix = "A-";
            string autoNumberSuffix = "-B";

            var expectedAutoNumber = "A-000670-B";

            //Act
            var actualAutoNumber = AutoNumberManager.GenerateAutoNumber(autoNumberLenght, autoNumberSequence, autoNumberPrefix, autoNumberSuffix);

            //Assert
            Assert.AreEqual(expectedAutoNumber, actualAutoNumber);
        }

        #endregion


        #region GetNumber method test

        [TestMethod]
        public void GetNumber_WithoutPrefixAndSuffix_Valid()
        {
            //Arrange
            string prefix = null;
            string suffix = null;
            string autoNumber = "1";
            int expectedNumber = 1;

            //Act
            int actualNumber= AutoNumberManager.GetNumber(autoNumber, prefix, suffix);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }


        [TestMethod]
        public void GetNumber_WithEncreasedLengthWithoutPrefixAndSuffix_Valid()
        {
            //Arrange
            string prefix = null;
            string suffix = null;
            string autoNumber = "000062";
            int expectedNumber = 62;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber, prefix, suffix);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }


        [TestMethod]
        public void GetNumber_WithPrefix_Valid()
        {
            //Arrange
            string prefix = "AAA-";
            string suffix = null;
            string autoNumber = "AAA-122";
            int expectedNumber = 122;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber, prefix, suffix);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }


        [TestMethod]
        public void GetNumber_WithSuffix_Valid()
        {
            //Arrange
            string prefix = "";
            string suffix = "-BBB";
            string autoNumber = "000120-BBB";
            int expectedNumber = 120;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber, prefix, suffix);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }


        [TestMethod]
        public void GetNumber_WithPrefixAndSuffix_Valid()
        {
            //Arrange
            string prefix = "AAA-";
            string suffix = "-BBB";
            string autoNumber = "AAA-000000900-BBB";
            int expectedNumber = 900;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber, prefix, suffix);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }


        [TestMethod]
        public void GetNumber_WithoutNumberWhitPrefix_Valid()
        {
            //Arrange
            string prefix = "AAA";
            string suffix = null;
            string autoNumber = "AAA";
            int expectedNumber = 0;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber, prefix, suffix);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }

        [TestMethod]
        public void GetNumber_WithNumericPrefix_Valid()
        {
            //Arrange
            string prefix = "42";
            string suffix = null;
            string autoNumber = "420001";
            int expectedNumber = 1;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber, prefix, suffix);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }

        [TestMethod]
        public void GetNumber_WithNumericPrefixAndNumericSuffix_Valid()
        {
            //Arrange
            string prefix = "42";
            string suffix = "0534";
            string autoNumber = "4200040534";
            int expectedNumber = 4;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber, prefix, suffix);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }

        [TestMethod]
        public void GetNumber_WithNumericLetterPrefixAndNumericLetterSuffix_Valid()
        {
            //Arrange
            string prefix = "4A 2";
            string suffix = "0Bs q";
            string autoNumber = "4A 2000000200Bs q";
            int expectedNumber = 20;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber, prefix, suffix);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }

        [TestMethod]
        public void GetNumber_WithSpecificSymbolsPrefix_Valid()
        {
            //Arrange
            string prefix = "4A%-#$ 2";
            string suffix = null;
            string autoNumber = "4A%-#$ 20000201";
            int expectedNumber = 201;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber, prefix, suffix);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }

        [TestMethod]
        public void GetNumber_WithSpecificSymbolsPrefixAndSpecificSymbolsSuffix_Valid()
        {
            //Arrange
            string prefix = "4A%-#$ 2";
            string suffix = " w,!*^";
            string autoNumber = "4A%-#$ 200002010 w,!*^";
            int expectedNumber = 2010;

            //Act
            int actualNumber = AutoNumberManager.GetNumber(autoNumber, prefix, suffix);

            //Assert
            Assert.AreEqual(expectedNumber, actualNumber);
        }
        #endregion

    }
}
