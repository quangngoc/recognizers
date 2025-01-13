using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Recognizers.Text.Number;
using Microsoft.Recognizers.Text.NumberWithUnit;
using QuangNgoc.Recognizers.Contracts;
using QuangNgoc.Recognizers.Text.DateTime;
using System.Globalization;

namespace QuangNgoc.Recognizers.Services
{
    public class TextRecognizerService : ITextRecognizerService
    {
        public static readonly string[] SUPPORTED_CULTURES = new string[] { Culture.English, Culture.EnglishOthers, Culture.French, Culture.German };

        private readonly ILogger _logger;

        public TextRecognizerService(ILogger<ITextRecognizerService> logger)
        {
            this._logger = logger;
        }

        public static string MapToNearestCulture(string culture) => Culture.MapToNearestLanguage(culture);

        /// <inheritdoc/>
        public IDictionary<string, string> RecognizeDateRange(string textValue)
        {
            var reconizedDateRanges = new Dictionary<string, string>();
            foreach (var culture in SUPPORTED_CULTURES)
            {
                var reconizedDateRange = this.RecognizeDateRange(textValue, culture);
                if (!string.IsNullOrEmpty(reconizedDateRange)) reconizedDateRanges.Add(culture, reconizedDateRange);
            }
            return reconizedDateRanges;
        }

        /// <inheritdoc/>
        public string RecognizeDateRange(string textValue, string culture)
        {
            List<ModelResult> recognizedResult = StrictDateTimeRecognizer.RecognizeDateTime(textValue, culture);

            if (recognizedResult == null) return null;

            var acceptedDateTypes = new string[] { "datetimeV2.daterange", "datetimeV2.datetimerange" };
            var foundTimexs = recognizedResult
                .Where(p => acceptedDateTypes.Contains(p.TypeName) && p.Resolution?.ContainsKey("values") == true)
                .SelectMany(p => p.Resolution["values"] as List<Dictionary<string, string>>)
                .Where(p => p?.ContainsKey("timex") == true)
                .Select(p => p["timex"]).Distinct()
                .Select(p => new TimexProperty(p)).ToList();
            if (foundTimexs.Count == 1)
            {
                return foundTimexs[0].TimexValue;
            }
            return null;
        }

        /// <inheritdoc/>
        public IDictionary<string, DateTime> RecognizeDateTime(string textValue)
        {
            var reconizedDates = new Dictionary<string, DateTime>();
            foreach (var culture in SUPPORTED_CULTURES)
            {
                var reconizedDate = this.RecognizeDateTime(textValue, culture);
                if (reconizedDate != null) reconizedDates.Add(culture, reconizedDate.Value);
            }
            return reconizedDates;
        }

        /// <inheritdoc/>
        public DateTime? RecognizeDateTime(string textValue, string culture)
        {
            if (string.IsNullOrEmpty(textValue)) return null;
            if (culture == Culture.French && textValue.Contains(" mil ")) textValue = textValue.Replace(" mil ", " mille ");
            List<ModelResult> recognizedResult = StrictDateTimeRecognizer.RecognizeDateTime(textValue, culture, fallbackToDefaultCulture: false);
            if (recognizedResult == null) return null;

            var acceptedDateTypes = new string[] { "datetimeV2.date", "datetimeV2.datetime" };
            var resolutions = recognizedResult
                .Where(p => acceptedDateTypes.Contains(p.TypeName) && p.Resolution?.ContainsKey("values") == true)
                .SelectMany(p => p.Resolution["values"] as List<Dictionary<string, string>>);
            var foundTimexs = resolutions.Where(p => p?.ContainsKey("timex") == true)
                .Select(p => p["timex"]).Distinct().Select(p => new TimexProperty(p)).ToList();

            // Search only timex values with year & month & day
            var foundDates = foundTimexs.Where(p => p.Year != null && p.Month != null && p.DayOfMonth != null).ToList();

            if (foundDates.Count == 1)
            {
                var timexResults = TimexResolver.Resolve(new[] { foundDates[0].TimexValue }).Values;
                if (timexResults.Count == 1)
                {
                    // Timex value is formated in ISO 8601 standard
                    return DateTime.Parse(timexResults[0].Value, CultureInfo.InvariantCulture, DateTimeStyles.None);
                }
                else if (timexResults.Count > 1)
                {
                    this._logger.LogError($"{nameof(TextRecognizerService)}: Multiple resolutions found for {textValue} in '{culture}' culture, timex value = '{foundDates.First().TimexValue}'");
                }
            }
            else if (foundDates.Count > 1)
            {
                this._logger.LogError($"{nameof(TextRecognizerService)}: Multiple timex values found for {textValue} in '{culture}' culture");
            }
            else
            {
                var recognizedValue = resolutions.Where(p => p?.ContainsKey("value") == true)
                    .Select(p => p["value"].ToString()).Distinct().LastOrDefault();
                if (recognizedValue != null) return this.RecognizeDateTime(recognizedValue, culture);
            }
            return null;
        }

