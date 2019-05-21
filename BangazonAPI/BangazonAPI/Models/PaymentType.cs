using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    //Class PaymentType: AcctNumber/Name with a FK of CustomerId
    public class PaymentType
    {
        //[Key]
        public int Id { get; set; }
        //[Required]
        public int AcctNumber { get; set; }
        //[Required]
        public string Name { get; set; }
        //[Required]
        public int CustomerId { get; set; }
    }
}
