namespace Airgeddon.LanguageFactory
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using GoogleTranslateFreeApi;
    using Airgeddon.LanguageFactory.Models;
    using System.Text.Json;
    using System.Text;
    using Airgeddon.LanguageFactory.Helpers;

    public class TranslationManager
    {
        private const string ConfigFile = "config.json";
        private TranslationFile _translations;
        private TranslationManagerConfig _config;
        private string _destinationFile = string.Empty;

        public Action<string> ConsoleMessage = null;

        public TranslationManager()
        {



        }

        public void Initialize(string inputFile)
        {
            _config = Path.Combine(Directory.GetCurrentDirectory(), ConfigFile)
                .FromJson<TranslationManagerConfig>();

            _translations = inputFile.FromJson<TranslationFile>();

            _destinationFile = inputFile;
        }

        public List<string> AddTranslation(string referenceLanguage, string newLanguage, string isoCode)
        {
            var retVal = new List<string>();

            referenceLanguage = referenceLanguage.ToUpper();
            newLanguage = newLanguage.ToUpper();

            // Use unknown_chipset as index and language check for all arrays.
            if (!_translations.unknown_chipset.Any(x => x.Language.ToUpper() == referenceLanguage))
                throw new Exception($"Invalid reference language {referenceLanguage}");

            if (_translations.unknown_chipset.Any(x => x.Language.ToUpper() == newLanguage))
                throw new Exception($"New language {newLanguage} already exists");

            var isoReference = GetIsoFromLanguage(referenceLanguage);

            var noIndexText = GetNoIndexWords();
            var translatedIndexText = TranslateText(noIndexText, isoReference, isoCode);
            var translatedIndexItems = translatedIndexText.Split(Environment.NewLine);

            for(int i = 0; i < TranslationConstants.NoIndexWords.Length; i++)
            {
                AddTranslatedItemNoIndex(TranslationConstants.NoIndexWords[i], translatedIndexItems[i]);
            }

            // Index words
            foreach (var item in TranslationConstants.IndexWords)
            {
                ShowMessage($"Translating {item}");
                AddTranslatedItem(item);
            }

            void AddTranslatedItem(string item) // Local func
            {
                var type = _translations.GetType().GetProperty(item).GetValue(_translations, null) as List<TranslationItemWithIndex>;
                var containedItems = type.Where(x => x.Language.ToUpper() == referenceLanguage).ToList();
                if (containedItems == null)
                    retVal.Add($"Cannot find {item} on json file");
                else
                {
                    foreach(var contained in containedItems)
                        type.Add(new TranslationItemWithIndex
                        {
                            Language = newLanguage,
                            Text = TranslateText(contained.Text, isoReference, isoCode),
                            Index = contained.Index
                        });
                }
            }

            try
            {
                _translations.ToJson(_destinationFile);
            }
            catch(Exception ex)
            {
                retVal.Add(ex.Message);
            }

            return retVal;

            // Local functions

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
            return result.MergedTranslation;
        }

        private string GetIsoFromLanguage(string language)
            => _config.Languages.FirstOrDefault(x => x.Key == language).Value;

        private void ShowMessage(string message) => ConsoleMessage?.Invoke(message);

    }
}
