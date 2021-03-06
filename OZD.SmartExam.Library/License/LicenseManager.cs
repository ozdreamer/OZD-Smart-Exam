﻿namespace OZD.SmartExam.Library.License
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Runtime.Serialization.Formatters.Binary;

    public static class LicenseManager
    {
        /// <summary>
        /// Full license key.
        /// </summary>
        private static readonly string fullLicenseKey = "0000-0000-0000-0000";

        /// <summary>
        /// Generates the serial key.
        /// </summary>
        public static string GetDiskSerialKey()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
            {
                if (searcher.Get().OfType<ManagementObject>().FirstOrDefault() is ManagementObject mo)
                {
                    return mo["SerialNumber"].ToString().Trim();
                }

                return null;
            }
        }

        /// <summary>
        /// Determines whether [is license valid].
        /// </summary>
        /// <returns>
        /// The license information.
        /// </returns>
        public static LicenseInfo GetLicense()
        {
            var licenseFile = Path.GetFullPath(ConfigurationManager.AppSettings["LicenseFile"].ToString());
            if (!File.Exists(licenseFile))
            {
                throw new LicenseException("License file not found", LicenseErrorCode.NotFound);
            }

            using (var stream = File.Open(Path.GetFullPath(licenseFile), FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var formatter = new BinaryFormatter();
                var rawObject = formatter.Deserialize(stream);
                if (!(rawObject is LicenseInfo licenseInfo))
                {
                    throw new LicenseException("Invalid license format", LicenseErrorCode.InvalidFormat);
                }

                var validDays = (licenseInfo.Expiry - DateTime.UtcNow.Date).TotalDays;
                if (validDays < 0)
                {
                    throw new LicenseException($"License has expired on {licenseInfo.Expiry.ToShortDateString()}", LicenseErrorCode.Expired);
                }

                var serialKey = GetDiskSerialKey();
                if (serialKey == null)
                {
                    throw new LicenseException("Cannot read the machine identifier", LicenseErrorCode.MachineIdError);
                }

                if (!string.IsNullOrWhiteSpace(licenseInfo.MachineId))
                {
                    if (!string.Equals(serialKey, licenseInfo.MachineId, StringComparison.Ordinal) && !string.Equals(licenseInfo.MachineId, fullLicenseKey, StringComparison.Ordinal))
                    {
                        throw new LicenseException("License is not valid for this machine", LicenseErrorCode.InvalidMachine);
                    }
                }

                return licenseInfo;
            }
        }

        /// <summary>
        /// Temporaries the write license file.
        /// </summary>
        public static void TempWriteLicenseFile() => WriteLicenseFile(null, new DateTime(31, 12, 2021));

        /// <summary>
        /// Temporaries the write license file.
        /// </summary>
        /// <param name="expiry">Expiry date.</param>
        public static void WriteLicenseFile(DateTime expiry) => WriteLicenseFile(null, expiry);

        /// <summary>
        /// Writes the license file based on the machine id and expiry date.
        /// </summary>
        /// <param name="machineId">The machine identifier.</param>
        /// <param name="expiry">Expiry date.</param>
        public static void WriteLicenseFile(string machineId, DateTime expiry)
        {
            WriteLicenseFile(ConfigurationManager.AppSettings["LicenseFile"].ToString(), machineId, expiry);
        }

        /// <summary>
        /// Writes the license file based on the machine id and expiry date.
        /// </summary>
        /// <param name="licenseFile">The license file.</param>
        /// <param name="machineId">The machine identifier.</param>
        /// <param name="expiry">Expiry date.</param>
        public static void WriteLicenseFile(string licenseFile, string machineId, DateTime expiry)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(licenseFile));
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = File.Open(licenseFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var formatter = new BinaryFormatter();
                var licenseInfo = new LicenseInfo
                {
                    Expiry = expiry,
                    MachineId = machineId,
                    Key = Guid.NewGuid().ToString(),
                    Version = "1.0",
                };

                formatter.Serialize(stream, licenseInfo);
                stream.Flush();
                stream.Close();
            }
        }
    }
}