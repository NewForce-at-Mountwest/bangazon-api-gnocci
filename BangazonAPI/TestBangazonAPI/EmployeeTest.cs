
using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using BangazonAPI;

namespace TestBangazonAPI
{

    public class EmployeeTestBangazonAPI
    {

        // Since we need to clean up after ourselves, we'll create and delete a student when we test POST and PUT
        // Otherwise, eveyr time we ran our test suite it would create a new David entry and we'd end up with a tooon of Davids

        // Create a new student in the db and make sure we get a 200 OK status code back
        public async Task<Employee> createEmployee(HttpClient client)
        {
            Employee person = new Employee
            {

                FirstName = "Test",
                LastName = "Person",
                DepartmentId = 2,
                IsSuperVisor = false
                
            };
            string thingAsJSON = JsonConvert.SerializeObject(person);


            HttpResponseMessage response = await client.PostAsync(
                "api/Employee",
                new StringContent(thingAsJSON, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Employee newEmployee = JsonConvert.DeserializeObject<Employee>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newEmployee;

        }

        // Delete a student in the database and make sure we get a no content status code back
        public async Task deleteEmployee(Employee employee, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/Employee/{employee.Id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_Employees()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our students; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/Employee");


                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of student instances
                List<Employee> employeeList = JsonConvert.DeserializeObject<List<Employee>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any students in the list?
                Assert.True(employeeList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Employee()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Creaete a new student
                Employee newEmployee = await createEmployee(client);

                // Try to get that student from the database
                HttpResponseMessage response = await client.GetAsync($"api/Employee/{newEmployee.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Employee employee = JsonConvert.DeserializeObject<Employee>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Test", employee.FirstName);
                Assert.Equal("Person", employee.LastName);

                // Clean up after ourselves- delete david!
                deleteEmployee(newEmployee, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_Employee_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a student with an enormously huge Id
                HttpResponseMessage response = await client.GetAsync("api/Employee/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_Employee()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new Employee
                Employee person = await createEmployee(client);

                // Make sure his info checks out
                Assert.Equal("Test", person.FirstName);
                Assert.Equal("Person", person.LastName);
                Assert.Equal(false, person.IsSuperVisor);
                 
                // Clean up after ourselves - delete new Employee!
                deleteEmployee(person, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Employee_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist in the DB
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/Employee/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Employee()
        {

            // We're going to change a student's name! This is their new name.
            string newFirstName = "Test Edit";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new student
                Employee newEmployee = await createEmployee(client);

                // Change their first name
                newEmployee.FirstName = newFirstName;

                // Convert them to JSON
                string modifiedEmployeeAsJSON = JsonConvert.SerializeObject(newEmployee);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/Employee/{newEmployee.Id}",
                    new StringContent(modifiedEmployeeAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // We should have gotten a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                // Try to GET the student we just edited
                HttpResponseMessage getEmployee = await client.GetAsync($"api/Employee/{newEmployee.Id}");
                getEmployee.EnsureSuccessStatusCode();

                string getEmployeeBody = await getEmployee.Content.ReadAsStringAsync();
                Employee modifiedEmployee = JsonConvert.DeserializeObject<Employee>(getEmployeeBody);

                Assert.Equal(HttpStatusCode.OK, getEmployee.StatusCode);

                // Make sure his name was in fact updated
                Assert.Equal(newFirstName, modifiedEmployee.FirstName);

                // Clean up after ourselves- delete him
                deleteEmployee(modifiedEmployee, client);
            }
        }
    }
}