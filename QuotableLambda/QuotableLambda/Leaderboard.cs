using System;
using System.Collections.Generic;
using System.Text;

namespace QuotableLambda
{
    public class Leaderboard
    {
        public int QuoteCount { get; set; }
        public Tuple<string, int>[] MostQuoted { get; set; }
        public Tuple<string, int>[] MostReported { get; set; }
        public Tuple<string, int>[] MostNarcissistic { get; set; }
    }
}
