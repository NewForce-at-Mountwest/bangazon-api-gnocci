
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

    public class TestBangazonAPI
    {

        // Since we need to clean up after ourselves, we'll create and delete a student when we test POST and PUT
        // Otherwise, eveyr time we ran our test suite it would create a new David entry and we'd end up with a tooon of Davids

        // Create a new student in the db and make sure we get a 200 OK status code back
        public async Task<Product> createProduct(HttpClient client)
        {
            Product thing = new Product
            {   
                
                Price = 650,
                Title = "Bird Thing",
                Description = "fun toy",
                Quantity = 6,
                IsArchived = false,
                ProductTypeId = 1,
                CustomerId = 1
            };
            string thingAsJSON = JsonConvert.SerializeObject(thing);


            HttpResponseMessage response = await client.PostAsync(
                "api/Product",
                new StringContent(thingAsJSON, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Product newThing = JsonConvert.DeserializeObject<Product>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newThing;

        }

        // Delete a student in the database and make sure we get a no content status code back
        public async Task deleteThing(Product thing, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/Product/{thing.Id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_Products()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our students; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/Product");


                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of student instances
                List<Product> productList = JsonConvert.DeserializeObject<List<Product>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any students in the list?
                Assert.True(productList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Product()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new student
                Product newThing = await createProduct(client);

                // Try to get that student from the database
                HttpResponseMessage response = await client.GetAsync($"api/Product/{newThing.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Product otherThing = JsonConvert.DeserializeObject<Product>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(6, otherThing.Quantity);
                Assert.Equal(650, otherThing.Price);

                // Clean up after ourselves- delete david!
                deleteThing(newThing, client);
            }
        }

        //        [Fact]
        //        public async Task Test_Get_NonExitant_Student_Fails()
        //        {

        //            using (var client = new APIClientProvider().Client)
        //            {
        //                // Try to get a student with an enormously huge Id
        //                HttpResponseMessage response = await client.GetAsync("api/students/999999999");

        //                // It should bring back a 204 no content error
        //                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        //            }
        //        }


        //        [Fact]
        //        public async Task Test_Create_And_Delete_Student()
        //        {
        //            using (var client = new APIClientProvider().Client)
        //            {

        //                // Create a new David
        //                Student newDavid = await createDavid(client);

        //                // Make sure his info checks out
        //                Assert.Equal("David", newDavid.FirstName);
        //                Assert.Equal("Bird", newDavid.LastName);
        //                Assert.Equal("@david", newDavid.SlackHandle);

        //                // Clean up after ourselves - delete David!
        //                deleteDavid(newDavid, client);
        //            }
        //        }

        //        [Fact]
        //        public async Task Test_Delete_NonExistent_Student_Fails()
        //        {
        //            using (var client = new APIClientProvider().Client)
        //            {
        //                // Try to delete an Id that shouldn't exist in the DB
        //                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/students/600000");
        //                Assert.False(deleteResponse.IsSuccessStatusCode);
        //                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        //            }
        //        }

        //        [Fact]
        //        public async Task Test_Modify_Student()
        //        {

        //            // We're going to change a student's name! This is their new name.
        //            string newFirstName = "Super cool dude";

        //            using (HttpClient client = new APIClientProvider().Client)
        //            {

        //                // Create a new student
        //                Student newDavid = await createDavid(client);

        //                // Change their first name
        //                newDavid.FirstName = newFirstName;

        //                // Convert them to JSON
        //                string modifiedDavidAsJSON = JsonConvert.SerializeObject(newDavid);

        //                // Make a PUT request with the new info
        //                HttpResponseMessage response = await client.PutAsync(
        //                    $"api/students/{newDavid.Id}",
        //                    new StringContent(modifiedDavidAsJSON, Encoding.UTF8, "application/json")
        //                );


        //                response.EnsureSuccessStatusCode();

        //                // Convert the response to JSON
        //                string responseBody = await response.Content.ReadAsStringAsync();

        //                // We should have gotten a no content status code
        //                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        //                /*
        //                    GET section
        //                 */
        //                // Try to GET the student we just edited
        //                HttpResponseMessage getDavid = await client.GetAsync($"api/students/{newDavid.Id}");
        //                getDavid.EnsureSuccessStatusCode();

        //                string getDavidBody = await getDavid.Content.ReadAsStringAsync();
        //                Student modifiedDavid = JsonConvert.DeserializeObject<Student>(getDavidBody);

        //                Assert.Equal(HttpStatusCode.OK, getDavid.StatusCode);

        //                // Make sure his name was in fact updated
        //                Assert.Equal(newFirstName, modifiedDavid.FirstName);

        //                // Clean up after ourselves- delete him
        //                deleteDavid(modifiedDavid, client);
        //            }
        //        }
    }
}