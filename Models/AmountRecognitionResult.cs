namespace QuangNgoc.Recognizers.Models
{
    public class AmountRecognitionResult
    {
        public string Text { get; set; }
        public decimal? Number { get; set; }
        public string Unit { get; set; }
    }
}