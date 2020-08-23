using CommandLine;

namespace Airgeddon.LanguageFactory.Verbs
{
    [Verb("addsentence", HelpText = "Adds a new sentence to the selected array.")]
    public class AddSentenceOptions : BaseOptions
    {
        [Option('r', "reference", Required = true, HelpText = "ISO-639-1 Reference language (used to translate the new one)")]
        public string Reference { get; set; }
        [Option('s', "sentence", Required = true, HelpText = "Sentence to add (use reference language)")]
        public string Sentence { get; set; }
        [Option('a', "arrayname", Required = true, HelpText = "Add to this array name")]
        public string ArrayName { get; set; }
    }
}
