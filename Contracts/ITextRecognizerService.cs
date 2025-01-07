namespace QuangNgoc.Recognizers.Contracts
{
    public interface ITextRecognizerService
    {
        /// <summary>
        /// Return duration (in seconds)
        /// </summary>
        /// <param name="textValue">text string to recognize duration</param>
        /// <returns></returns>
        long? RecognizeDuration(string textValue, out string timexValue);

        /// <summary>
        /// Return duration (in seconds)
        /// </summary>
        /// <param name="textValue">text string to recognize duration</param>
        /// <param name="cultures">culture array to recognize duration</param>
        /// <returns></returns>
        long? RecognizeDuration(string textValue, string[] cultures, out string timexValue);

        /// <summary>
        /// Return duration (in seconds)
        /// </summary>
        /// <param name="textValue">text string to recognize duration</param>
        /// <param name="culture">culture to recognize duration</param>
        /// <returns></returns>
        long? RecognizeDuration(string textValue, string culture, out string timexValue);

        /// <summary>
        /// Return the first number recognized in textValue
        /// </summary>
        /// <typeparam name="T">only int and decimal are supported</typeparam>
        /// <param name="textValue">text string to recognize number</param>
        /// <returns></returns>
        T? RecognizeNumber<T>(string textValue) where T : struct;

        /// <summary>
        /// Return the first number recognized in textValue
        /// </summary>
        /// <typeparam name="T">only int and decimal are supported</typeparam>
        /// <param name="textValue">text string to recognize number</param>
        /// <param name="culture">culture to recognize number</param>
        /// <returns></returns>
        T? RecognizeNumber<T>(string textValue, string culture) where T : struct;

        /// <summary>
        /// Returns the dictionary of culture and date time recognized in textValue
        /// </summary>
        /// <param name="textValue">text string to recognize date time value</param>
        /// <returns></returns>
        IDictionary<string, DateTime> RecognizeDateTime(string textValue);

        /// <summary>
        /// Returns the date time recognized in textValue
        /// </summary>
        /// <param name="textValue">text string to recognize date time value</param>
        /// <param name="culture">culture to recognize date value</param>
        /// <returns></returns>
        DateTime? RecognizeDateTime(string textValue, string culture);

        /// <summary>
        /// Recognizes the number.
        /// </summary>
        /// <param name="textValue">The text value.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        decimal? RecognizeNumber(string textValue, string culture);

        /// <summary>
        /// Recognizes the currency.
        /// </summary>
        /// <param name="textValue">The text value.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        string RecognizeCurrency(string textValue, string culture);

        /// <summary>
        /// Return timex value of date range
        /// </summary>
        /// <param name="textValue">text string to recognize date range value</param>
        /// <param name="culture">culture to recognize date range value</param>
        /// <returns></returns>
        string RecognizeDateRange(string textValue, string culture);

        /// <summary>
        /// Return the dictionary of culture and timex value of date range
        /// </summary>
        /// <param name="textValue">text string to recognize date range value</param>
        /// <returns></returns>
        IDictionary<string, string> RecognizeDateRange(string textValue);

        /// <summary>
        /// Recognizes the amount and currency.
        /// </summary>
        /// <param name="textValue">The text value.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        (string Text, decimal? Number, string Unit)? RecognizeAmountAndCurrency(string textValue, string culture);
    }
}