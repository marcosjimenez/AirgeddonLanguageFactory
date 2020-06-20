using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GoogleTranslateFreeApi;
using Airgeddon.LanguageFactory.Models;

namespace Airgeddon.LanguageFactory
{
    public class TranslationManager
    {

        private TranslationFile _translations;

        private readonly Dictionary<string, string> _languages;

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
            _translations = JsonConvert.DeserializeObject<TranslationFile>(text);
        }

        public void AddTranslation(string referenceLanguage, string newLanguage, string isoCode)
        {

            referenceLanguage = referenceLanguage.ToUpper();
            newLanguage = newLanguage.ToUpper();

            if (!_translations.unknown_chipset.Any(x => x.Language.ToUpper() == referenceLanguage))
                throw new Exception($"Invalid reference language {referenceLanguage}");

            if (_translations.unknown_chipset.Any(x => x.Language.ToUpper() == newLanguage))
                throw new Exception($"New language {referenceLanguage} already exists");

            var isoReference = GetIsoFromLanguage(referenceLanguage);

            var item = _translations.unknown_chipset.FirstOrDefault(x => x.Language.ToUpper() == referenceLanguage);
            _translations.unknown_chipset.Add(new TranslationItem
            {
                Language = newLanguage,
                Text = TranslateText(item.Text, isoReference, isoCode)
            });
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

            string retVal = string.Empty;
            try
            {
                TranslationResult result = translator.TranslateLiteAsync(text, from, to).GetAwaiter().GetResult();
                retVal = result.MergedTranslation;
            }
            catch(Exception ex)
            {
                var a = ex.Message;
            }

            return retVal;

        }


        private string GetIsoFromLanguage(string language)
            => _languages.FirstOrDefault(x => x.Key == language).Value;

    }
}
