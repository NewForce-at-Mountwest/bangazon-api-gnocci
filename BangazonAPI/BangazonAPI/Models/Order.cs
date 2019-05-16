using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{

    //Order Class : Two FK (PaymentType/CustomerId) Two List for Production and Customer 
    public class Order
    {
        public int Id { get; set; }
        public int PaymentTypeId { get; set; }
        public int CustomerId { get; set; }
        public List<Product> ProductList { get; set; } = new List<Product>();
        public List<Customer> CustomerList { get; set; } = new List<Customer>();
    }
}
