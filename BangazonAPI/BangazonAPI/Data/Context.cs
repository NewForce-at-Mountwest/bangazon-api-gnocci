using Microsoft.EntityFrameworkCore;
using BangazonAPI.Models;

namespace BangazonAPI.Data
{
    //Context --> DbContext; DbSet(s) Test Double:
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }

        //Tables to be Inserted in Database:
        public DbSet<Computer> Computer { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<PaymentType> PaymentType { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductType> ProductType { get; set; }
    }
}
