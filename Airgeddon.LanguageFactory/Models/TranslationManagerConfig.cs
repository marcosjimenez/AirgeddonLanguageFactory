using System;
using System.Collections.Generic;
using System.Text;

namespace Airgeddon.LanguageFactory.Models
{
    public class TranslationManagerConfig
    {
        public Dictionary<string, string> Languages { get; set; }

        public int LastIndex { get; set; }
        public string LastTranslatedIndexWord { get; set; }
        public string LastTranslatedIndexWordIndex { get; set; }

    }
}
