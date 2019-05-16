using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Order
    {
        public int PaymentTypeId { get; set; }
        public int CustomerId { get; set; }
        public List<Product> ProductList { get; set; } = new List<Product>();
        public List<Customer> CustomerList { get; set; } = new List<Customer>();
    }
}
