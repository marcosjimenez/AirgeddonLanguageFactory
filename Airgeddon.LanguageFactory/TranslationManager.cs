namespace Airgeddon.LanguageFactory
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using GoogleTranslateFreeApi;
    using Airgeddon.LanguageFactory.Models;
    using System.Text.Json;

    public class TranslationManager
    {

        private TranslationFile _translations;
        private string _destinationFile = string.Empty;
        private readonly Dictionary<string, string> _languages;

        public Action<string> ConsoleMessage = null;

        public TranslationManager()
        {
            _languages = new Dictionary<string, string>();
            _languages.Add("ENGLISH", "en");
            _languages.Add("SPANISH", "es");
            _languages.Add("FRENCH","fr");
            _languages.Add("CATALAN","ca");
            _languages.Add("PORTUGUESE","pt");
            _languages.Add("RUSSIAN","ru");
            _languages.Add("GREEK","el");
            _languages.Add("ITALIAN","it");
            _languages.Add("POLISH","pl");
            _languages.Add("GERMAN","de");
            _languages.Add("TURKISH","tr");
        }

        public void Initialize(string inputFile)
        {
            var text = File.ReadAllText(inputFile);
            _translations = JsonSerializer.Deserialize<TranslationFile>(text);
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

            foreach(var item in TranslationConstants.NoIndexWords)
            {
                ShowMessage($"Translating {item}");
                AddTranslatedItemNoIndex(item);
            }

            foreach(var item in TranslationConstants.IndexWords)
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

            void AddTranslatedItemNoIndex(string item) // Local func
            {
                var type = _translations.GetType().GetProperty(item).GetValue(_translations, null) as List<TranslationItem>;
                var containedItem = type.FirstOrDefault(x => x.Language.ToUpper() == referenceLanguage);
                if (containedItem == null)
                    retVal.Add($"Cannot find {item} on json file");
                else
                    type.Add(new TranslationItem
                    {
                        Language = newLanguage,
                        Text = TranslateText(containedItem.Text, isoReference, isoCode)
                    });
            }

            try
            { 
                var text = JsonSerializer.Serialize(_translations, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                File.WriteAllText(_destinationFile, text);
            }
            catch(Exception ex)
            {
                retVal.Add(ex.Message);
            }

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
            return result.MergedTranslation;
        }

        private string GetIsoFromLanguage(string language)
            => _languages.FirstOrDefault(x => x.Key == language).Value;

        private void ShowMessage(string message) => ConsoleMessage?.Invoke(message);

    }
}
