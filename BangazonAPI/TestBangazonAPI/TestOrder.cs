
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

namespace BangazonAPITest
{
    //NOTES FOR FUTURE REFERENCE -- I tried creating the customer then running the test and was getting back 'No test to run'
    //Here only thing I'm doing is creating the customer, not actually doing anything with it, I actually have to try to PUT/Post etc
    //To actually run the test, also the [FACT] is what marks it as test ?I think?

    //Testing Customer
    public class TestOrder
    {
        public async Task<Order> createOrder(HttpClient client)
        {
            //Creating A new customer
            Order newOrder = new Order
            {
                PaymentTypeId = 5,
                CustomerId = 5
            };
            //Making it Json-ify
            string orderAsJson = JsonConvert.SerializeObject(newOrder);

            //Making sure it goes to api/customer
            HttpResponseMessage response = await client.PostAsync(
                "api/order",
                new StringContent(orderAsJson, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Order anotherOrder = JsonConvert.DeserializeObject<Order>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return anotherOrder;

        }

        //Created to clean up after myself, don't actually have to have delete method for customers but created to delete when testing POST
        public async Task deleteOrder(Order order, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/order/{order.Id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }

        [Fact]
        //Test to get every customer in the database -> Ran the first time and worked correectly.
        public async Task Test_Get_All_Orders()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get every customer and 'await' for a response
                HttpResponseMessage response = await client.GetAsync("api/order");

                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of Customer instances
                List<Order> orderList = JsonConvert.DeserializeObject<List<Order>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any customers in the list?
                Assert.True(orderList.Count > 0);
            }
        }
        [Fact]
        //Single test to get single customer
        public async Task Single_Order_Test()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new customer
                Order newOrder = await createOrder(client);

                // Try to get that customer from api/customer/
                HttpResponseMessage response = await client.GetAsync($"api/order/{newOrder.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Order order = JsonConvert.DeserializeObject<Order>(responseBody);

                // Check to see if our response is == to code Larry Johnson 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(5, newOrder.PaymentTypeId);
                Assert.Equal(5, newOrder.CustomerId);

                // Delete the customer
                deleteOrder(newOrder, client);
            }
        }

        [Fact]

        //Test to see if we get an error from a Id that does not exist
        public async Task Test_Get_NonExitant_Order_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a customer with an enormously huge Id
                HttpResponseMessage response = await client.GetAsync("api/order/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        //Test to create a customer & Delete, don't have delete method but added for future reference for myself
        [Fact]
        public async Task Test_Create_And_Delete_Order()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new David
                Order newOrder = await createOrder(client);

                // Make sure his info checks out
                Assert.Equal(5, newOrder.PaymentTypeId);
                Assert.Equal(5, newOrder.CustomerId);

                // Clean up after ourselves - delete David!
                deleteOrder(newOrder, client);
            }
        }


        //Test to delete customer that does not exist in DB
        [Fact]
        public async Task Test_Delete_NonExistent_Order_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist in the DB
                HttpResponseMessage deleteResponse = await client.DeleteAsync("api/order/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }


        [Fact]
        //test to edit a customer
        public async Task Test_Modify_Order()
        {

            // change the customers name 
            int newPaymentType = 3;

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new customer
                Order newOrder = await createOrder(client);

                // Change their first name
                newOrder.PaymentTypeId = newPaymentType;

                // Convert them to JSON
                string modifiedOrderAsJSON = JsonConvert.SerializeObject(newOrder);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/order/{newOrder.Id}",
                    new StringContent(modifiedOrderAsJSON, Encoding.UTF8, "application/json")
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
                HttpResponseMessage getOrder = await client.GetAsync($"api/order/{newOrder.Id}");
                getOrder.EnsureSuccessStatusCode();

                string getOrderBody = await getOrder.Content.ReadAsStringAsync();
                Order modifiedOrder = JsonConvert.DeserializeObject<Order>(getOrderBody);

                Assert.Equal(HttpStatusCode.OK, getOrder.StatusCode);

                // Make sure his name was in fact updated
                Assert.Equal(newPaymentType, modifiedOrder.PaymentTypeId);

                // delete
                deleteOrder(modifiedOrder, client);
            }
        }

    }
}
