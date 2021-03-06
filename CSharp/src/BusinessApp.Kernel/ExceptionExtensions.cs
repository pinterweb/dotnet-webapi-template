using System;
using System.Collections.Generic;

namespace BusinessApp.Kernel
{
    /// <summary>
    /// Extension methods for exceptions
    /// </summary>
    public static class ExceptionExtensions
    {
        public static IEnumerable<Exception> Flatten(this Exception e)
        {
            if (e == null) yield break;

            var ex = e;

            while (ex != null)
            {
                yield return ex;

                ex = ex.InnerException;
            }
        }
    }
}
