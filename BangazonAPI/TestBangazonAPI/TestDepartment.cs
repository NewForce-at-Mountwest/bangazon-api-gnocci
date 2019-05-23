using BangazonAPI.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestBangazonAPI;
using Xunit;

//TEST Department:
namespace BangazonAPITest
{
    public class TestDepartment
    {
        // CREATE New Department in the Database; Return a 200 OK Status Code:
        public async Task<Department> createDepartment(HttpClient client)
        {
            Department department = new Department
            {
                Name = "Test Department",
                Budget = 300001

            };
            string departmentAsJSON = JsonConvert.SerializeObject(department);
            HttpResponseMessage response = await client.PostAsync(
                "api/department",
                new StringContent(departmentAsJSON, Encoding.UTF8, "application/json")
            );
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Department newDepartment = JsonConvert.DeserializeObject<Department>(responseBody);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return newDepartment;
        }

        // DELETE Department in the Database; Return a No Content Status Code:
        public async Task deleteDepartment(Department department, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/department/{department.Id}?q=delete_test_item");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }

        [Fact]
        //TEST to GET Departments in the Database:
        public async Task Test_Get_All_Departments()
        {
            // Use HTTP Client:
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Call Route to GET ALL Departments; 
                //Wait for RESPONSE Object (GET):
                HttpResponseMessage response = await client.GetAsync("api/department");
                // RESPONSE Comes Back:
                response.EnsureSuccessStatusCode();
                // Read RESPONSE Body (as JSON):
                string responseBody = await response.Content.ReadAsStringAsync();
                // Convert JSON to a List of Department(s):
                List<Department> departmentList = JsonConvert.DeserializeObject<List<Department>>(responseBody);
                // 200 OK Status Code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                // Departments in List?
                Assert.True(departmentList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_All_Departments_Include_Employees()
        {
            // Use HTTP Client:
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Call Route to GET ALL Departments (w/ Employees); 
                //Wait for RESPONSE Object (GET):
                HttpResponseMessage response = await client.GetAsync("api/department?_include=employees");
                // RESPONSE Comes Back:
                response.EnsureSuccessStatusCode();
                // Read RESPONSE Body (as JSON):
                string responseBody = await response.Content.ReadAsStringAsync();
                // Convert JSON to a List of Department(s):
                List<Department> departmentList = JsonConvert.DeserializeObject<List<Department>>(responseBody);
                // 200 OK Status Code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                // Departments in List?
                Assert.True(departmentList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_All_Departments_Filter_by_Budget()
        {
            // Use HTTP Client:
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Call Route to GET ALL Departments (Budget > $300,000); 
                //Wait for RESPONSE Object (GET):
                HttpResponseMessage response = await client.GetAsync("api/department?_filter=budget&_gt>300000");
                // RESPONSE Comes Back:
                response.EnsureSuccessStatusCode();
                // Read RESPONSE Body (as JSON):
                string responseBody = await response.Content.ReadAsStringAsync();
                // Convert JSON to a List of Department(s):
                List<Department> departmentList = JsonConvert.DeserializeObject<List<Department>>(responseBody);
                // 200 OK Status Code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                // Departments in List?
                Assert.True(departmentList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Department()
        {
            // Use HTTP Client:
            using (HttpClient client = new APIClientProvider().Client)
            {
                // CREATE New Department:
                Department newDepartment = await createDepartment(client);
                // GET Department from Database:
                HttpResponseMessage response = await client.GetAsync($"api/department/{newDepartment.Id}");
                // RESPONSE Comes Back:
                response.EnsureSuccessStatusCode();
                // Read RESPONSE Body (as JSON):
                string responseBody = await response.Content.ReadAsStringAsync();
                // Turn JSON into C Sharp:
                Department department = JsonConvert.DeserializeObject<Department>(responseBody);
                // Return Expected RESPONSE? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Test Department", newDepartment.Name);
                // Delete Department:
                deleteDepartment(newDepartment, client);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Department_Include_Employees()
        {
            // Use HTTP Client:
            using (HttpClient client = new APIClientProvider().Client)
            {
                // CREATE New Department:
                Department newDepartment = await createDepartment(client);
                // GET Department from Database:
                HttpResponseMessage response = await client.GetAsync($"api/department/{newDepartment.Id}?_include=employees");
                // RESPONSE Comes Back:
                response.EnsureSuccessStatusCode();
                // Read RESPONSE Body (as JSON):
                string responseBody = await response.Content.ReadAsStringAsync();
                // Turn JSON into C Sharp:
                Department department = JsonConvert.DeserializeObject<Department>(responseBody);
                // Return Expected RESPONSE? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Test Department", newDepartment.Name);
                // Delete Department:
                deleteDepartment(newDepartment, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExistent_Department_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // GET Department with Huge ID:
                HttpResponseMessage response = await client.GetAsync("api/department/666666666");
                // Return 204 No Content Error:
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Create_Department()
        {
            using (var client = new APIClientProvider().Client)
            {
                // CREATE New Department:
                Department newDepartment = await createDepartment(client);
                // Confirm Information:
                Assert.Equal("Test Department", newDepartment.Name);
                // DELETE Department:
                deleteDepartment(newDepartment, client);
            }
        }

        [Fact]
        public async Task Test_Modify_Department()
        {
            // CHANGE Department's Name (New Name):
            string newName = "House Castelloe";
            // Use HTTP Client:
            using (HttpClient client = new APIClientProvider().Client)
            {
                // CREATE New Department:
                Department newDepartment = await createDepartment(client);
                // CHANGE Name:
                newDepartment.Name = newName;
                // CONVERT Response to JSON:
                string modifiedDepartmentAsJSON = JsonConvert.SerializeObject(newDepartment);
                // Make a PUT Request with the New Information:
                HttpResponseMessage response = await client.PutAsync(
                    $"api/department/{newDepartment.Id}",
                    new StringContent(modifiedDepartmentAsJSON, Encoding.UTF8, "application/json")
                );
                // RESPONSE Comes Back:
                response.EnsureSuccessStatusCode();
                // CONVERT Response to JSON:
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return No Content Status Code:
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                /*GET Section*/
                // GET the Department Previously Edited:
                HttpResponseMessage getDepartment = await client.GetAsync($"api/department/{newDepartment.Id}");
                getDepartment.EnsureSuccessStatusCode();
                string getDepartmentBody = await getDepartment.Content.ReadAsStringAsync();
                Department modifiedDepartment = JsonConvert.DeserializeObject<Department>(getDepartmentBody);
                Assert.Equal(HttpStatusCode.OK, getDepartment.StatusCode);
                // CONFIRM Update:
                Assert.Equal(newName, modifiedDepartment.Name);
                // DELETE Modified Department:
                deleteDepartment(modifiedDepartment, client);
            }
        }
    }
}