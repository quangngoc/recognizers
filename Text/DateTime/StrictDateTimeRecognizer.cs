using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Recognizers.Text.DateTime.English;
using Microsoft.Recognizers.Text.DateTime.French;
using Microsoft.Recognizers.Text.DateTime.German;
using Microsoft.Recognizers.Text.DateTime.Italian;
using Microsoft.Recognizers.Text.DateTime.Spanish;
using System.Reflection;
using System.Text.RegularExpressions;
using English = Microsoft.Recognizers.Definitions.English;
using French = Microsoft.Recognizers.Definitions.French;
using German = Microsoft.Recognizers.Definitions.German;
using Italian = Microsoft.Recognizers.Definitions.Italian;
using Spanish = Microsoft.Recognizers.Definitions.Spanish;

namespace QuangNgoc.Recognizers.Text.DateTime
{
    public class StrictDateTimeRecognizer : DateTimeRecognizer
    {
        public StrictDateTimeRecognizer(string targetCulture, DateTimeOptions options = DateTimeOptions.None, bool lazyInitialization = false)
                : base(targetCulture, options, lazyInitialization)
        {
        }

        public StrictDateTimeRecognizer(string targetCulture, int options, bool lazyInitialization = false)
            : this(targetCulture, GetOptions(options), lazyInitialization)
        {
        }

        public StrictDateTimeRecognizer(DateTimeOptions options = DateTimeOptions.None, bool lazyInitialization = true)
            : this(null, options, lazyInitialization)
        {
        }

        public StrictDateTimeRecognizer(int options, bool lazyInitialization = true)
            : this(null, options, lazyInitialization)
        {
        }

        public new static List<ModelResult> RecognizeDateTime(string query, string culture, DateTimeOptions options = DateTimeOptions.None, System.DateTime? refTime = null, bool fallbackToDefaultCulture = true)
        {
            var recognizer = new StrictDateTimeRecognizer(options);
            var model = recognizer.GetModel<StrictDateTimeModel>(culture, fallbackToDefaultCulture);
            return model.Parse(query, refTime ?? System.DateTime.Now);
        }

        protected override void InitializeConfiguration()
        {
            RegisterModel<StrictDateTimeModel>(
                Culture.English,
                options => new StrictDateTimeModel(
                    new BaseMergedDateTimeParser(
                        GetEnglishParserConfiguration(Culture.English, options)),
                    new BaseMergedDateTimeExtractor(
                        new EnglishMergedExtractorConfiguration(new BaseDateTimeOptionsConfiguration(Culture.English, options, dmyDateFormat: false)))));

            RegisterModel<StrictDateTimeModel>(
                Culture.EnglishOthers,
                options => new StrictDateTimeModel(
                    new BaseMergedDateTimeParser(
                        GetEnglishParserConfiguration(Culture.EnglishOthers, options)),
                    new BaseMergedDateTimeExtractor(
                        new EnglishMergedExtractorConfiguration(new BaseDateTimeOptionsConfiguration(Culture.EnglishOthers, options, dmyDateFormat: true)))));

            RegisterModel<StrictDateTimeModel>(
                Culture.French,
                options => new StrictDateTimeModel(
                    new BaseMergedDateTimeParser(
                        GetFrenchParserConfiguration(options)),
                    new BaseMergedDateTimeExtractor(
                        new FrenchMergedExtractorConfiguration(new BaseDateTimeOptionsConfiguration(Culture.French, options)))));

            RegisterModel<StrictDateTimeModel>(
                Culture.German,
                options => new StrictDateTimeModel(
                    new BaseMergedDateTimeParser(
                        GetGermanParserConfiguration(options)),
                    new BaseMergedDateTimeExtractor(
                        new GermanMergedExtractorConfiguration(new BaseDateTimeOptionsConfiguration(Culture.German, options)))));

            RegisterModel<StrictDateTimeModel>(
                Culture.Italian,
                options => new StrictDateTimeModel(
                    new BaseMergedDateTimeParser(
                        GetItalianParserConfiguration(options)),
                    new BaseMergedDateTimeExtractor(
                        new ItalianMergedExtractorConfiguration(new BaseDateTimeOptionsConfiguration(Culture.Italian, options)))));

            RegisterModel<StrictDateTimeModel>(
                Culture.Spanish,
                options => new StrictDateTimeModel(
                    new BaseMergedDateTimeParser(
                        GetSpanishParserConfiguration(options)),
                    new BaseMergedDateTimeExtractor(
                        new SpanishMergedExtractorConfiguration(new BaseDateTimeOptionsConfiguration(Culture.Spanish, options)))));
        }

        private EnglishMergedParserConfiguration GetEnglishParserConfiguration(string culture, DateTimeOptions options)
        {
            bool enableDmy = culture == Culture.EnglishOthers;
            RegexOptions regexFlags = RegexOptions.Singleline | RegexOptions.ExplicitCapture;

            // 3-23-2017
            var dateRegex4 = new Regex(English.DateTimeDefinitions.DateExtractor4, regexFlags);

            // 23-3-2015
            var dateRegex5 = new Regex(English.DateTimeDefinitions.DateExtractor5, regexFlags);

            // on (Sunday,)? 1.3
            var dateRegex6 = new Regex(English.DateTimeDefinitions.DateExtractor6, regexFlags);

            // on (Sunday,)? 24-12
            var dateRegex8 = new Regex(English.DateTimeDefinitions.DateExtractor8, regexFlags);

            // "(Sunday,)? 7/23, 2018", year part is required
            var dateRegex7L = new Regex(English.DateTimeDefinitions.DateExtractor7L, regexFlags);

            // "(Sunday,)? 7/23", year part is not required
            var dateRegex7S = new Regex(English.DateTimeDefinitions.DateExtractor7S, regexFlags);

            // "(Sunday,)? 23/7, 2018", year part is required
            var dateRegex9L = new Regex(English.DateTimeDefinitions.DateExtractor9L, regexFlags);

            // "(Sunday,)? 23/7", year part is not required
            var dateRegex9S = new Regex(English.DateTimeDefinitions.DateExtractor9S, regexFlags);

            string[] regexStringsToRemove = enableDmy
                ? new[] { dateRegex4, dateRegex6, dateRegex7L, dateRegex7S }.Select(p => p.ToString()).ToArray()
                : new[] { dateRegex5, dateRegex8, dateRegex9L, dateRegex9S }.Select(p => p.ToString()).ToArray();

            return GetCustomParserConfiguration<EnglishMergedParserConfiguration, EnglishDateParserConfiguration>(culture, options, regexStringsToRemove, enableDmy);
        }

