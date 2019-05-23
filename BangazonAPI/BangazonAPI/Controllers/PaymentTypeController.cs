using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

//Ticket #3: Allow Developers to Access Payment Type Resource

//# Feature Criteria:
// - `GET`
// - `POST`
// - `PUT`
// - `DELETE`
// - User should be able to GET a List, and GET a Single Item.

//Class to GET All, POST, PUT, and DELETE Payment Type in Bamngazon API:
namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentTypeController(IConfiguration config)
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

        //GET List of ACTIVE (Is[NOT]Archived) Payment Types:
        [HttpGet]
        public async Task<IActionResult> GetAllPaymentTypes()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string commandText = $"SELECT Id, AcctNumber, Name, CustomerId, IsArchived FROM PaymentType WHERE IsArchived = 0";
                    cmd.CommandText = commandText;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<PaymentType> paymentTypes = new List<PaymentType>();
                    PaymentType paymentType = null;
                    while (reader.Read())
                    {
                        paymentType = new PaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            IsArchived = reader.GetBoolean(reader.GetOrdinal("IsArchived"))
                        };
                        paymentTypes.Add(paymentType);
                    }
                    reader.Close();
                    return Ok(paymentTypes);
                }
            }
        }

        //GET Code for Single Payment Type (Is[NOT]Archived?):
        [HttpGet("{id}", Name = "PaymentType")]
        public async Task<IActionResult> GetSinglePaymentType([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT Id, AcctNumber, Name, CustomerId, IsArchived from PaymentType WHERE Id=@id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    PaymentType paymentTypeToDisplay = null;
                    while (reader.Read())
                    {
                        {
                            paymentTypeToDisplay = new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                IsArchived = reader.GetBoolean(reader.GetOrdinal("IsArchived"))
                            };
                        };
                    };
                    reader.Close();
                    return Ok(paymentTypeToDisplay);
                }
            }
        }

        //  POST: Code to CREATE a Payment Type ('IsArchived' to 0 [false]):
        [HttpPost]
        public async Task<IActionResult> PostPaymentType([FromBody] PaymentType paymentType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"INSERT INTO PaymentType (AcctNumber, Name, CustomerId, IsArchived)
                                                    OUTPUT INSERTED.Id
                                                    VALUES (@AcctNumber, @Name, @CustomerId, 0)";
                    cmd.Parameters.Add(new SqlParameter("@AcctNumber", paymentType.AcctNumber));
                    cmd.Parameters.Add(new SqlParameter("@Name", paymentType.Name));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", paymentType.CustomerId));
                    int newId = (int)cmd.ExecuteScalar();
                    paymentType.Id = newId;
                    return CreatedAtRoute("PaymentType", new { id = newId }, paymentType);
                }
            }
        }

        // PUT Code to EDIT a Payment Type:
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentType([FromRoute] int id, [FromBody] PaymentType paymentType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE PaymentType
                                                        SET AcctNumber = @AcctNumber,
                                                        Name = @Name,
                                                        CustomerId=@CustomerId,
                                                        IsArchived = 0 WHERE id = @id";
                        cmd.Parameters.Add(new SqlParameter("@AcctNumber", paymentType.AcctNumber));
                        cmd.Parameters.Add(new SqlParameter("@Name", paymentType.Name));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", paymentType.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No Rows Affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE Code for Payment Type ("SOFT Delete" Changes 'IsArchived' to 1 [true]):
        [HttpDelete("{id}")]
        public async Task<IActionResult> deletePaymentType([FromRoute] int id, bool HardDelete)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        if (HardDelete == true)
                        {
                            cmd.CommandText = @"DELETE PaymentType
                                              WHERE id = @id";
                        }
                        else
                        {
                            cmd.CommandText = @"UPDATE PaymentType
                                            SET IsArchived = 1
                                            WHERE id = @id";
                        }
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No Rows Affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool PaymentTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, name
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