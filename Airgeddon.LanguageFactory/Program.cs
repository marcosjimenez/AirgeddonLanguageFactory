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

            return Parser.Default.ParseArguments<GenerateOptions, AddOptions>(args)
               .MapResult(
                 (GenerateOptions opts) => RunGenerate(opts),
                 (AddOptions opts) => RunAdd(opts),
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

        static void CheckInputFileExists()
        {
            if (!File.Exists(GetInputFilename())) { 
                ShowError($"File {TranslationFilename} not found.");
                throw new FileNotFoundException();
            }
        }

        static int RunGenerate(GenerateOptions opts)
        {

            CheckInputFileExists();

            var generator = new TranslationFileGenerator(GetInputFilename());

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

        static int RunAdd(AddOptions opts)
        {
            CheckInputFileExists();

            var manager = new TranslationManager();
            manager.Initialize(opts.Filename);

            try
            {
                manager.AddTranslation(opts.Reference, opts.Language, opts.IsoCode);
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
