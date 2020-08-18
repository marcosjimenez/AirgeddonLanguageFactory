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

    public class TranslationManager
    {
        private const string ConfigFile = "config.json";
        private const string SH_File_Template = "template.sh";
        private const string PendingOfTranslation = "\\${pending_of_translation} ";

        private TranslationFile _translations;
        private TranslationManagerConfig _config;
        private string _destinationFile = string.Empty;

        public Action<string> ConsoleMessage = null;

        public void Initialize(string inputFile)
        {
            _config = GetConfig();

            _translations = inputFile.FromJson<TranslationFile>();

            _destinationFile = inputFile;
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
                _translations.ToJsonFile(_destinationFile);
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
                        Text = translatedText
                    });
            }

        }

        public bool GenerateScript(string destinationFilename, string version)
        {
            bool retVal = true;

            if (!File.Exists(SH_File_Template))
                throw new FileNotFoundException(SH_File_Template);

            var template = File.ReadAllText(SH_File_Template);
            var engine = new Template(template, '¬', '¬');

            //Prepare translations
            //TODO: Add parameters on related command on sorting, and filtering.
            _translations.aircrack_texts = _translations.aircrack_texts.OrderBy(x => x.Index).ToList();
            _translations.arr = _translations.arr.OrderBy(x => x.Index).ToList();
            _translations.asleap_texts = _translations.asleap_texts.OrderBy(x => x.Index).ToList();
            _translations.et_misc_texts = _translations.et_misc_texts.OrderBy(x => x.Index).ToList();
            _translations.footer_texts = _translations.footer_texts.OrderBy(x => x.Index).ToList();
            _translations.hashcat_texts = _translations.hashcat_texts.OrderBy(x => x.Index).ToList();
            _translations.jtr_texts = _translations.jtr_texts.OrderBy(x => x.Index).ToList();
            _translations.wps_texts = _translations.wps_texts.OrderBy(x => x.Index).ToList();

            engine.Add("Data", new
            {
                Version = version,
                Translations = _translations
            });

            var renderedText = engine.Render();
            File.WriteAllText(destinationFilename, renderedText);

            return retVal;
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
