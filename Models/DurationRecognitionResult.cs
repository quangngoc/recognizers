namespace QuangNgoc.Recognizers.Models
{
    public class DurationRecognitionResult
    {
        public long Duration { get; set; }  // Duration value in a standardized format
        public string TimexValue { get; set; }  // Associated Timex value
    }
}