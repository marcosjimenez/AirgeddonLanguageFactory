namespace Airgeddon.LanguageFactory
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Net.Http;
    using GoogleTranslateFreeApi;
    using Airgeddon.LanguageFactory.Models;
    using Airgeddon.LanguageFactory.Helpers;
    using Airgeddon.LanguageFactory.Infrastructure.Exceptions;
    using Antlr4.StringTemplate;
    using System.Security.Cryptography.X509Certificates;
    using System.Runtime.InteropServices.ComTypes;

    public class TranslationManager
    {
        private const string ConfigFile = "config.json";
        private const string SH_File_Template = "template.sh";
        private const string PendingOfTranslation = "\\${pending_of_translation} ";

        private TranslationFile _translations;
        private TranslationManagerConfig _config;
        private string _translationsFile = string.Empty;

        public Action<string> ConsoleMessage = null;

        public void Initialize(string inputFile)
        {
            if (!File.Exists(inputFile))
                throw new FileNotFoundException(inputFile);

            _config = GetConfig();

            _translations = inputFile.FromJson<TranslationFile>();

            _translationsFile = inputFile;
        }

        public List<string> AddTranslation(string referenceLanguage, string newLanguage, string isoCode, bool continueGeneration)
        {
            var retVal = new List<string>();

            referenceLanguage = referenceLanguage.ToUpper();
            newLanguage = newLanguage.ToUpper();

            // Use unknown_chipset as index and language check for all arrays.
            if (!_translations.unknown_chipset.Any(x => x.Language.ToUpper() == referenceLanguage))
                throw new Exception($"Invalid reference language {referenceLanguage}");

            if (!continueGeneration && _translations.unknown_chipset.Any(x => x.Language.ToUpper() == newLanguage))
                throw new Exception($"New language {newLanguage} already exists");

            var isoReference = GetIsoFromLanguage(referenceLanguage);

            if (!continueGeneration)
            {
                var noIndexText = GetNoIndexWords();
                var translatedIndexText = TranslateText(noIndexText, isoReference, isoCode);
                var translatedIndexItems = translatedIndexText.Split(Environment.NewLine);

                // No index words
                for (int i = 0; i < TranslationConstants.NoIndexWords.Length; i++)
                {
                    AddTranslatedItemNoIndex(TranslationConstants.NoIndexWords[i], translatedIndexItems[i]);
                }
            }

            // Index words
            var wordList = TranslationConstants.IndexWords.ToList();
            bool continueNext = true;
            bool haveErrors = false;
            bool useContinuation = continueGeneration;
            foreach (var item in wordList)
            {

                if (!continueNext)
                    break;

                ShowMessage($"Translating {item}");
                try
                {
                    var wordIndex = wordList.IndexOf(item);

                    if (continueGeneration && wordIndex <= _config.LastIndex)
                        continue;

                    int containedIndex = -1;
                    if (useContinuation)
                    {
                        if (string.IsNullOrWhiteSpace(_config.LastTranslatedIndexWordIndex) ||
                            !int.TryParse(_config.LastTranslatedIndexWordIndex, out containedIndex))
                        {
                            containedIndex = -1;
                        }
                        else
                        {
                            containedIndex++;
                        }

                        useContinuation = false;
                    }

                    AddTranslatedItem(item, containedIndex);
                    
                    _config.LastIndex = wordIndex;
                }
                catch (TranslationLimitReachedException limitEx)
                {
                    ShowMessage($"Limit reached on {item}: {limitEx}");
                    haveErrors = true;
                    continueNext = false;
                }
                catch (Exception ex)
                {
                    ShowMessage($"Error: {ex.Message}");
                    haveErrors = true;
                    continueNext = true;
                }
                finally
                {
                    ShowMessage(Environment.NewLine);
                    if (haveErrors)
                    {
                        retVal.Add($"Generation interrupted at index: {_config.LastIndex}");
                        continueNext = false;
                    }
                    else
                    {
                        if (_config.LastIndex == wordList.Count)
                        { 
                            ShowMessage($"Generation completed. Last index: {_config.LastIndex}");
                            continueNext = false;
                        }
                    }
                }
            }

            _config.ToJsonFile(GetConfigFile());

            try
            {
                _translations.ToJsonFile(_translationsFile);
            }
            catch(Exception ex)
            {
                retVal.Add(ex.Message);
            }

            return retVal;

            // Local functions

            void AddTranslatedItem(string item, int continueOnIndex = -1)
            {
                var type = _translations.GetType().GetProperty(item).GetValue(_translations, null) as List<TranslationItemWithIndex>;
                var containedItems = type.Where(x => x.Language.ToUpper() == referenceLanguage).ToList();
                if (containedItems == null)
                    retVal.Add($"Cannot find {item} on json file");
                else
                {
                    foreach (var contained in containedItems)
                    {
                        ShowMessage($"Translating contained at {contained.Index}: {contained.Text}");
                        try
                        {
                            if ((continueOnIndex >= 0) && (int.Parse(contained.Index) < continueOnIndex))
                                continue;

                            type.Add(new TranslationItemWithIndex
                            {
                                Language = newLanguage,
                                Text = string.Concat(PendingOfTranslation, TranslateText(contained.Text, isoReference, isoCode)),
                                Index = contained.Index
                            });
                            _config.LastTranslatedIndexWord = contained.Text;
                            _config.LastTranslatedIndexWordIndex = contained.Index;
                        }
                        catch (HttpRequestException ex)
                        {
                            throw new TranslationLimitReachedException(ex.Message);
                        }
                        catch
                        {
                            throw;
                        }
                    }
                }
            }

            string GetNoIndexWords()
            {
                // No index words
                var sb = new StringBuilder();
                foreach (var item in TranslationConstants.NoIndexWords)
                {
                    ShowMessage($"Translating {item}");
                    sb.AppendLine(GetItemNoIndex(item));
                }
                return sb.ToString();
            }

            string GetItemNoIndex(string item)
            {
                var itemText = string.Empty;
                var type = _translations.GetType().GetProperty(item).GetValue(_translations, null) as List<TranslationItem>;
                var containedItem = type.FirstOrDefault(x => x.Language.ToUpper() == referenceLanguage);
                if (containedItem == null)
                    retVal.Add($"Cannot find {item} on json file");
                else
                    itemText = containedItem.Text;

                return itemText;
            }

            void AddTranslatedItemNoIndex(string item, string translatedText) // Local func
            {
                var type = _translations.GetType().GetProperty(item).GetValue(_translations, null) as List<TranslationItem>;
                var containedItem = type.FirstOrDefault(x => x.Language.ToUpper() == referenceLanguage);
                if (containedItem == null)
                    retVal.Add($"Cannot find {item} on json file");
                else
                    type.Add(new TranslationItem
                    {
                        Language = newLanguage,
                        Text = string.Concat(PendingOfTranslation, translatedText)
                    });
            }
        }

        public bool AddSentence(string arrayName, string reference, string sentence)
        {

            if (!_config.Languages.Any(x => x.Value == reference))
            { 
                ShowMessage($"Reference language [{reference}] not found in config.json language list (use ISO code)");
                return false;
            }

            if (!TranslationConstants.IndexWords.Contains(arrayName.Trim().ToLower()))
            {
                ShowMessage($"Invalid array name: {arrayName}");
                return false;
            }
            else
            {
                ShowMessage($"Adding new sentence to {arrayName}");

                var type = _translations.GetType()
                                        .GetProperty(arrayName)
                                        .GetValue(_translations, null)
                                        as List<TranslationItemWithIndex>;

                int newIndex;
                if (type != null)
                    newIndex = type.Max(x => int.Parse(x.Index)) + 1;
                else
                    newIndex = 0;

                foreach (var language in _config.Languages)
                {
                    ShowMessage($"Translating to {language}");
                    string text = (language.Value != reference) ? 
                        string.Concat(PendingOfTranslation, TranslateText(sentence, reference, language.Value))
                        : sentence;

                    type.Add(new TranslationItemWithIndex
                    {
                        Index = newIndex.ToString(),
                        Language = language.Key,
                        Text = text
                    });
                }
            }

            _translations.SortIndexItems();
            _translations.ToJsonFile(_translationsFile);

            return true;
        }

        public bool GenerateScript(string destinationFilename, string version)
        {
            bool retVal = true;

            if (!File.Exists(SH_File_Template))
                throw new FileNotFoundException(SH_File_Template);

            var template = File.ReadAllText(SH_File_Template);
            var engine = new Template(template, '¬', '¬');

            //Prepare translations
            _translations.SortIndexItems();

            StringBuilder sb = new StringBuilder();
            string CreateIndexString(List<TranslationItemWithIndex> array, string name)
            {
                sb.Clear();
                string lastIndex = string.Empty;
                if (array == null)
                    return sb.ToString();

                foreach (var item in array)
                {
                    if (!string.IsNullOrEmpty(lastIndex) && !lastIndex.Equals(item.Index))
                        sb.AppendLine();

                    sb.AppendLine($"{name}[\"{item.Language}\",{item.Index}]=\"{item.Text}\"");
                    lastIndex = item.Index;
                }
                return sb.ToString();
            }

            string CreateNoIndexString(List<TranslationItem> array, string name)
            {
                sb.Clear();
                if (array == null)
                    return sb.ToString();

                foreach (var item in array)
                {
                    sb.AppendLine($"{name}[\"{item.Language}\"]=\"{item.Text}\"");
                }
                return sb.ToString();
            }

            engine.Add("Data", new
            {
                Version = version,
                unknown_chipset = CreateNoIndexString(_translations.unknown_chipset, "unknown_chipset"),
                hintprefix = CreateNoIndexString(_translations.hintprefix, "hintprefix"),
                optionaltool_needed = CreateNoIndexString(_translations.optionaltool_needed, "optionaltool_needed"),
                under_construction = CreateNoIndexString(_translations.under_construction, "under_construction"),
                possible_package_names_text = CreateNoIndexString(_translations.possible_package_names_text, "possible_package_names_text"),
                disabled_text = CreateNoIndexString(_translations.disabled_text, "disabled_text"),
                reboot_required = CreateNoIndexString(_translations.reboot_required, "reboot_required"),
                docker_image = CreateNoIndexString(_translations.docker_image, "docker_image"),
                et_misc_texts = CreateIndexString(_translations.et_misc_texts, "et_misc_texts"),
                wps_texts = CreateIndexString(_translations.wps_texts, "wps_texts"),
                wep_texts = CreateIndexString(_translations.wep_texts, "wep_texts"),
                asleap_texts = CreateIndexString(_translations.asleap_texts, "asleap_texts"),
                jtr_texts = CreateIndexString(_translations.jtr_texts, "jtr_texts"),
                hashcat_texts = CreateIndexString(_translations.hashcat_texts, "hashcat_texts"),
                aircrack_texts = CreateIndexString(_translations.aircrack_texts, "aircrack_texts"),
                enterprise_texts = CreateIndexString(_translations.enterprise_texts, "enterprise_texts"),
                footer_texts = CreateIndexString(_translations.footer_texts, "footer_texts"),
                arr = CreateIndexString(_translations.arr, "arr"),
            });
            var renderedText = engine.Render();

            // Data.Translations.et_misc_texts : { translation | et_misc_texts["¬translation.Language¬", ¬translation.Index¬]="¬translation.Text¬"

            File.WriteAllText(destinationFilename, renderedText);

            return retVal;
        }

        public void ApplyFixes()
        {

            //item.Text = FixTranslation(item.Text);

            // No index words
            for (int i = 0; i < TranslationConstants.NoIndexWords.Length; i++)
            {
                ShowMessage($"Fixing {TranslationConstants.NoIndexWords[i]}");
                var type = _translations.GetType().GetProperty(TranslationConstants.NoIndexWords[i])
                    .GetValue(_translations, null) as List<TranslationItem>;

                if (type == null)
                    continue;

                foreach (var item in type)
                {
                    item.Text = FixTranslation(item.Text);
                }
            }

            // index words
            for (int i = 0; i < TranslationConstants.IndexWords.Length; i++)
            {
                ShowMessage($"Fixing {TranslationConstants.IndexWords[i]}");
                var type = _translations.GetType().GetProperty(TranslationConstants.IndexWords[i])
                    .GetValue(_translations, null) as List<TranslationItemWithIndex>;

                if (type == null)
                    continue;

                foreach (var item in type)
                {
                    item.Text = FixTranslation(item.Text);
                }
            }

            _translations.SortIndexItems();
            _translations.ToJsonFile(_translationsFile);

            ShowMessage($"{_translationsFile} updated");

        }

        /// <summary>
        /// TODO: Refactor this urgently, use a HTML parser
        /// </summary>
        /// <param name="text"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public string TranslateText(string text, string origin, string destination)
        {
            var translator = new GoogleTranslator();

            Language from = GoogleTranslator.GetLanguageByISO(origin);
            Language to = GoogleTranslator.GetLanguageByISO(destination);

            TranslationResult result = translator.TranslateLiteAsync(text, from, to).GetAwaiter().GetResult();

            var translated = FixTranslation(result.MergedTranslation);

            return translated;
        }

        private string FixTranslation(string translation)
        {
            string retVal = translation;

            foreach(var item in _config.TranslationFixes)
            {
                foreach (var fix in item.Find)
                    retVal = retVal.Replace(fix, item.ReplaceWith);
            }

            return retVal;
        }

        private string GetIsoFromLanguage(string language)
            => _config.Languages.FirstOrDefault(x => x.Key == language).Value;

        private void ShowMessage(string message) => ConsoleMessage?.Invoke(message);

        private string GetConfigFile() => Path.Combine(Directory.GetCurrentDirectory(), ConfigFile);

        private TranslationManagerConfig GetConfig() => GetConfigFile().FromJson<TranslationManagerConfig>();
    }
}
