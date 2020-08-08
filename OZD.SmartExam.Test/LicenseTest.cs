using System;
using System.Configuration;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OZD.SmartExam.Library.License;

namespace OZD.SmartExam.Test
{
    [TestClass]
    public class LicenseTest
    {
        private readonly string licenseFile = Path.GetFullPath(ConfigurationManager.AppSettings["LicenseFile"].ToString());

        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void TestCleanup()
        {

        }

        [TestMethod]
        public void LicenseNotFoundTest()
        {
            if (File.Exists(licenseFile))
            {
                File.Delete(licenseFile);
            }

            try
            {
                LicenseManager.IsLicenseValid(out LicenseInfo licenseInfo);
                Assert.Fail();
            }
            catch(LicenseException ex)
            {
                Assert.AreEqual(ex.ErrorCode, LicenseErrorCode.NotFound);
            }
        }

        [TestMethod]
        public void LicenseExpiredTest()
        {
            LicenseManager.WriteLicenseFile(DateTime.Now.AddDays(-2));

            try
            {
                LicenseManager.IsLicenseValid(out LicenseInfo licenseInfo);
                Assert.Fail();
            }
            catch (LicenseException ex)
            {
                Assert.AreEqual(ex.ErrorCode, LicenseErrorCode.Expired);
            }
        }

        [TestMethod]
        public void LicenseInvalidMachineTest()
        {
            LicenseManager.WriteLicenseFile("000-000-3244-345542-387", DateTime.Today.AddDays(5));

            try
            {
                LicenseManager.IsLicenseValid(out LicenseInfo licenseInfo);
                Assert.Fail();
            }
            catch (LicenseException ex)
            {
                Assert.AreEqual(ex.ErrorCode, LicenseErrorCode.InvalidMachine);
            }
        }

        [TestMethod]
        public void LicenseValidTest()
        {
            if (File.Exists(licenseFile))
            {
                File.Delete(licenseFile);
            }

            var serialKey = LicenseManager.GetDiskSerialKey();
            LicenseManager.WriteLicenseFile(serialKey, DateTime.Today.AddDays(5));
            Assert.IsTrue(LicenseManager.IsLicenseValid(out LicenseInfo licenseInfo));
        }

        [TestMethod]
        public void DiskSerialKeyTest()
        {
            Assert.IsNotNull(LicenseManager.GetDiskSerialKey());
        }
        }

}
