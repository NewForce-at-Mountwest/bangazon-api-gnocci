using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using BangazonAPI.Data;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


namespace BangazonAPI.Controllers
{

    //Ticket #3: Allow Developers to Access Payment Type Resource

    //# Feature Criteria:
    // - `GET`
    // - `POST`
    // - `PUT`
    // - `DELETE`
    // - User should be able to GET a list, and GET a single item.


    //Class to GET all, POST, PUT, and DELETE payment type in Bamngazon API:
    [Route("api/[controller]")]
    [ApiController]
    //public class PaymentTypeController : Controller
    public class PaymentTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentTypeController(IConfiguration config)

        //Empty Variable _context that references Context[.cs] class:
        //private Context _context;
        //public PaymentTypeController(Context ctx)
        {
            //_context = ctx;
            _config = config;
        }

        public SqlConnection Connection
        {

            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }


        // GET a List, and GET a Single Item:
        [HttpGet]
        //public IActionResult Get()
        //{
        //    IQueryable<object> PaymentType = from paymentType in _context.PaymentType select paymentType;

        //    if (PaymentType == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(PaymentType);

        //}
        public async Task<IActionResult> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //Empty string "command" to Build Query Strings:
                    string command = "";
                    //Strings to Show Payment Type Information:
                    string paymentTypeColumn = @"SELECT pm.Id AS 'PaymentType Id',
                    pm.AcctNumber AS 'PaymentType Account Number',
                    pm.[Name] AS 'PaymentType (Card Type)'";
                    string paymentTypeTable = "FROM PaymentType pm";

                    //Making Query String to Show Customer Information, if Requested (?include=customers):
                    if (include == "customers")
                    {
                        string customerColumn = @",c.Id AS 'Customer Id',
                                                  c.FirstName AS 'Customer First Name',
                                                  c.LastName AS 'Customer Last Name'";
                        string customerTable = @"JOIN Customer c ON c.Id = pm.CustomerId";

                        //Making "command" = the Query Strings:
                        command = $@"{paymentTypeColumn}
                                     {customerColumn}
                                     {paymentTypeTable}
                                     {customerTable}";
                    }

                    else
                    // Set "command" to = Only Payment Type Information, if the User does not add "include":
                    {
                        command = $@"{paymentTypeColumn}
                                     {paymentTypeTable}";
                    }

                    //If Statement for 'q' String, User Sets q = "":
                    if (q != null)
                    {
                        command += $" WHERE pm.AcctNumber LIKE '{q}%' OR pm.Name LIKE '{q}%'";
                    }

                    //Query String for Orders:
                    if (include == "orders")
                    {

                        string orderColumn = @",o.Id AS 'Order Id',
                                                  o.CustomerId AS 'Order Customer ID',
                                                  o.PaymentTypeId AS 'Payment Type ID', o.CustomerId AS 'Customer Id'";
                        string orderTable = @"Join Order o on pm.Id = o.PaymentTypeId";
                        //Combining Strings to show Payment Type and Order Information:
                        command = $@"{paymentTypeColumn}
                                     {orderColumn}
                                     {paymentTypeTable}
                                     {orderTable}";
                    }

                    cmd.CommandText = command;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<PaymentType> paymentTypes = new List<PaymentType>();

