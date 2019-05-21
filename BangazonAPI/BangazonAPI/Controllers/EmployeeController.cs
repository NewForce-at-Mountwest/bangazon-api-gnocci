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
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmployeeController(IConfiguration config)
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
            // GET: api/Employee
            [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"Select e.Id, e.DepartmentId, e.FirstName, e.LastName, e.IsSuperVisor, d.Id, d.Budget, d.Name AS 'Department Name', ce.Id AS 'ComputerEmployee Id', ce.EmployeeId, ce.ComputerId, c.Id, c.Make, c.Manufacturer, c.PurchaseDate, c.DecomissionDate, ce.AssignDate  FROM Employee e  LEFT JOIN ComputerEmployee ce on e.id = ce.EmployeeId  LEFT JOIN Computer c on ce.ComputerId = c.Id  JOIN Department d on e.DepartmentId = d.Id";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Employee> employees = new List<Employee>();

                    while (reader.Read())
                    {

                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            CurrentDepartment = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Department Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                            }

                            //CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("ComputerEmployee Id")))
                        {
                            Computer AssignedComputer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ComputerEmployee Id")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                //DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"))

                            };
                            employee.AssignedComputer = AssignedComputer;
                        }
                        
                            employees.Add(employee);
                        
                    }
                    reader.Close();

                    return Ok(employees);
                }
            }
        }









        // GET: api/Employee/5
        [HttpGet("{id}", Name = "GetEmployee")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"Select e.Id, e.DepartmentId, e.FirstName, e.LastName,
                                       e.IsSuperVisor, d.Id, d.Budget, d.Name AS 'Department Name',
                                       ce.Id, ce.EmployeeId, ce.ComputerId, c.Id, c.Make,
                                       c.Manufacturer, c.PurchaseDate, c.DecomissionDate, ce.AssignDate 
                                       FROM Employee e 
                                       JOIN Department d ON e.DepartmentId = d.Id
                                       JOIN ComputerEmployee ce ON e.Id = ce.EmployeeId 
                                       JOIN Computer c ON ce.EmployeeId = c.Id
                                       WHERE e.Id = @id"; 
                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Employee employee = null;

                    if (reader.Read())
                    {
                         employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("IsSuperVisor")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            AssignedComputer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                //DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"))

                            },
                            CurrentDepartment = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Department Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                            }

                            //CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                        };

                    }
                    reader.Close();

                    return Ok(employee);
                }
            }
        }


        // POST: api/Employee
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee employee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Employee (FirstName, LastName, DepartmentId, IsSuperVisor)
                                        OUTPUT INSERTED.Id
                                        VALUES (@FirstName, @LastName, @DepartmentId, @IsSuperVisor)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", employee.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", employee.LastName));
                    cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));
                    cmd.Parameters.Add(new SqlParameter("@IsSuperVisor", employee.IsSuperVisor));




                    int newId = (int)cmd.ExecuteScalar();
                    employee.Id = newId;
                    return CreatedAtRoute("GetEmployee", new { id = newId }, employee);
                }
            }
        }

        // PUT: api/Employee/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
