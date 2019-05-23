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

namespace BangazonAPITest
{

    public class TestComputer
    {


        // Create a new Computer in the db and make sure we get a 200 OK status code back
        public async Task<Computer> CreateComputer(HttpClient client)
        {
            Computer computer = new Computer
            {
                PurchaseDate = new System.DateTime(2015, 09, 12, 5, 42, 22),

                DecomissionDate = new System.DateTime(2015, 09, 12, 5, 42, 22),

                Make = "MacBook Bro",

                Manufacturer = "Orange",

                isArchived = false
    };
            string thingAsJSON = JsonConvert.SerializeObject(computer);


            HttpResponseMessage response = await client.PostAsync(
                "api/Computer",
                new StringContent(thingAsJSON, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Computer newComputer = JsonConvert.DeserializeObject<Computer>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newComputer;

        }
        // Delete a Computer in the database and make sure we get a no content status code back
        public async Task deleteComputer(Computer computer, HttpClient client)
        {
            //here we make harddelete = true so we can actually delete the new Computer from the db instead of archive it
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/Computer/{computer.Id}?harddelete=true");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        //Test to make sure we can get every Computer in the db
        [Fact]
        public async Task Test_Get_All_Computers()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our students; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/Computer");


                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of student instances
                List<Computer> ComputerList = JsonConvert.DeserializeObject<List<Computer>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any Computers in the list?
                Assert.True(ComputerList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Computer()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new Computer
                Computer newComputer = await CreateComputer(client);

                // Try to get that Computer from the database
                HttpResponseMessage response = await client.GetAsync($"api/Computer/{newComputer.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Computer otherComputer = JsonConvert.DeserializeObject<Computer>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("MacBook Bro", otherComputer.Make);

                // delete the Computer so we don't over-populate our database
                deleteComputer(newComputer, client);
            }
        }

        //test ot make sure we don't get a Computer back when we put in a randomly high id that is not in the database
        [Fact]
        public async Task Test_Get_NonExitant_Computer_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                HttpResponseMessage response = await client.GetAsync("api/Computer/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        //Test to make sure we can create and delete a Computer
        [Fact]
        public async Task Test_Create_And_Delete_Computer()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new Computer
                Computer Computer = await CreateComputer(client);

                // Make sure his info checks out
                Assert.Equal("MacBook Bro", Computer.Make);

                deleteComputer(Computer, client);
            }
        }

        //Test to make sure we can't delete a Computer that is not in the DB
        [Fact]
        public async Task Test_Delete_NonExistent_Computer_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist in the DB
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/Computer/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        //Test to edit a Computer
        [Fact]
        public async Task Test_Modify_Computer()
        {

            // create a string to change the Computers name
            string newMake = "MacBook BROS";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // create new Computer
                Computer newcomputer = await CreateComputer(client);

                // Change the name
                newcomputer.Make = newMake;

                // Convert them to JSON
                string modifiedComputerAsJSON = JsonConvert.SerializeObject(newcomputer);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/Computer/{newcomputer.Id}",
                    new StringContent(modifiedComputerAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // We should have gotten a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Try to GET the Computer we just edited
                HttpResponseMessage getComputer = await client.GetAsync($"api/Computer/{newcomputer.Id}");
                getComputer.EnsureSuccessStatusCode();

                string getComputerbody = await getComputer.Content.ReadAsStringAsync();
                Computer modifiedComputer = JsonConvert.DeserializeObject<Computer>(getComputerbody);

                Assert.Equal(HttpStatusCode.OK, getComputer.StatusCode);

                // make sure it was updated
                Assert.Equal(newMake, modifiedComputer.Make);

                // DELETEEEEEEE
                deleteComputer(modifiedComputer, client);
            }
        }

    }
}
