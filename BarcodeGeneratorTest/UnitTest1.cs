using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BarcodeGeneratorTest
{
    [TestClass]
    public class UnitTest1
    {
        BarcodeGenerator.Code128GS1.Encoder encoder;

        [TestInitialize]
        public void TestSetup()
        {
            encoder = new BarcodeGenerator.Code128GS1.Encoder();
        }

        [TestCleanup]
        public void TestExit()
        {
            encoder = null;
        }

        [TestMethod]
        [TestCategory("Misc methods")]
        public void Test_CalculateChecksum()
        {
            Assert.IsNotNull(encoder);

            // 10, 20, 30, 40 = 10 + (1*20) + (2*30) + (3*40) = 210 % 103 = 4
            Assert.AreEqual(4, encoder.CalculateChecksum(new List<int>(new int[] { 10, 20, 30, 40 })));

            // 35, 29, 115, 94 = 35 + (1*29) + (2*115) + (3*94) = 576 % 103 = 61
            Assert.AreEqual(61, encoder.CalculateChecksum(new List<int>(new int[] { 35, 29, 115, 94 })));
        }

        [TestMethod]
        [TestCategory("Misc methods")]
        public void Test_GetCodeValueForChar()
        {
            Assert.IsNotNull(encoder);

            // (asciiCode >= 32) ? asciiCode - 32 : asciiCode + 64;
            Assert.AreEqual(50 - 32, encoder.GetCodeValueForChar(50));
            Assert.AreEqual(20 + 64, encoder.GetCodeValueForChar(20));
        }

        [TestMethod]
        [TestCategory("Encoding")]
        public void Test_Encode_AutodetectOptimize()
        {
            Assert.IsNotNull(encoder);

            int[] validationValues1 = new int[] { 105, 102, 0, 1, 23, 45, 67, 89, 7, 106 };
            int[] encodeResult1 = encoder.Encode("(00)0123456789");

            Assert.IsTrue(encodeResult1.SequenceEqual(validationValues1));
        }
    }
}
