namespace Airgeddon.LanguageFactory
{
    using System;
    using System.Collections.Generic;
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

            return Parser.Default.ParseArguments<GenerateOptions, 
                AddLanguageOptions, 
                CreateScriptOption, 
                FixesOptions,
                AddSentenceOptions>(args)
               .MapResult(
                 (GenerateOptions opts) => RunGenerate(opts),
                 (AddLanguageOptions opts) => RunAddLanguage(opts),
                 (AddSentenceOptions opts) => RunAddSentence(opts),
                 (CreateScriptOption opts) => RunCreateScript(opts),
                 (FixesOptions opts) => RunFixes(opts),
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

        static int RunAddLanguage(AddLanguageOptions opts)
        {
            var retVal = 0;
            CheckInputFileExists();

            var manager = new TranslationManager();
            manager.Initialize(opts.Filename);
            manager.ConsoleMessage = (x) => ShowMessage(x);

            var stopGeneration = false;
            var generateScript = false;
            while(!stopGeneration)
            {
                ShowMessage("Starting generation.", ConsoleColor.DarkYellow);
                List<string> errors = new List<string>();
                try
                {
                    errors = manager.AddTranslation(opts.Reference, opts.Language, opts.IsoCode, opts.Continue);
                }
                catch(Exception ex)
                {
                    stopGeneration = true;
                    generateScript = false;
                    retVal = 1;
                    errors.Add(ex.Message);
                }

                if (errors.Count == 0)
                {
                    retVal = 0;
                    stopGeneration = true;
                    generateScript = true;
                }
                else
                {
                    ShowErrors(errors);
                    // Wait 100 seconds
                    stopGeneration = (!WaitSeconds(100));
                    opts.Continue = true;
                    retVal = 1;
                    generateScript = false;
                }
            }

            return retVal;
        }

        static int RunAddSentence(AddSentenceOptions opts)
        {
            var manager = new TranslationManager
            {
                ConsoleMessage = (x) => ShowMessage(x)
            };
            manager.Initialize(opts.Filename);

            var retVal = manager.AddSentence(opts.ArrayName, opts.Reference, opts.Sentence);

            return retVal ? 0 : 1;
        }

        static int RunCreateScript(CreateScriptOption opts)
        {
            var manager = new TranslationManager
            {
                ConsoleMessage = (x) => ShowMessage(x)
            };
            manager.Initialize(opts.Filename);

            ShowMessage($"Generating script {opts.ScriptFilename}");

            var retVal = RunScriptCreation(manager, opts); // TODO: use on Add translation too

            if (retVal == 0)
                ShowMessage("Script generated.");

            return retVal;
        }

        static int RunScriptCreation(TranslationManager manager, CreateScriptOption opts)
            => manager?.GenerateScript(opts.ScriptFilename, opts.Version) == true ? 0 : 1;


        static int RunFixes(FixesOptions opts)
        {
            var manager = new TranslationManager
            {
                ConsoleMessage = (x) => ShowMessage(x)
            };
            manager.Initialize(opts.Filename);

            ShowMessage($"Applying fixes on {opts.Filename}");

            manager.ApplyFixes();

            return 0;
        }

        private static void ShowErrors(List<string> errors)
        {
            var sb = new StringBuilder();
            foreach (string err in errors)
            {
                sb.AppendLine(err);
            }

            ShowError(sb.ToString());
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

            while(watch.ElapsedMilliseconds <= totalMilliseconds)
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
