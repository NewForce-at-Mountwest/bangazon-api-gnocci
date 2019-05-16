using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class PaymentType
    {
        public int AcctNumber { get; set; }
        public string Name { get; set; }
        public int CustomerId { get; set; }
    }
}
