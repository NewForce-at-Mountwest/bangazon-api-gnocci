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
    public class ProductTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductTypeController(IConfiguration config)
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

        // GET: api/ProductType
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, IsArchived FROM ProductType";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<ProductType> ProductType = new List<ProductType>();

                    while (reader.Read())
                    {

                        ProductType newProductType = new ProductType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            IsArchived = reader.GetBoolean(reader.GetOrdinal("IsArchived"))
                        };

                        //IF is archived is false we're going to add newproducttype to our product type

                        if (newProductType.IsArchived == false)
                        {
                            ProductType.Add(newProductType);
                        }

                    }
                    reader.Close();

                    return Ok(ProductType);
                }
            }
        }

        //get single product type
        [HttpGet("{id}", Name = "GetProductType")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, IsArchived FROM ProductType WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    ProductType newProductType = null;

                    if (reader.Read())
                    {
                        newProductType = new ProductType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            IsArchived = reader.GetBoolean(reader.GetOrdinal("IsArchived"))
                        };

                    }
                    reader.Close();

                    return Ok(newProductType);
                }
            }
        }

        //adding producttype to db 

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductType productType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO ProductType (name, IsArchived)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @IsArchived)";
                    cmd.Parameters.Add(new SqlParameter("@name", productType.Name));
                    cmd.Parameters.Add(new SqlParameter("@IsArchived", productType.IsArchived));

                    int newId = (int)cmd.ExecuteScalar();
                    productType.Id = newId;
                    return CreatedAtRoute("GetProductType", new { Id = newId }, productType);
                }
            }
        }


        //edit product type
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] ProductType producttype)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE ProductType
                                            SET Name = @Name,
                                                IsArchived = @IsArchived
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@Name", producttype.Name));
                        cmd.Parameters.Add(new SqlParameter("@IsArchived", producttype.IsArchived));
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
                if (!ProductTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //delete prodcttype - add bool paramater for hard delete, that way if harddelete = true we hard delete else soft delete (Archive it)
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
                            cmd.CommandText = @"DELETE FROM ProductType WHERE Id = @id";
                            cmd.Parameters.Add(new SqlParameter("@id", id));

                        }
                        else
                        {
                            cmd.CommandText = @"UPDATE ProductType 
                                            SET IsArchived = 1
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
                if (!ProductTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ProductTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, IsArchived
                        FROM ProductType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }


    }
}