namespace Airgeddon.LanguageFactory
{
    using System;
    using System.IO;
    using System.Text;
    using Airgeddon.LanguageFactory.Verbs;
    using CommandLine;
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

        private static void ShowError(string message) => ShowMessage(message, ConsoleColor.Red);

        private static void ShowMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            var lastColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = lastColor;
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
                ShowMessage($"Generated file {opts.Filename}");
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
            manager.ConsoleMessage = (x) => ShowMessage(x);

            try
            {
                var errors = manager.AddTranslation(opts.Reference, opts.Language, opts.IsoCode);

                if (errors.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var err in errors)
                        sb.AppendLine(err);
                    ShowError(sb.ToString());
                }
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
