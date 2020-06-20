using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Airgeddon.LanguageFactory.Verbs
{
    public class GenerateOptions
    {

        [Option('f', "filename", Required = true, HelpText = "Output filename (json extension added if empty).")]
        public string Filename { get; set; }

    }
}