                    while (reader.Read())
                    {
                        PaymentType attempt = new PaymentType
                        //Getting the Payment Type Information:
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("PaymentType Id")),
                            AcctNumber = reader.GetInt32(reader.GetOrdinal("PaymentType Account Number")),
                            Name = reader.GetString(reader.GetOrdinal("PaymentType (Card Type)"))
                        };

                        //Getting the Customer Information, if included:
                        if (include == "customers")
                        {
                            Customer currentCustomer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Customer Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("Customer First Name")),
                                LastName = reader.GetString(reader.GetOrdinal("Customer Last Name"))
                            };

                            // Determining if Payment Type List references the Customer(s):
                            if (paymentTypes.Any(pm => pm.Id == attempt.Id))
                            {
                                //Finds the Customer in List:
                                PaymentType thisPaymentType = paymentTypes.Where(pm => pm.Id == attempt.Id).FirstOrDefault();
                                thisPaymentType.CustomerList.Add(currentCustomer);
                            }

                            //If PaymentType is NOT in said List, it will be Added:
                            else
                            {
                                attempt.CustomerList.Add(currentCustomer);
                            }
                        }
                        if (include == "orders")
                        {
                            //For Orders:
                            Order currentOrder = new Order
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Order Id")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("Order Customer ID")),
                                PaymentTypeId = reader.GetInt32(reader.GetOrdinal("Payment Type ID"))
                            };

                            if (paymentTypes.Any(pm => pm.Id == attempt.Id))
                            {
                                PaymentType thisPaymentType = paymentTypes.Where(pm => pm.Id == attempt.Id).FirstOrDefault();
                                thisPaymentType.OrderList.Add(currentOrder);
                            }
                            else
                            {
                                attempt.OrderList.Add(currentOrder);
                                paymentTypes.Add(attempt);
                            }
                        }
                        //Print:
                        else
                        {
                            paymentTypes.Add(attempt);
                        }
                    }
                    reader.Close();

                    return Ok(paymentTypes);
                }
            }
        }

        //string paymentTypeColumn = @"SELECT pm.Id AS 'PaymentType Id',
        //            pm.AcctNumber AS 'PaymentType Account Number',
        //            pm.[Name] AS 'PaymentType (Card Type)'";
        //string paymentTypeTable = "FROM PaymentType pm";
        // GET a Single Payment Type by "Id":
        [HttpGet("{Id}", Name = "GetSinglePaymentType")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, AcctNumber, [Name]
                        FROM PaymentType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    PaymentType PaymentType = null;

                    if (reader.Read())
                    {
                        PaymentType = new PaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("PaymentType Id")),
                            AcctNumber = reader.GetInt32(reader.GetOrdinal("PaymentType Account Number")),
                            Name = reader.GetString(reader.GetOrdinal("PaymentType (Card Type)"))

                        };
                    }
                    reader.Close();

                    return Ok(PaymentType);
                }
            }
        }
        //public IActionResult Get([FromRoute] int Id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    try
        //    {
        //        PaymentType PaymentType = _context.PaymentType.Single(m => m.Id == Id);

        //        if (PaymentType == null)
        //        {
        //            return NotFound();
        //        }

        //        return Ok(PaymentType);
        //    }
        //    catch (System.InvalidOperationException ex)
        //    {
        //        return NotFound(ex);
        //    }
        //}


        // POST ('Add') New Payment Type:
        [HttpPost]
        //public IActionResult Post([FromBody] PaymentType newPaymentType)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    _context.PaymentType.Add(newPaymentType);

        //    try
        //    {
        //        _context.SaveChanges();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        if (PaymentTypeExists(newPaymentType.Id))
        //        {
        //            return new StatusCodeResult(StatusCodes.Status409Conflict);
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return CreatedAtRoute("GetSinglePaymentType", new { id = newPaymentType.Id }, newPaymentType);
        //}
        public async Task<IActionResult> Post([FromBody] PaymentType PaymentType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO PaymentType (AcctNumber, Name)
                                        OUTPUT INSERTED.Id
                                        VALUES (@acctnumber, @name)";
                    cmd.Parameters.Add(new SqlParameter("@acctnumber", PaymentType.AcctNumber));
                    cmd.Parameters.Add(new SqlParameter("@name", PaymentType.Name));
                    int newId = (int)cmd.ExecuteScalar();
                    PaymentType.Id = newId;
                    return CreatedAtRoute("GetPaymentType", new { id = newId }, PaymentType);
                }
            }
        }


        // "HELPER METHOD" checks if PaymentType Exists in the Database:
        //private bool PaymentTypeExists(int Id)
        //{
        //    return _context.PaymentType.Count(e => e.Id == Id) > 0;
        //}


        // EDIT Payment Type Object:
        [HttpPut("{id}")]
        //public IActionResult Put(int id, [FromBody] PaymentType modifiedPaymentType)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != modifiedPaymentType.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(modifiedPaymentType).State = EntityState.Modified;

        //    try
        //    {
        //        _context.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!PaymentTypeExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return new StatusCodeResult(StatusCodes.Status204NoContent);
        //}
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] PaymentType PaymentType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE PaymentType
                                            SET AcctNumber=@account, 
                                            Name=@name
                                            WHERE Id = @Id";
                        cmd.Parameters.Add(new SqlParameter("@first", PaymentType.AcctNumber));
                        cmd.Parameters.Add(new SqlParameter("@last", PaymentType.Name));
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
                if (!PaymentTypeExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        // DELETE a Payment Type:
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    PaymentType paymentType = _context.PaymentType.Single(m => m.Id == id);
        //    if (paymentType == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.PaymentType.Remove(paymentType);
        //    _context.SaveChanges();

        //    return Ok(paymentType);
        //}
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM PaymentType WHERE Id = @id";
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
                if (!PaymentTypeExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool PaymentTypeExist(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            AcctNumber, Name
                        FROM PaymentType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}