using Airgeddon.LanguageFactory.Models;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Airgeddon.LanguageFactory
{
    public class TranslationGenerator
    {

        private readonly string _inputFilename;
        private readonly string[] NoIndexWords = { "unknown_chipset", "hintprefix", "optionaltool_needed", "under_construction", "possible_package_names_text", "disabled_text", "reboot_required", "docker_image" };
        private readonly string[] IndexWords = { "et_misc_texts", "wps_texts", "asleap_texts", "jtr_texts", "hashcat_texts", "aircrack_texts", "enterprise_texts", "footer_texts", "arr" };

        public TranslationGenerator(string inputFilename)
        {
            _inputFilename = inputFilename;
        }

        public void GenerateFile(string outputFilename)
        {
            var DestinationFile = new TranslationFile();

            FileStream fileStream = new FileStream(_inputFilename, FileMode.Open);
            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            string line = reader.ReadLine();
            while (line != null)
            {
                var IsChecked = false;
                foreach(var noIndexWord in NoIndexWords)
                {
                    if (IsChecked)
                        break;

                    if (line.Contains($"{noIndexWord}[\""))
                    {
                        var item = GetItemWithNoIndex(line);
                        AddNoIdexWord(DestinationFile, noIndexWord, item);
                        IsChecked = true;
                    }
                }

                if (!IsChecked)
                {
                    if (IsChecked)
                        break;

                    foreach (var indexWord in IndexWords)
                    {
                        if (line.Contains($"{indexWord}[\""))
                        {
                            var item = GetItemWithIndex(line);
                            AddIndexWord(DestinationFile, indexWord, item);
                            IsChecked = true;
                        }
                    }
                }
                line = reader.ReadLine();
            }

            var text = JsonSerializer.Serialize(DestinationFile, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            File.WriteAllText(outputFilename, text);

        }

        private TranslationItem GetItemWithNoIndex(string line)
        {
            var parts = line.Split('=', 2);
            var language = ExtractText(parts[0], '"', '"');
            var text = parts[1]
                .Trim().Substring(1, parts[1].Trim().Length - 2);

            return new TranslationItem
            {
                Language = language,
                Text = text
            };
        }

        private TranslationItemWithIndex GetItemWithIndex(string line)
        {
            var parts = line.Split('=');
            var language = ExtractText(parts[0], '"', '"');
            var index = ExtractText(parts[0], ',', ']');
            var text = parts[1]
                .Trim().Substring(1, parts[1].Trim().Length - 2);

            return new TranslationItemWithIndex
            {
                Index = index,
                Language = language,
                Text = text
            };
        }

        private void AddIndexWord(TranslationFile destinationFile, string indexWord, TranslationItemWithIndex item)
        {
            switch (indexWord)
            {
                case "et_misc_texts":
                    destinationFile.et_misc_texts.Add(item);
                    break;
                case "wps_texts":
                    destinationFile.wps_texts.Add(item);
                    break;
                case "asleap_texts":
                    destinationFile.asleap_texts.Add(item);
                    break;
                case "jtr_texts":
                    destinationFile.jtr_texts.Add(item);
                    break;
                case "hashcat_texts":
                    destinationFile.hashcat_texts.Add(item);
                    break;
                case "aircrack_texts":
                    destinationFile.aircrack_texts.Add(item);
                    break;
                case "enterprise_texts":
                    destinationFile.enterprise_texts.Add(item);
                    break;
                case "footer_texts":
                    destinationFile.footer_texts.Add(item);
                    break;
                case "arr":
                    destinationFile.arr.Add(item);
                    break;
            }

        }

        private void AddNoIdexWord(TranslationFile destinationFile, string noIndexWord, TranslationItem item)
        {
            switch (noIndexWord)
            {
                case "unknown_chipset":
                    destinationFile.unknown_chipset.Add(item);
                    break;
                case "hintprefix":
                    destinationFile.hintprefix.Add(item);
                    break;
                case "optionaltool_needed":
                    destinationFile.optionaltool_needed.Add(item);
                    break;
                case "under_construction":
                    destinationFile.under_construction.Add(item);
                    break;
                case "possible_package_names_text":
                    destinationFile.possible_package_names_text.Add(item);
                    break;
                case "disabled_text":
                    destinationFile.disabled_text.Add(item);
                    break;
                case "reboot_required":
                    destinationFile.reboot_required.Add(item);
                    break;
                case "docker_image":
                    destinationFile.docker_image.Add(item);
                    break;
            }

        }

        private string ExtractText(string text, char start, char end)
        {
            var initPos = text.IndexOf(start);
            var endPos = text.IndexOf(end, initPos + 1);
            return text.Substring(initPos + 1, endPos - (initPos +1));
        }

    }
}
