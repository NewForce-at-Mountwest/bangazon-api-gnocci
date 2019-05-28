using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComputerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ComputerController(IConfiguration config)
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

        // GET: api/Computer
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, PurchaseDate, DecomissionDate, Make, Manufacturer, isArchived FROM Computer";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Computer> Computer = new List<Computer>();

                    while (reader.Read())
                    {

                        Computer newComputer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),

                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            isArchived = reader.GetBoolean(reader.GetOrdinal("isArchived"))
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            newComputer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }

                        //IF is archived is false we're going to add newComputer to our Computer

                        if (newComputer.isArchived == false)
                        {
                            Computer.Add(newComputer);
                        }

                    }
                    reader.Close();

                    return Ok(Computer);
                }
            }
        }

            //get single Computer
        [HttpGet("{id}", Name = "GetComputer")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, PurchaseDate, DecomissionDate, Make, Manufacturer, isArchived FROM Computer WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Computer newComputer = null;

                    if (reader.Read())
                    {
                        newComputer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),

                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            isArchived = reader.GetBoolean(reader.GetOrdinal("isArchived"))
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            newComputer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }

                    }
                    reader.Close();

                    return Ok(newComputer);
                }
            }
        }

        //adding Computer to db 

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Computer Computer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Computer (PurchaseDate, DecomissionDate, Make, Manufacturer, isArchived)
                                        OUTPUT INSERTED.Id
                                        VALUES (@purchaseDate, @decomissiondate, @make, @manufacturer, @isarchived)";
                    cmd.Parameters.Add(new SqlParameter("@purchaseDate", Computer.PurchaseDate));
                    cmd.Parameters.Add(new SqlParameter("@decomissiondate", Computer.DecomissionDate));
                    cmd.Parameters.Add(new SqlParameter("@make", Computer.Make));
                    cmd.Parameters.Add(new SqlParameter("@manufacturer", Computer.Manufacturer));
                    cmd.Parameters.Add(new SqlParameter("@isarchived", Computer.isArchived));


                    int newId = (int)cmd.ExecuteScalar();
                    Computer.Id = newId;
                    return CreatedAtRoute("GetComputer", new { id = newId }, Computer);
                }
            }
        }

        //edit Computer
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Computer Computer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Computer
                                            SET PurchaseDate = @purchaseDate, 
                                            DecomissionDate = @decomissiondate, 
                                            Make = @make, 
                                            Manufacturer = @manufacturer, 
                                            isArchived = @isarchived
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@purchaseDate", Computer.PurchaseDate));
                        cmd.Parameters.Add(new SqlParameter("@decomissiondate", Computer.DecomissionDate));
                        cmd.Parameters.Add(new SqlParameter("@make", Computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@manufacturer", Computer.Manufacturer));
                        cmd.Parameters.Add(new SqlParameter("@isarchived", Computer.isArchived));
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
                if (!ComputerExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        //delete Computer - add bool paramater for hard delete, that way if harddelete = true we hard delete else soft delete (Archive it)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id, bool harddelete)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        if (harddelete == true)
                        {
                            cmd.CommandText = @"DELETE FROM Computer WHERE Id = @id";
                            cmd.Parameters.Add(new SqlParameter("@id", id));

                        }
                        else
                        {
                            cmd.CommandText = @"UPDATE Computer 
                                            SET isArchived = 1
                                            Where Id = @id";
                            cmd.Parameters.Add(new SqlParameter("@id", id));
                        }

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
                if (!ComputerExist(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool ComputerExist(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, PurchaseDate, DecomissionDate, Make, Manufacturer, isArchived FROM Computer WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}