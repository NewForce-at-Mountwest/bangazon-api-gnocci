
using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using TestBangazonAPI;

namespace TestBangazonAPI
{
    //NOTES FOR FUTURE REFERENCE -- I tried creating the customer then running the test and was getting back 'No test to run'
    //Here only thing I'm doing is creating the customer, not actually doing anything with it, I actually have to try to PUT/Post etc
    //To actually run the test, also the [FACT] is what marks it as test ?I think?

    //Testing Customer
    public class TestCustomer
    {
        public async Task<Customer> createCustomer(HttpClient client)
        {
            //Creating A new customer
            Customer newCustomer = new Customer
            {
                FirstName = "Larry",
                LastName = "Johnson"
            };
            //Making it Json-ify
            string customerAsJson = JsonConvert.SerializeObject(newCustomer);

            //Making sure it goes to api/customer
            HttpResponseMessage response = await client.PostAsync(
                "api/customer",
                new StringContent(customerAsJson, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Customer anotherCustomer = JsonConvert.DeserializeObject<Customer>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return anotherCustomer;

        }

        //Created to clean up after myself, don't actually have to have delete method for customers but created to delete when testing POST
        public async Task deleteCustomer(Customer larry, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/customer/{larry.Id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }

        [Fact]
        //Test to get every customer in the database -> Ran the first time and worked correectly.
        public async Task Test_Get_All_Customers()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get every customer and 'await' for a response
                HttpResponseMessage response = await client.GetAsync("api/customer");

                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of Customer instances
                List<Customer> customerList = JsonConvert.DeserializeObject<List<Customer>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any customers in the list?
                Assert.True(customerList.Count > 0);
            }
        }

        [Fact]
        //Single test to get single customer
        public async Task Single_Customer_Test()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new customer
                Customer newCustomer = await createCustomer(client);

                // Try to get that customer from api/customer/
                HttpResponseMessage response = await client.GetAsync($"api/customer/{newCustomer.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Customer customer = JsonConvert.DeserializeObject<Customer>(responseBody);

                // Check to see if our response is == to code Larry Johnson 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Larry", newCustomer.FirstName);
                Assert.Equal("Johnson", newCustomer.LastName);

                // Delete the customer
                deleteCustomer(newCustomer, client);
            }
        }

        [Fact]

        //Test to see if we get an error from a Id that does not exist
        public async Task Test_Get_NonExitant_Customer_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a customer with an enormously huge Id
                HttpResponseMessage response = await client.GetAsync("api/customer/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        //Test to create a customer & Delete, don't have delete method but added for future reference for myself
        [Fact]
        public async Task Test_Create_And_Delete_Customer()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new David
                Customer newCustomer = await createCustomer(client);

                // Make sure his info checks out
                Assert.Equal("Larry", newCustomer.FirstName);
                Assert.Equal("Johnson", newCustomer.LastName);

                // Clean up after ourselves - delete David!
                deleteCustomer(newCustomer, client);
            }
        }


        //Test to delete customer that does not exist in DB
        [Fact]
        public async Task Test_Delete_NonExistent_Customer_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist in the DB
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/customer/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }


        [Fact]
        //test to edit a customer
        public async Task Test_Modify_Customer()
        {

            // change the customers name 
            string newFirstName = "new larry";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new customer
                Customer newCustomer = await createCustomer(client);

                // Change their first name
                newCustomer.FirstName = newFirstName;

                // Convert them to JSON
                string modifiedCustomerAsJSON = JsonConvert.SerializeObject(newCustomer);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/customer/{newCustomer.Id}",
                    new StringContent(modifiedCustomerAsJSON, Encoding.UTF8, "application/json")
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
                HttpResponseMessage getCustomer = await client.GetAsync($"api/customer/{newCustomer.Id}");
                getCustomer.EnsureSuccessStatusCode();

                string getCustomerBody = await getCustomer.Content.ReadAsStringAsync();
                Customer modifiedCustomer = JsonConvert.DeserializeObject<Customer>(getCustomerBody);

                Assert.Equal(HttpStatusCode.OK, getCustomer.StatusCode);

                // Make sure his name was in fact updated
                Assert.Equal(newFirstName, modifiedCustomer.FirstName);

                // delete
                deleteCustomer(modifiedCustomer, client);
            }
        }

    }
}
