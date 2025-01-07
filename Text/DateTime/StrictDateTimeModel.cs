using Microsoft.Recognizers.Text.DateTime;

namespace QuangNgoc.Recognizers.Text.DateTime
{
    // Create a different DateTimeModel to use different ModelFactory cache in Recognizer
    public class StrictDateTimeModel : DateTimeModel
    {
        public StrictDateTimeModel(IDateTimeParser parser, IDateTimeExtractor extractor)
            : base(parser, extractor)
        {
        }
    }
}