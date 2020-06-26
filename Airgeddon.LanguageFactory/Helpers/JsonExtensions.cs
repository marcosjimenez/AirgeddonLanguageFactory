using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Airgeddon.LanguageFactory.Helpers
{
    public static class JsonExtensions
    {

        public static T FromJson<T>(this string source) where T : class
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var retVal = JsonSerializer.Deserialize<T>(File.ReadAllText(source));
            
            return retVal ?? default;
        }

        public static void ToJson<T>(this T source, string file) where T : class
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var text = JsonSerializer.Serialize(source, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            File.WriteAllText(file, text);
        }

    }
}
