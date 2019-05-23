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
                    cmd.CommandText = @"Select e.Id, e.DepartmentId, e.FirstName, e.LastName, e.IsSuperVisor, d.Id, d.Budget, d.Name AS 'Department Name', ce.Id AS 'ComputerEmployee Id', ce.EmployeeId, ce.ComputerId, c.Id AS 'computer Id', c.Make, c.Manufacturer, c.PurchaseDate, c.DecomissionDate, ce.AssignDate  FROM Employee e  LEFT JOIN ComputerEmployee ce on e.id = ce.EmployeeId  LEFT JOIN Computer c on ce.ComputerId = c.Id  JOIN Department d on e.DepartmentId = d.Id";
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

                       };
                        if (!reader.IsDBNull(reader.GetOrdinal("computer Id")))
                        {
                            Computer AssignedComputer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("computer Id")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"))

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
                    cmd.CommandText = @"Select e.Id, e.DepartmentId, e.FirstName, e.LastName, e.IsSuperVisor, d.Id, d.Budget, d.Name AS 'Department Name', ce.Id AS 'ComputerEmployee Id', ce.EmployeeId, ce.ComputerId, c.Id AS 'computer Id', c.Make, c.Manufacturer, c.PurchaseDate, c.DecomissionDate, ce.AssignDate  
                      FROM Employee e  
                      LEFT JOIN ComputerEmployee ce on e.id = ce.EmployeeId  
                      LEFT JOIN Computer c on ce.ComputerId = c.Id  
                      JOIN Department d on e.DepartmentId = d.Id WHERE e.Id = @id";
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
                            CurrentDepartment = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Department Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget"))
                            }
                       };
                        if (!reader.IsDBNull(reader.GetOrdinal("computer Id")))
                        {
                            Computer AssignedComputer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("computer Id")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"))
                            };
                            employee.AssignedComputer = AssignedComputer;
                        }


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
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Employee employee)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Employee
                                            SET FirstName = @FirstName,
                                                LastName = @LastName,
                                                DepartmentId = @DepartmentId,
                                                IsSuperVisor = @IsSuperVisor
                                            WHERE id = @id";
                        cmd.Parameters.Add(new SqlParameter("@FirstName", employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@LastName", employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));
                        cmd.Parameters.Add(new SqlParameter("@IsSuperVisor", employee.IsSuperVisor));
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
                if (!EmployeeExists(id))
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
                        cmd.CommandText = @"DELETE FROM Employee WHERE Id = @id";
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
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool EmployeeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, DepartmentId, IsSupervisor
                        FROM Employee
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

    }


    

}
