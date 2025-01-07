using Microsoft.AspNetCore.Mvc;
using Microsoft.Recognizers.Text;
using QuangNgoc.Recognizers.Contracts;
using QuangNgoc.Recognizers.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace QuangNgoc.Recognizers.Controllers
{
    [ApiController]
    [Route("api/recognizer")]
    public class TextRecognizerController : ControllerBase
    {
        private readonly ILogger<TextRecognizerController> _logger;
        private readonly ITextRecognizerService _textRecognizerService;

        public TextRecognizerController(ILogger<TextRecognizerController> logger, ITextRecognizerService textRecognizerService)
        {
            _logger = logger;
            _textRecognizerService = textRecognizerService;
        }

        [HttpPost]
        [Route("datetime")]
        [SwaggerOperation(
            Summary = "Recognizes date and time from text",
            Description = "Analyzes the input text for date and time patterns and returns recognized values based on the specified culture.",
            Tags = new[] { "DateTime" })]
        public IEnumerable<DateTime?> RecognizeDateTime(
            [FromBody][SwaggerParameter(Description = "Array of text inputs to analyze for date and time values.")] string[] texts,
            [FromQuery][SwaggerParameter(Description = "Culture code to use for date and time recognition, e.g., 'en-us' for English or 'fr-fr' for French.")] string culture = Culture.French)
        {
            return texts.Select(t => _textRecognizerService.RecognizeDateTime(t, culture)).ToList();
        }

        [HttpPost]
        [Route("daterange")]
        [SwaggerOperation(
            Summary = "Recognizes date range from text",
            Description = "Analyzes the input text for date range patterns and returns recognized timex values based on the specified culture.",
            Tags = new[] { "DateTime" })]
        public IEnumerable<string> RecognizeDateRange(
            [FromBody][SwaggerParameter(Description = "Array of text inputs to analyze for date and time values.")] string[] texts,
            [FromQuery][SwaggerParameter(Description = "Culture code to use for date and time recognition, e.g., 'en-us' for English or 'fr-fr' for French.")] string culture = Culture.French)
        {
            return texts.Select(t => _textRecognizerService.RecognizeDateRange(t, culture)).ToList();
        }

        [HttpPost]
        [Route("duration")]
        [SwaggerOperation(
            Summary = "Recognizes duration from text",
            Description = "Analyzes input text to identify a duration (e.g., '5 hours', '2 days') and returns the duration in a standardized format along with an optional 'timexValue'.",
            Tags = new[] { "DateTime" })]
        [SwaggerResponse(200, "A list of recognized durations with timex values.", typeof(IEnumerable<DurationRecognitionResult>))]
        public IEnumerable<DurationRecognitionResult?> RecognizeDuration(
            [FromBody][SwaggerParameter(Description = "Array of text inputs containing durations (e.g., '5 hours', '2 days').")] string[] texts,
            [FromQuery][SwaggerParameter(Description = "Culture code to use for duration recognition, e.g., 'en-us' for English or 'fr-fr' for French. Optional, defaults to 'en'.")] string culture = Culture.French)
        {
            var results = new List<DurationRecognitionResult?>();
            foreach (var text in texts)
            {
                var duration = _textRecognizerService.RecognizeDuration(text, culture, out var timexValue);
                results.Add(duration.HasValue ? new DurationRecognitionResult
                {
                    Duration = duration.Value,
                    TimexValue = timexValue
                } : null);
            }
            return results;
        }

        [HttpPost]
        [Route("currency")]
        [SwaggerOperation(
        Summary = "Recognizes currency values from text",
        Description = "Analyzes the input text for currency values and returns recognized amounts based on the specified culture.",
        Tags = new[] { "Amount & Currency" })]
        public IEnumerable<string> RecognizeCurrency(
        [FromBody][SwaggerParameter(Description = "Array of text inputs to analyze for currency values.")] string[] texts,
        [FromQuery][SwaggerParameter(Description = "Culture code to use for currency recognition, e.g., 'en-us' for English or 'fr-fr' for French.")] string culture = Culture.French)
        {
            return texts.Select(t => _textRecognizerService.RecognizeCurrency(t, culture)).ToList();
        }

        [HttpPost]
        [Route("amount-and-currency")]
        [SwaggerOperation(
            Summary = "Recognizes amount and currency from text",
            Description = "Analyzes the input text to identify amounts and associated currency units, returning the recognized text, numeric amount, and currency unit.",
            Tags = new[] { "Amount & Currency" })]
        [SwaggerResponse(200, "A list of recognized amounts and currencies.", typeof(IEnumerable<AmountRecognitionResult?>))]
        public IEnumerable<AmountRecognitionResult?> RecognizeAmountAndCurrency(
            [FromBody][SwaggerParameter(Description = "Array of text inputs to analyze for amounts and currency values.")] string[] texts,
            [FromQuery][SwaggerParameter(Description = "Culture code to use for amount and currency recognition, e.g., 'en-us' for English or 'fr-fr' for French.")] string culture = Culture.French)
        {
            return texts.Select(t =>
            {
                var result = _textRecognizerService.RecognizeAmountAndCurrency(t, culture);
                return result.HasValue ? new AmountRecognitionResult
                {
                    Text = result.Value.Text,
                    Number = result.Value.Number,
                    Unit = result.Value.Unit
                } : null;
            }).ToList();
        }

        [HttpPost]
        [Route("number")]
        [SwaggerOperation(
            Summary = "Recognizes integer, decimal, power, and fraction values from text",
            Description = "Analyzes input text to identify a variety of numerical formats, including integers, decimals, powers (e.g., 10e3, 10^2), and fractions (e.g., 1/2), returning a list of recognized numbers based on the specified culture.",
            Tags = new[] { "Number" })]
        [SwaggerResponse(200, "A list of recognized numbers (integer, decimal, power, fraction).", typeof(IEnumerable<decimal?>))]
        public IEnumerable<decimal?> RecognizeNumber(
            [FromBody][SwaggerParameter(Description = "Array of text inputs containing numbers in various formats (integer, decimal, power, fraction). Examples: '100', '12.34', '10e3', '1/2'.")] string[] texts,
            [FromQuery][SwaggerParameter(Description = "Culture code to use for number recognition, e.g., 'en-us' for English or 'fr-fr' for French. Optional, defaults to 'en'.")] string culture = Culture.French)
        {
            return texts.Select(t => _textRecognizerService.RecognizeNumber(t, culture)).ToList();
        }
    }
}