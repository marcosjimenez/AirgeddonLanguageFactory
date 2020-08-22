using System;
using System.Collections.Generic;
using System.Text;

namespace Airgeddon.LanguageFactory.Infrastructure.Exceptions
{
    public class TranslationLimitReachedException : Exception
    {
        public TranslationLimitReachedException() : base() { }
        public TranslationLimitReachedException(string message) : base(message) { }
        public TranslationLimitReachedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
