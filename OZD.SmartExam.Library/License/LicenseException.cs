namespace OZD.SmartExam.Library.License
{
    using System;

    public class LicenseException : Exception
    {
        /// <summary>
        /// Any information related to license.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errorCode">Error code</param>
        public LicenseException(string message, LicenseErrorCode errorCode)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// The license error code.
        /// </summary>
        public LicenseErrorCode ErrorCode { get; }
    }
}
