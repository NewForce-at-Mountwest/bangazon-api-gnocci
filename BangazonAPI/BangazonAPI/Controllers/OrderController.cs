using System;
using System.Collections.Generic;
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

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly IConfiguration _config;

        public OrderController(IConfiguration config)
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
        public async Task<IActionResult> Get(string include, bool completed)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //Making an empty string "command" to build up for different query strings
                    string command = "";
                    //Strings to show customer information
                    string orderColumn = @"SELECT o.Id AS 'Order Id', o.PaymentTypeId AS 'Payment-Type Id', o.CustomerId AS 'Customer Id'";
                    string orderTable = "FROM [Order] o";

                    //Making Query string to show customer product type if they ask for it (?include=products)
                    if (include == "products")
                    {
                        string orderProductColumn = @",op.Id AS 'Order-Product Id', op.OrderId AS 'Order Id', op.ProductId AS 'Product Id'";
                        string orderProductTable = @"JOIN[Order] o ON op.OrderId = o.Id";


                        string productColumn = @",p.Id AS 'Product Id',
                                                  p.Price AS 'Product Price',
                                                  p.Title AS 'Product Title',
                                                  p.[Description] AS 'Product Description',
                                                  p.Quantity AS 'Product Quantity',p.CustomerId AS 'Customer Id', p.ProductTypeId AS 'Product Type Id'";
                        string productTable = @"FROM OrderProduct op JOIN Product p ON p.Id =  op.ProductId";

                        //Making command = the query strings 
                        command = $@"{orderColumn}
                                     {orderProductColumn}
                                     {productColumn}
                                     {productTable}
                                     {orderProductTable}";
                    }
                    else
                    // set command to = just customer information if the user does not add 'include'
                    {
                        command = $@"{orderColumn}
                                      {orderTable}";
                    }
                    //Another query string, doing the same thing as product except w/ payments
                    if (include == "customers")
                    {

                        string customerColumn = @",
                                                    c.FirstName AS 'Customer First Name',
                                                    c.LastName AS 'Customer Last Name'";
                        string customerTable = @"FROM Customer c JOIN [Order] o ON c.Id = o.CustomerId";
                        //Adding the strings together to show customer and payment
                        command = $@"{orderColumn}
                                     {customerColumn}
                                     {customerTable}";
                    }


                    if (completed == true)
                    {
                        command = @"SELECT o.Id AS 'Order Id', o.PaymentTypeId AS 'Payment-Type Id', o.CustomerId AS 'Customer Id', p.AcctNumber AS 'Account Number', p.[Name] AS 'Name' FROM [Order] o JOIN PaymentType p ON o.PaymentTypeId = p.Id";
                    }

                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Order> orders = new List<Order>();

                    while (reader.Read())
                    {
                        Order currentOrder = new Order
                        //Getting the information of the customer - FUTURE JOSH MAKE SURE YOU REFERENCE WHAT YOU SET THE COLUM NAME 'AS' --
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Order Id")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("Payment-Type Id")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("Customer Id"))
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
                            if (orders.Any(c => c.Id == currentOrder.Id))
                            {
                                //Finds the product in the list (if it is in there)
                                Order thisCustomer = orders.Where(c => c.Id == currentOrder.Id).FirstOrDefault();
                                thisCustomer.ProductList.Add(currentProduct);
                            }
                            else
                            {
                                //if the product is not in the list it will add it
                                currentOrder.ProductList.Add(currentProduct);
                            }
                        }
                        if (include == "customers")
                        {
                            //Same thing as above but for payments
                            Customer NewCustomer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Customer Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("Customer First Name")),
                                LastName = reader.GetString(reader.GetOrdinal("Customer Last Name"))
                            };
                            currentOrder.SingleCustomer = NewCustomer;
                            orders.Add(currentOrder);
                        
                        }
                        else
                        {
                            orders.Add(currentOrder);
                        }
                    }
                    reader.Close();

                    return Ok(orders);
                }
            }
        }

        [HttpGet("{Id}", Name = "GetOrder")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, PaymentTypeId, CustomerId
                        FROM [Order]
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Order Order = null;

                    if (reader.Read())
                    {
                        Order = new Order
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId"))

                        };
                    }
                    reader.Close();

                    return Ok(Order);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order Order)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO [Order] (PaymentTypeId, CustomerId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@PaymentTypeId, @CustomerId)";
                    cmd.Parameters.Add(new SqlParameter("@PaymentTypeId", Order.PaymentTypeId));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", Order.CustomerId));
                    int newId = (int)cmd.ExecuteScalar();
                    Order.Id = newId;
                    return CreatedAtRoute("GetOrder", new { id = newId }, Order);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Order Order)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE [Order]
                                            SET PaymentTypeId=@payment, 
                                            CustomerId=@customer
                                            WHERE Id = @Id";
                        cmd.Parameters.Add(new SqlParameter("@payment", Order.PaymentTypeId));
                        cmd.Parameters.Add(new SqlParameter("@customer", Order.CustomerId));
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
                if (!OrderExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

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
                        cmd.CommandText = @"DELETE FROM Order WHERE Id = @id";
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
                if (!OrderExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool OrderExist(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            PaymentTypeId, CustomerId
                        FROM Order
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}