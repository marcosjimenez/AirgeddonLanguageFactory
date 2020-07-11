using CommandLine;

namespace Airgeddon.LanguageFactory.Verbs
{
    [Verb("add", HelpText = "Adds a new language.")]
    public class AddOptions : BaseOptions
    {
        [Option('r', "reference", HelpText = "Reference language (used to translate the new one)")]
        public string Reference { get; set; }
        [Option('l', "language", HelpText = "New language name")]
        public string Language { get; set; }
        [Option('i', "iso", HelpText = "ISO-639-1 Language code")]
        public string IsoCode { get; set; }
        [Option('c', "continue", HelpText = "Continue the las generation, LastTranslatedIndexWord and LastTranslatedIndexWordIndex needed on config.json")]
        public bool Continue { get; set; }

    }
}
