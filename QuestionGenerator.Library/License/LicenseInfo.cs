using System;

namespace QuestionGenerator.Library.License
{
    [Serializable]
    public class LicenseInfo
    {
        /// <summary>
        /// Gets or sets the expiry.
        /// </summary>
        /// <value>The expiry.</value>
        public DateTime Expiry { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }


        /// <summary>
        /// Gets or sets the machine identifier.
        /// </summary>
        /// <value>
        /// The machine identifier.
        /// </value>
        public string MachineId { get; set; }
    }
}