        private FrenchMergedParserConfiguration GetFrenchParserConfiguration(DateTimeOptions options)
        {
            RegexOptions regexFlags = RegexOptions.Singleline | RegexOptions.ExplicitCapture;

            // 3-23-2017
            var dateRegex4 = new Regex(French.DateTimeDefinitions.DateExtractor4, regexFlags);

            // on 1.3
            var dateRegex6 = new Regex(French.DateTimeDefinitions.DateExtractor6, regexFlags);

            // 7/23
            var dateRegex7 = new Regex(French.DateTimeDefinitions.DateExtractor7, regexFlags);

            string[] regexStringsToRemove = new[] { dateRegex4, dateRegex6, dateRegex7 }.Select(p => p.ToString()).ToArray();
            return GetCustomParserConfiguration<FrenchMergedParserConfiguration, FrenchDateParserConfiguration>(Culture.French, options, regexStringsToRemove);
        }

        private GermanMergedParserConfiguration GetGermanParserConfiguration(DateTimeOptions options)
        {
            RegexOptions regexFlags = RegexOptions.Singleline | RegexOptions.ExplicitCapture;

            // 3-23-2017
            var dateRegex4 = new Regex(German.DateTimeDefinitions.DateExtractor4, regexFlags);

            // 7/23
            var dateRegex7 = new Regex(German.DateTimeDefinitions.DateExtractor7, regexFlags);

            string[] regexStringsToRemove = new[] { dateRegex4, dateRegex7 }.Select(p => p.ToString()).ToArray();
            return GetCustomParserConfiguration<GermanMergedParserConfiguration, GermanDateParserConfiguration>(Culture.German, options, regexStringsToRemove);
        }

        private ItalianMergedParserConfiguration GetItalianParserConfiguration(DateTimeOptions options)
        {
            RegexOptions regexFlags = RegexOptions.Singleline | RegexOptions.ExplicitCapture;

            // 3-23-2017
            var dateRegex4 = new Regex(Italian.DateTimeDefinitions.DateExtractor4, regexFlags);

            // on 1.3
            var dateRegex6 = new Regex(Italian.DateTimeDefinitions.DateExtractor6, regexFlags);

            // 7/23
            var dateRegex7 = new Regex(Italian.DateTimeDefinitions.DateExtractor7, regexFlags);

            string[] regexStringsToRemove = new[] { dateRegex4, dateRegex6, dateRegex7 }.Select(p => p.ToString()).ToArray();
            return GetCustomParserConfiguration<ItalianMergedParserConfiguration, ItalianDateParserConfiguration>(Culture.Italian, options, regexStringsToRemove);
        }

        private SpanishMergedParserConfiguration GetSpanishParserConfiguration(DateTimeOptions options)
        {
            RegexOptions regexFlags = RegexOptions.Singleline | RegexOptions.ExplicitCapture;

            // 3-23-2017
            var dateRegex4 = new Regex(Spanish.DateTimeDefinitions.DateExtractor4, regexFlags);

            // on 1.3
            var dateRegex6 = new Regex(Spanish.DateTimeDefinitions.DateExtractor6, regexFlags);

            // 7/23
            var dateRegex7 = new Regex(Spanish.DateTimeDefinitions.DateExtractor7, regexFlags);

            string[] regexStringsToRemove = new[] { dateRegex4, dateRegex6, dateRegex7 }.Select(p => p.ToString()).ToArray();
            return GetCustomParserConfiguration<SpanishMergedParserConfiguration, SpanishDateParserConfiguration>(Culture.Spanish, options, regexStringsToRemove);
        }

        private TMerged GetCustomParserConfiguration<TMerged, TParser>(string culture, DateTimeOptions options, string[] regexStringsToRemove, bool enableDmy = false)
            where TMerged : IMergedParserConfiguration
            where TParser : IDateParserConfiguration
        {
            var dateTimeParser = (TMerged)Activator.CreateInstance(typeof(TMerged), new object[] { new BaseDateTimeOptionsConfiguration(culture, options, enableDmy) });
            var parserConfiguration = (TParser)Activator.CreateInstance(typeof(TParser), new object[] { dateTimeParser });

            var dateRegexList = parserConfiguration.DateRegexes.ToList();

            dateRegexList.RemoveAll(p => regexStringsToRemove.Contains(p.ToString()));

            typeof(TParser)
               .GetField("<DateRegexes>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
               .SetValue(parserConfiguration, dateRegexList);

            var dateParser = new BaseDateParser(parserConfiguration);
            typeof(TMerged)
                .GetProperty("DateParser", BindingFlags.Instance | BindingFlags.Public)
                .SetValue(dateTimeParser, dateParser);

            return dateTimeParser;
        }
    }
}