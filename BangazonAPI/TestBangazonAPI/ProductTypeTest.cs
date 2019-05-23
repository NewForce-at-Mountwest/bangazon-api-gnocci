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

    public class TestProductType
    {


        // Create a new producttype in the db and make sure we get a 200 OK status code back
        public async Task<ProductType> createProductType(HttpClient client)
        {
            ProductType thing = new ProductType
            {

                Name = "AwesomeProduct",
                IsArchived = false
            };
            string thingAsJSON = JsonConvert.SerializeObject(thing);


            HttpResponseMessage response = await client.PostAsync(
                "api/ProductType",
                new StringContent(thingAsJSON, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            ProductType newThing = JsonConvert.DeserializeObject<ProductType>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newThing;

        }
        // Delete a producttype in the database and make sure we get a no content status code back
        public async Task deleteThing(ProductType thing, HttpClient client)
        {
            //here we make harddelete = true so we can actually delete the new producttype from the db instead of archive it
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/ProductType/{thing.Id}?harddelete=true");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }


        //Test to make sure we can get every producttype in the db
        [Fact]
        public async Task Test_Get_All_ProductTypes()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our students; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/ProductType");


                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of student instances
                List<ProductType> productTypeList = JsonConvert.DeserializeObject<List<ProductType>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any producttypes in the list?
                Assert.True(productTypeList.Count > 0);
            }
        }

        //test to get single producttype by id
        [Fact]
        public async Task Test_Get_Single_ProductType()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new producttype
                ProductType newThing = await createProductType(client);

                // Try to get that producttype from the database
                HttpResponseMessage response = await client.GetAsync($"api/ProductType/{newThing.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                ProductType otherThing = JsonConvert.DeserializeObject<ProductType>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("AwesomeProduct", otherThing.Name);

                // delete the producttype so we don't over-populate our database
                deleteThing(newThing, client);
            }
        }

        //test ot make sure we don't get a producttype back when we put in a randomly high id that is not in the database
        [Fact]
        public async Task Test_Get_NonExitant_ProductType_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                HttpResponseMessage response = await client.GetAsync("api/ProductType/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        //Test to make sure we can create and delete a producttype
        [Fact]
        public async Task Test_Create_And_Delete_ProductType()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new producttype
                ProductType thing = await createProductType(client);

                // Make sure his info checks out
                Assert.Equal("AwesomeProduct", thing.Name);

                deleteThing(thing, client);
            }
        }

        //Test to make sure we can't delete a producttype that is not in the DB
        [Fact]
        public async Task Test_Delete_NonExistent_ProductType_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist in the DB
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/ProductType/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        //Test to edit a producttype
        [Fact]
        public async Task Test_Modify_ProductProduct()
        {

            // create a string to change the producttypes name
            string newName = "New ProductType Name";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // create new producttype
                ProductType newProductType = await createProductType(client);

                // Change the name
                newProductType.Name = newName;

                // Convert them to JSON
                string modifiedDavidAsJSON = JsonConvert.SerializeObject(newProductType);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/ProductType/{newProductType.Id}",
                    new StringContent(modifiedDavidAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // We should have gotten a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Try to GET the producttype we just edited
                HttpResponseMessage getProductType = await client.GetAsync($"api/ProductType/{newProductType.Id}");
                getProductType.EnsureSuccessStatusCode();

                string getProductTypeBody = await getProductType.Content.ReadAsStringAsync();
                ProductType modifiedProductType = JsonConvert.DeserializeObject<ProductType>(getProductTypeBody);

                Assert.Equal(HttpStatusCode.OK, getProductType.StatusCode);

                // make sure it was updated
                Assert.Equal(newName, modifiedProductType.Name);

                // DELETEEEEEEE
                deleteThing(modifiedProductType, client);
            }
        }

    }
}