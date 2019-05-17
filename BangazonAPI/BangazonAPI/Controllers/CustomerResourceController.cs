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
    public class CustomerResourceController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomerResourceController(IConfiguration config)
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
                    string command = "";
                    string customerColumn = @"SELECT c.Id AS 'Customer Id',
                    c.FirstName AS 'Customer First Name',
                    c.LastName AS 'Customer Last Name'";
                    string customerTable = "FROM Customer c";

                    if(include == "products")
                    {
                        string productColumn = @",p.Id AS 'Product Id',
                                                  p.Price AS 'Product Price',
                                                  p.Title AS 'Product Title',
                                                  p.[Description] AS 'Product Description',
                                                  p.Quantity AS 'Product Quantity";
                        string productTable = @"JOIN Product p ON c.Id = p.CustomerId;";
                        command = $@"{customerColumn}
                                     {productColumn}
                                     {customerTable}
                                      {productTable}";
                    }

                }


            }

        }