﻿using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace CTA.TestData.Test
{
    [TestFixture]
    public class MyTest
    {
        private CTA.TestData.ITestCase ctaTestCase = null;

        [SetUp]
        public void Init()
        {
            ctaTestCase = CTA.TestData.Connector.GetTestCase(true);
        }

        [TestCase(Description=@"Checking disk free space", Category="System")]
        public void TestingDriveFreeSpace()
        {  
            DriveInfo drive = DriveInfo.GetDrives().First(di => di.Name.StartsWith("C", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(drive.IsReady, "Drive is not ready");
            Assert.Greater(drive.TotalFreeSpace, 1000, "Drive has less than 1000 bytes free");
            if (ctaTestCase != null)
            {
                ctaTestCase.AddTestData("TotalFreeSpace", drive.TotalFreeSpace.ToString());
            }
        }
        [Test]
        public void TestingEikon()
        {
            Assert.IsTrue(Directory.Exists(@"C:\Program Files (x86)\Thomson Reuters"), "Missing Thomson Reuters folder");
        }
    }

    [TestFixture]
    public class MyTest2
    {
        private CTA.TestData.ITestCase ctaTestCase = null;

        [SetUp]
        public void Init()
        {
            ctaTestCase = CTA.TestData.Connector.GetTestCase();
        }

        [TestCase(Description = @"Checking RAM", Category = "System")]
        public void TestingRAM()
        {
            var ram = GC.GetTotalMemory(false);
            Assert.Greater(ram, 1000, "RAM is less than 1000 bytes");
            if (ctaTestCase != null)
            {
                ctaTestCase.AddTestData("TotalMemory", ram.ToString());
                ctaTestCase.AddTestData("Random value", new Random().NextDouble().ToString());
            }
        }

        [TestCase(Description = @"Checking stuff")]
        public void MoreValues()
        {
            if (ctaTestCase != null)
            {
                bool b = ctaTestCase.AddTestData("Data1", "should not be there");
                b = b && ctaTestCase.AddTestData("Data1", new Random().NextDouble().ToString());
                b = b && ctaTestCase.AddTestData("Data2", new Random().NextDouble().ToString());
                if (!b)
                {
                    Assert.Inconclusive("Cannot add test data");
                }
            }
        }
    }
}  