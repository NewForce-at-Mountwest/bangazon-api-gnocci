using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;

namespace CustomerResource.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomerController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //Making an empty string "command" to build up for different query strings
                    string command = "";
                    //Strings to show customer information
                    string customerColumn = @"SELECT c.Id AS 'Customer Id',
                    c.FirstName AS 'Customer First Name',
                    c.LastName AS 'Customer Last Name'";
                    string customerTable = "FROM Customer c";

                    //Making String Query to show customer product type if they ask for it (?include=products)
                    if (include == "products")
                    {
                        string productColumn = @",p.Id AS 'Product Id',
                                                  p.Price AS 'Product Price',
                                                  p.Title AS 'Product Title',
                                                  p.[Description] AS 'Product Description',
                                                  p.Quantity AS 'Product Quantity'";
                        string productTable = @"JOIN Product p ON c.Id = p.CustomerId";
                        //Making command = the query strings 
                        command = $@"{customerColumn}
                                     {productColumn}
                                     {customerTable}
                                      {productTable}";
                    }
                    else
                    {
                        command = $@"{customerColumn}
                                      {customerTable}";
                    }
                    if (include == "payments")
                    {
                        string paymentColumn = @",pm.Id AS 'Payment Id',
                                                  pm.Name AS 'Payment Name',
                                                  pm.AcctNumber AS 'Payment Account Number'";
                        string paymentTable = @"Join PaymentType pm on c.Id = pm.CustomerId";
                        //Adding the strings together to show customer and payment
                        command = $@"{customerColumn}
                                     {paymentColumn}
                                     {customerTable}
                                     {paymentTable}";

                    }

                    //Making String Query to show customer payment type if they ask for it (?include=payments)
                    //else
                    //{
                    //    //Else user does not include payments/product it will make command = just customer info
                    //    command = $@"{customerTable}
                    //                 {customerTable}";
                    //}


                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())
                    {
                        Customer attempt = new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Customer Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("Customer First Name")),
                            LastName = reader.GetString(reader.GetOrdinal("Customer Last Name"))
                        };
                        
                        if(include == "products")
                        {
                            Product currentProduct = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Product Id")),
                                Price = reader.GetInt32(reader.GetOrdinal("Product Price")),
                                Title = reader.GetString(reader.GetOrdinal("Product Title")),
                                Description = reader.GetString(reader.GetOrdinal("Product Description")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Product Quantity"))
                            };

                            if(customers.Any(c => c.Id == attempt.Id))
                            {
                                Customer thisCustomer = customers.Where(c => c.Id == attempt.Id).FirstOrDefault();
                                thisCustomer.ProductList.Add(currentProduct);
                            }
                            else
                            {
                                attempt.ProductList.Add(currentProduct);
                                customers.Add(attempt);
                            }
                        }
                        if (include == "payments")
                        {
                            PaymentType currentPayment = new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Payment Id")),
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("Payment Account Number")),
                                Name = reader.GetString(reader.GetOrdinal("Payment Name")),
                            };

                            if (customers.Any(c => c.Id == attempt.Id))
                            {
                                Customer thisCustomer = customers.Where(c => c.Id == attempt.Id).FirstOrDefault();
                                thisCustomer.PaymentTypeList.Add(currentPayment);
                            }
                            else
                            {
                                attempt.PaymentTypeList.Add(currentPayment);
                                customers.Add(attempt);
                            }
                        }
                        else
                        {
                            customers.Add(attempt);
                        }
                    }
                    reader.Close();

                    return Ok(customers);
                }
            }
        }
        [HttpGet("{Id}", Name = "GetCustomer")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, FirstName, LastName
                        FROM Customer
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Customer Customer = null;

                    if (reader.Read())
                    {
                        Customer = new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))

                        };
                    }
                    reader.Close();

                    return Ok(Customer);
                }
            }
        }
    }
}