        /// <inheritdoc/>
        public decimal? RecognizeNumber(string textValue, string culture)
        {
            List<ModelResult> recognizedResult = NumberRecognizer.RecognizeNumber(textValue, culture);
            if (recognizedResult == null) return null;

            var acceptedNumberTypes = new string[] { "integer", "decimal", "power", "fraction" };
            List<decimal> foundNumbers = recognizedResult.Where(p => p.TypeName == "number"
                    && (!p.Resolution.ContainsKey("subtype") || acceptedNumberTypes.Contains(p.Resolution["subtype"])))
                .Select(p =>
                {
                    string value = p.Resolution["value"].ToString();
                    try
                    {
                        var cultureInfo = new CultureInfo(culture);
                        return decimal.Parse(value, cultureInfo);
                    }
                    catch (FormatException ex)
                    {
                        this._logger.LogError($"Error parsing number '{value}' with culture '{culture}': {ex.Message}");
                        return (decimal?)null;
                    }
                })
                .Where(p => p.HasValue).Select(p => p.Value).ToList();
            if (foundNumbers.Count == 0) return null;
            if (foundNumbers.Count == 1) return foundNumbers[0];
            else
            {
                this._logger.LogError($"{nameof(TextRecognizerService)}: Multiple number found for {textValue} in '{culture}' culture");
                return null;
            }
        }

        /// <inheritdoc/>
        public (string Text, decimal? Number, string Unit)? RecognizeAmountAndCurrency(string textValue, string culture)
        {
            List<ModelResult> recognizedResult = NumberWithUnitRecognizer.RecognizeCurrency(textValue, culture);
            var defaultCurrenies = new Dictionary<string, string>
            {
                { "Dollar", "USD" },
                { "Pound", "GBP" }
            };
            var foundCurrencies = recognizedResult.Where(p => p.TypeName == "currency")
                .Select(p =>
                {
                    var value = p.Resolution["value"]?.ToString();
                    decimal? number = decimal.TryParse(value, out decimal val) ? val : null;
                    var unit = p.Resolution.ContainsKey("isoCurrency")
                        ? p.Resolution["isoCurrency"].ToString()
                        : (p.Resolution.ContainsKey("unit") && defaultCurrenies.ContainsKey(p.Resolution["unit"]?.ToString())
                            ? defaultCurrenies[p.Resolution["unit"].ToString()] : null);
                    return (p.Text, Number: number, Unit: unit);
                })
                .ToList();
            if (foundCurrencies.Count == 0) return null;
            if (foundCurrencies.Count == 1) return foundCurrencies[0];
            else
            {
                this._logger.LogError($"{nameof(TextRecognizerService)}: Multiple currencies found for {textValue} in '{culture}' culture");
                return null;
            }
        }

        /// <inheritdoc/>
        public string RecognizeCurrency(string textValue, string culture)
        {
            List<ModelResult> recognizedResult = NumberWithUnitRecognizer.RecognizeCurrency(textValue, culture);
            var foundCurrencies = recognizedResult.Where(p => p.TypeName == "currency").Select(p => p.Resolution)
                .Select(p => p.ContainsKey("isoCurrency") ? p["isoCurrency"].ToString() : (p.ContainsKey("unit") && p["unit"].ToString() == "Dollar" ? "USD" : null))
                .ToList();
            if (foundCurrencies.Count == 0) return null;
            if (foundCurrencies.Count == 1) return foundCurrencies[0];
            else
            {
                this._logger.LogError($"{nameof(TextRecognizerService)}: Multiple currencies found for {textValue} in '{culture}' culture");
                return null;
            }
        }

