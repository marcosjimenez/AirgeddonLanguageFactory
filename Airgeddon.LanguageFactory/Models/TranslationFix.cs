using System;
using System.Collections.Generic;
using System.Text;

namespace Airgeddon.LanguageFactory.Models
{
    public class TranslationFix
    {
        public IEnumerable<string> Find { get; set; }
        public string ReplaceWith { get; set; }

    }
}
