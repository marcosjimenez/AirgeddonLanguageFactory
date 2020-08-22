using CommandLine;

namespace Airgeddon.LanguageFactory.Verbs
{
    [Verb("create", HelpText = "Create language_string.sh based on current template.")]
    public class CreateScriptOption : BaseOptions
    {

        [Option('n', "name", Required = false, Default = "language_strings.sh", HelpText = "Script filename.")]
        public string ScriptFilename { get; set; }

        [Option('v', "version", Default = "10.21-1", Required = false, HelpText = "Script version (for language_strings_version variable)")]
        public string Version { get; set; }

    }
}
