using BangazonAPI.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestBangazonAPI;
using Xunit;

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
        //TEST to GET Department(s) in the Database:
        public async Task Test_Get_All_Departments()
        {
            // Use HTTP Client:
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Call the route to get all our departments; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/department");

                // Make sure that a response comes back at all
                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of department instances
                List<Department> departmentList = JsonConvert.DeserializeObject<List<Department>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any departments in the list?
                Assert.True(departmentList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_All_Departments_Include_Employees()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our departments; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/department?_include=employees");

                // Make sure that a response comes back at all
                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of department instances
                List<Department> departmentList = JsonConvert.DeserializeObject<List<Department>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any departments in the list?
                Assert.True(departmentList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_All_Departments_Filter_by_Budget()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our departments; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/department?_filter=budget&_gt>60000");

                // Make sure that a response comes back at all
                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of department instances
                List<Department> departmentList = JsonConvert.DeserializeObject<List<Department>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any departments in the list?
                Assert.True(departmentList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Department()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new department
                Department newDepartment = await createDepartment(client);

                // Try to get that department from the database
                HttpResponseMessage response = await client.GetAsync($"api/department/{newDepartment.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Department department = JsonConvert.DeserializeObject<Department>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Test Department", newDepartment.Name);


                // Clean up after ourselves- delete department!
                deleteDepartment(newDepartment, client);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Department_Include_Employees()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new department
                Department newDepartment = await createDepartment(client);

                // Try to get that department from the database
                HttpResponseMessage response = await client.GetAsync($"api/department/{newDepartment.Id}?_include=employees");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Department department = JsonConvert.DeserializeObject<Department>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Test Department", newDepartment.Name);


                // Clean up after ourselves- delete department!
                deleteDepartment(newDepartment, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExistent_Department_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a department with an enormously huge Id
                HttpResponseMessage response = await client.GetAsync("api/department/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_Department()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new Department
                Department newDepartment = await createDepartment(client);

                // Make sure the info checks out
                Assert.Equal("Test Department", newDepartment.Name);



                // Clean up after ourselves - delete Department!
                deleteDepartment(newDepartment, client);
            }
        }



        [Fact]
        public async Task Test_Modify_Department()
        {

            // We're going to change a department's name! This is their new name.
            string newName = "Cool Department";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new department
                Department newDepartment = await createDepartment(client);

                // Change their first name
                newDepartment.Name = newName;

                // Convert them to JSON
                string modifiedDepartmentAsJSON = JsonConvert.SerializeObject(newDepartment);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/department/{newDepartment.Id}",
                    new StringContent(modifiedDepartmentAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // We should have gotten a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                // Try to GET the department we just edited
                HttpResponseMessage getDepartment = await client.GetAsync($"api/department/{newDepartment.Id}");
                getDepartment.EnsureSuccessStatusCode();

                string getDepartmentBody = await getDepartment.Content.ReadAsStringAsync();
                Department modifiedDepartment = JsonConvert.DeserializeObject<Department>(getDepartmentBody);

                Assert.Equal(HttpStatusCode.OK, getDepartment.StatusCode);

                // Make sure the name was in fact updated
                Assert.Equal(newName, modifiedDepartment.Name);

                // Clean up after ourselves- delete it
                deleteDepartment(modifiedDepartment, client);
            }
        }




    }
}