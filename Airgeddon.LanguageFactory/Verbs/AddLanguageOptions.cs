﻿using CommandLine;

namespace Airgeddon.LanguageFactory.Verbs
{
    [Verb("addlanguage", HelpText = "Adds a new language.")]
    public class AddLanguageOptions : BaseOptions
    {
        [Option('r', "reference", HelpText = "Reference language (used to translate the new one)")]
        public string Reference { get; set; }
        [Option('l', "language", HelpText = "New language name")]
        public string Language { get; set; }
        [Option('i', "iso", HelpText = "ISO-639-1 Language code")]
        public string IsoCode { get; set; }
        [Option('c', "continue", Required = false, Default = false, HelpText = "Continue the last generation, LastTranslatedIndexWord and LastTranslatedIndexWordIndex needed on config.json")]
        public bool Continue { get; set; }
    }
}
