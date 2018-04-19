using System;
using System.Collections.Generic;
using System.Text;

namespace QuotableLambda
{
    public class Quote
    {
        public string Quotee { get; set; }
        public string QuoteText { get; set; }
        public string AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }

        public override string ToString()
        {
            return $"{QuoteText} - {Quotee.TrimStart('@')}";
        }
    }
}
