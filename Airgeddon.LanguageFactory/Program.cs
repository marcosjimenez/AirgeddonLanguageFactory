using System;
using System.IO;
using Airgeddon.LanguageFactory.Verbs;
using CommandLine;

namespace Airgeddon.LanguageFactory
{
    class Program
    {
        private const string TranslationFilename = "language_strings.sh";

        static int Main(string[] args)
        {

            return Parser.Default.ParseArguments<GenerateOptions>(args)
               .MapResult(
                 (GenerateOptions opts) => RunGenerate(opts),
                 errs => 1);
        }

        private static string GetInputFilename() => Path.Combine(Directory.GetCurrentDirectory(), TranslationFilename);

        private static void ShowError(string description)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(description);
            Console.ForegroundColor = color;
        }

        static int RunGenerate(GenerateOptions opts)
        {

            if (!File.Exists(GetInputFilename()))
                ShowError($"File {TranslationFilename} not found.");

            var generator = new TranslationGenerator(GetInputFilename());

            try
            {
                generator.GenerateFile(opts.Filename);
                Console.Write($"Generated file {opts.Filename}");
                return 0;
            }
            catch(Exception ex)
            {
                ShowError(ex.Message);
                return 1;
            }
        }

    }
}
