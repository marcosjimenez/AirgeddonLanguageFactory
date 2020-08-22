using CommandLine;

namespace Airgeddon.LanguageFactory.Verbs
{
    public class BaseOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Output filename (json extension added if empty).")]
        public string Filename { get; set; }

    }
}