        /// <inheritdoc/>
        public long? RecognizeDuration(string textValue, out string timexValue)
        {
            timexValue = null;
            for (int i = 0; i < SUPPORTED_CULTURES.Length; i++)
            {
                var reconizedDuration = this.RecognizeDuration(textValue, SUPPORTED_CULTURES[i], out timexValue);
                if (reconizedDuration != null) return reconizedDuration;
            }
            return null;
        }

        /// <inheritdoc/>
        public long? RecognizeDuration(string textValue, string[] cultures, out string timexValue)
        {
            string[] cultureList = cultures?.Length > 0
                ? cultures.Intersect(SUPPORTED_CULTURES).ToArray()
                : SUPPORTED_CULTURES;
            if (cultureList.Length == 0) cultureList = SUPPORTED_CULTURES;

            timexValue = null;
            for (int i = 0; i < cultureList.Length; i++)
            {
                var reconizedDuration = this.RecognizeDuration(textValue, cultureList[i], out timexValue);
                if (reconizedDuration != null) return reconizedDuration;
            }
            return null;
        }

        /// <inheritdoc/>
        public long? RecognizeDuration(string textValue, string culture, out string timexValue)
        {
            List<ModelResult> recognizedResult = StrictDateTimeRecognizer.RecognizeDateTime(textValue, culture);
            timexValue = null;
            if (recognizedResult == null) return null;

            var acceptedDateTypes = new string[] { "datetimeV2.duration" };
            var foundDurations = recognizedResult.Where(p => acceptedDateTypes.Contains(p.TypeName)).ToList();
            if (foundDurations.Count == 0) return null;
            if (foundDurations.Count == 1 && foundDurations[0].Resolution.ContainsKey("values"))
            {
                long durationInSecond;
                var resolutions = foundDurations[0].Resolution["values"] as List<Dictionary<string, string>>;
                if (resolutions != null && resolutions.Count == 1 && resolutions[0].ContainsKey("value"))
                {
                    var isRecognized = long.TryParse(
                        resolutions[0]["value"],
                        out durationInSecond);
                    if (isRecognized)
                    {
                        timexValue = resolutions[0]["timex"];
                        return durationInSecond;
                    }
                }
                else if (resolutions.Count > 1)
                {
                    this._logger.LogError($"{nameof(TextRecognizerService)}: Multiple durations found for {textValue} in '{culture}' culture");
                }
            }
            else if (foundDurations.Count == 1)
            {
                this._logger.LogError($"{nameof(TextRecognizerService)}: Multiple durations found for {textValue} in '{culture}' culture");
            }
            return null;
        }

        /// <inheritdoc/>
        public T? RecognizeNumber<T>(string textValue) where T : struct
        {
            for (int i = 0; i < SUPPORTED_CULTURES.Length; i++)
            {
                var reconizedNumber = this.RecognizeNumber<T>(textValue, SUPPORTED_CULTURES[i]);
                if (reconizedNumber != null) return reconizedNumber;
            }
            return null;
        }

        /// <inheritdoc/>
        public T? RecognizeNumber<T>(string textValue, string culture) where T : struct
        {
            if (typeof(T) != typeof(int) && typeof(T) != typeof(decimal))
            {
                throw new NotSupportedException($"Type {nameof(T)} is not supported");
            }

            List<ModelResult> recognizedResult = NumberRecognizer.RecognizeNumber(textValue, culture);
            if (recognizedResult == null) return null;

            var foundNumber = recognizedResult.Find(p => p.TypeName == "number");
            var supportedSubTypes = new string[] { "integer", "decimal" };
            if (foundNumber?.Resolution.ContainsKey("subtype") == true
                && supportedSubTypes.Contains(foundNumber.Resolution["subtype"]))
            {
                return (T)Convert.ChangeType(foundNumber.Resolution["value"], typeof(T));
            }
            return null;
        }
    }
}