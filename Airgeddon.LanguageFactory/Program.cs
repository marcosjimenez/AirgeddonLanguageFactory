namespace Airgeddon.LanguageFactory
{
    using System;
    using System.Diagnostics;
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
            var retVal = 0;
            CheckInputFileExists();

            var manager = new TranslationManager();
            manager.Initialize(opts.Filename);
            manager.ConsoleMessage = (x) => ShowMessage(x);

            var stopGeneration = false;
            while(!stopGeneration)
            {
                try
                {
                    var errors = manager.AddTranslation(opts.Reference, opts.Language, opts.IsoCode, opts.Continue);

                    if (errors.Count == 0)
                    {
                        retVal = 0;
                        stopGeneration = true;
                    }
                    else
                    {
                        var sb = new StringBuilder();
                        foreach (var err in errors)
                            sb.AppendLine(err);
                    
                        ShowError(sb.ToString());
                        // Wait 100 seconds
                        stopGeneration = (!WaitSeconds(100));
                        retVal = 1;
                    }
                }
                catch(Exception ex)
                {
                    stopGeneration = true;
                    ShowError(ex.Message);
                    retVal = 1;
                }
            }
            return retVal;
        }

        private static bool WaitSeconds(int seconds)
        {
            var retVal = true;
            var totalMilliseconds = seconds * 1000;

            Console.TreatControlCAsInput = true;
            Console.CursorVisible = false;

            var posY = Console.CursorTop;

            var watch = new Stopwatch();
            watch.Start();

            while(watch.ElapsedMilliseconds < totalMilliseconds)
            {
                Console.SetCursorPosition(0, posY);
                ShowMessage($"Waiting {watch.ElapsedMilliseconds / 1000} / {seconds} (secs)", ConsoleColor.DarkYellow);
            }
            
            Console.CursorVisible = true;

            watch.Stop();

            return retVal;
        }

    }
}
