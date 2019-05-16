using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{

    //Customer Class : Get/Set Firstname/Lastname and added two list for production and PaymentType
    public class Customer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<Product> ProductList { get; set; } = new List<Product>();
        public List<PaymentType> PaymentTypeList { get; set; } = new List<PaymentType>();
    }
}
