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


        //Get method with two strings, 'include', 'q'
        [HttpGet]
        public async Task<IActionResult> Get(string include, string q)
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

                    //Making Query string to show customer product type if they ask for it (?include=products)
                    if (include == "products")
                    {
                        string productColumn = @",p.Id AS 'Product Id',
                                                  p.Price AS 'Product Price',
                                                  p.Title AS 'Product Title',
                                                  p.[Description] AS 'Product Description',
                                                  p.Quantity AS 'Product Quantity',p.CustomerId AS 'Customer Id', p.ProductTypeId AS 'Product Type Id'";
                        string productTable = @"JOIN Product p ON c.Id = p.CustomerId";

                        //Making command = the query strings 
                        command = $@"{customerColumn}
                                     {productColumn}
                                     {customerTable}
                                      {productTable}";
                    }
                    else
                    // set command to = just customer information if the user does not add 'include'
                    {
                        command = $@"{customerColumn}
                                      {customerTable}";
                    }

                    //If statement for 'q' string, user sets q='whatever' and find 'FirstName/LastName' that is LIKE what user sets 'q=';
                    //Future reference, don't forget to add '%' outside {q}
                    if (q != null)
                    {
                        command += $" WHERE c.FirstName LIKE '{q}%' OR c.LastName LIKE '{q}%'";
                    }
                    //Another query string, doing the same thing as product except w/ payments
                    if (include == "payments")
                    {

                        string paymentColumn = @",pm.Id AS 'Payment Id',
                                                  pm.Name AS 'Payment Name',
                                                  pm.AcctNumber AS 'Payment Account Number', pm.CustomerId AS 'Customer Id'";
                        string paymentTable = @"Join PaymentType pm on c.Id = pm.CustomerId";
                        //Adding the strings together to show customer and payment
                        command = $@"{customerColumn}
                                     {paymentColumn}
                                     {customerTable}
                                     {paymentTable}";
                    }

                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())
                    {
                        Customer attempt = new Customer
                        //Getting the information of the customer - FUTURE JOSH MAKE SURE YOU REFERENCE WHAT YOU SET THE COLUM NAME 'AS' --
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Customer Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("Customer First Name")),
                            LastName = reader.GetString(reader.GetOrdinal("Customer Last Name"))
                        };

                        //Getting the information of the products if include == 'products'
                        if (include == "products")
                        {
                            Product currentProduct = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Product Id")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("Customer Id")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("Product Type Id")),
                                Price = reader.GetInt32(reader.GetOrdinal("Product Price")),
                                Title = reader.GetString(reader.GetOrdinal("Product Title")),
                                Description = reader.GetString(reader.GetOrdinal("Product Description")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Product Quantity"))
                            };

                            // Determining if customers list already has the current product in it
                            if (customers.Any(c => c.Id == attempt.Id))
                            {
                                //Finds the product in the list (if it is in there)
                                Customer thisCustomer = customers.Where(c => c.Id == attempt.Id).FirstOrDefault();
                                thisCustomer.ProductList.Add(currentProduct);
                            }
                            else
                            {
                                //if the product is not in the list it will add it
                                attempt.ProductList.Add(currentProduct);
                            }
                        }
                        if (include == "payments")
                        {
                            //Same thing as above but for payments
                            PaymentType currentPayment = new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Payment Id")),
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("Payment Account Number")),
                                Name = reader.GetString(reader.GetOrdinal("Payment Name")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("Customer Id"))
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

        //Get for single customer
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


        //Post 'Add' Method
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer Customer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Customer (FirstName, LastName)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstname, @lastname)";
                    cmd.Parameters.Add(new SqlParameter("@firstname", Customer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", Customer.LastName));
                    int newId = (int)cmd.ExecuteScalar();
                    Customer.Id = newId;
                    return CreatedAtRoute("GetCustomer", new { Id = newId }, Customer);
                }
            }
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Customer Customer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Customer
                                            SET FirstName=@first, 
                                            LastName=@last
                                            WHERE Id = @Id";
                        cmd.Parameters.Add(new SqlParameter("@first", Customer.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@last", Customer.LastName));
                        cmd.Parameters.Add(new SqlParameter("@Id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!CustomerExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //added delete method for testing
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Customer WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!CustomerExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }



        private bool CustomerExist(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            FirstName, LastName
                        FROM Customer
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}