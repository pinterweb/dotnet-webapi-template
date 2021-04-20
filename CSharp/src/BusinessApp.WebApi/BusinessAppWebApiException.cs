using System;
using BusinessApp.Kernel;

namespace BusinessApp.WebApi
{
    /// <summary>
    /// Custom exception to throw when an error occurrs during data operations
    /// </summary>
    [Serializable]
    public class BusinessAppWebApiException : BusinessAppException
    {
        public BusinessAppWebApiException(string message)
            :base(message)
        { }
    }
}
