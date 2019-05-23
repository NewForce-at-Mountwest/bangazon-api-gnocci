using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//Ticket #4: Allow Developers to Access the Orders Resource:
//# Feature Criteria:
// - `GET`
// - `POST`
// - `PUT`
// - `DELETE`
// - User should be able to GET a List, and GET a Single Item.
// - When an order is deleted, every line item(i.e.entry in OrderProduct) should be removed
// - Should be able to filter out completed orders with the ? completed = false query string parameter.If the parameter value is true, then only completed order should be returned.
// - If the query string parameter of? _include = products is in the URL, then the list of products in the order should be returned.
// - If the query string parameter of? _include = customers is in the URL, then the customer representation should be included in the response.

//Class to GET All, POST, PUT, and DELETE Department in Bangazon API:
namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentController(IConfiguration config)
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

        //GET List of Departments
        [HttpGet]

        public async Task<IActionResult> GetAllDepartments(string _include, string _filter, int _gt)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    string commandText = $"SELECT d.Id as 'DepartmentId', d.Name AS 'Department Name', d.Budget AS 'Department Budget', e.Id as 'Employee Id', e.FirstName as 'Employee First Name', e.LastName as 'Employee Last Name', e.IsSuperVisor FROM Department d Full JOIN Employee e on d.Id = e.DepartmentId";
                    //Query String Parameters of `?_filter=budget&_gt=':
                    if (_filter == "Budget")
                    {
                        commandText += $" WHERE d.Budget >= '{_gt}'";
                    }
                    cmd.CommandText = commandText;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Department> departments = new List<Department>();
                    Department department = null;
                    List<Employee> employees = new List<Employee>();
                    Employee employee = null;
                    while (reader.Read())
                    {
                        department = new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Name = reader.GetString(reader.GetOrdinal("Department Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Department Budget"))
                        };

                        //CONFIRM Line Item on Table has an Employee[ID]:
                        if (!reader.IsDBNull(reader.GetOrdinal("Employee Id")))
                        {
                            employee = new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Employee Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("Employee First Name")),
                                LastName = reader.GetString(reader.GetOrdinal("Employee Last Name")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("isSuperVisor"))
                            };
                        }
                        else { employee = null; }
                        if (departments.Any(d => d.Id == department.Id))
                        {
                            //Query String Parameter of `?_include = employees`:
                            Department departmentOnList = departments.Where(d => d.Id == department.Id).FirstOrDefault();
                            if (_include == "employees")
                            {
                                if (!departmentOnList.EmployeeList.Any(e => e.Id == employee.Id))
                                {
                                    departmentOnList.EmployeeList.Add(employee);
                                }
                            }
                        }
                        else
                        {
                            if (_include == "employees")
                            {
                                department.EmployeeList.Add(employee);
                            }

                            departments.Add(department);
                        }
                    }
                    reader.Close();
                    return Ok(departments);
                }
            }
        }

        //GET Single Department:
        [HttpGet("{Id}", Name = "Department")]

        public async Task<IActionResult> GetSingleDepartment([FromRoute] int Id, string _include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT d.Id as 'Department Id', d.Name AS 'Department Name', d.Budget, e.Id as 'Employee Id', e.FirstName as 'Employee First Name', e.LastName as 'Employee Last Name', e.IsSuperVisor FROM Department d Full JOIN Employee e on d.Id = e.DepartmentId WHERE d.Id=@Id";
                    cmd.Parameters.Add(new SqlParameter("@Id", Id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Employee employee = null;
                    Department departmentToDisplay = null;
                    int counter = 0;
                    while (reader.Read())
                    {
                        if (counter < 1)
                        {
                            departmentToDisplay = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Department Id")),
                                Name = reader.GetString(reader.GetOrdinal("Department Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Department Budget"))
                            };
                            counter++;
                        }
                        if (_include == "employees")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("Employee Id")))
                            {
                                employee = new Employee
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Employee Id")),
                                    FirstName = reader.GetString(reader.GetOrdinal("Employee First Name")),
                                    LastName = reader.GetString(reader.GetOrdinal("Employee Last Name")),
                                    DepartmentId = reader.GetInt32(reader.GetOrdinal("Department Id")),
                                    IsSuperVisor = reader.GetBoolean(reader.GetOrdinal("isSuperVisor"))
                                };
                                departmentToDisplay.EmployeeList.Add(employee);
                            }
                        }
                    };
                    reader.Close();
                    return Ok(departmentToDisplay);
                }
            }
        }

        //  POST to Create Department:
        [HttpPost]
        public async Task<IActionResult> PostDepartment([FromBody] Department department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Department (Name, Budget)
                                                            OUTPUT INSERTED.Id
                                                            VALUES (@Name, @Budget)";
                    cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
                    cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));
                    int newId = (int)cmd.ExecuteScalar();
                    department.Id = newId;
                    return CreatedAtRoute("Department", new { Id = newId }, department);
                }
            }
        }

        // PUT to Edit Department:
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment([FromRoute] int Id, [FromBody] Department department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Department
                                           SET name = @Name,
                                           Budget = @Budget
                                           WHERE Id = @Id";
                        cmd.Parameters.Add(new SqlParameter("@Name", department.Name));
                        cmd.Parameters.Add(new SqlParameter("@Budget", department.Budget));
                        cmd.Parameters.Add(new SqlParameter("@Id", Id));
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
                if (!DepartmentExists(Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE Department:
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment([FromRoute] int Id, string q)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        if (q == "delete_test_item")
                        {
                            cmd.CommandText = @"DELETE Department
                                              WHERE Id = @Id";
                        }
                        cmd.Parameters.Add(new SqlParameter("@Id", Id));
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
                if (!DepartmentExists(Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool DepartmentExists(int Id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name
                                            FROM Department
                                            WHERE Id = @Id";
                    cmd.Parameters.Add(new SqlParameter("@Id", Id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}