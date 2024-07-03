using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Contracts
{
    public class AuctionFinished
    {
        public bool ItemSold { get; set; }
        public string AuctionId { get; set; }
        public string Winner { get; set; }
        public string Seller { get; set; }
        public int? Amount { get; set; }
    }
}
