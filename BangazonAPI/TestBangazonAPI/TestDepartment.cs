
using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TestStudentExercisesAPI;

//TEST Payment Type:
namespace BangazonAPITest
{
    public class TestPaymentType
    {
        // CREATE New Payment Type in the Database; Return a 200 OK Status Code:
        public async Task<PaymentType> createPaymentType(HttpClient client)
        {
            PaymentType paymentType = new PaymentType
            {
                AcctNumber = 123456,
                Name = "Test Payment Type",
                CustomerId = 1,
                IsArchived = false
            };
            string paymentTypeAsJSON = JsonConvert.SerializeObject(paymentType);
            HttpResponseMessage response = await client.PostAsync(
                "api/paymentType",
                new StringContent(paymentTypeAsJSON, Encoding.UTF8, "application/json")
            );
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            PaymentType newPaymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return newPaymentType;
        }

        // DELETE Payment Type in the Database; Return a No Content Status Code:
        public async Task deletePaymentType(PaymentType paymentType, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/paymentType/{paymentType.Id}?HardDelete=true");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [Fact]
        //TEST to GET Payment Types in the Database:
        public async Task Test_Get_All_PaymentTypes()
        {
            // Use HTTP Client:
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Call Route to GET ALL Payment Types; 
                //Wait for RESPONSE Object (GET):
                HttpResponseMessage response = await client.GetAsync("api/paymentType");
                // RESPONSE Comes Back:
                response.EnsureSuccessStatusCode();
                // Read RESPONSE Body (as JSON):
                string responseBody = await response.Content.ReadAsStringAsync();
                // Convert JSON to a List of Payment Type(s):
                List<PaymentType> paymentTypeList = JsonConvert.DeserializeObject<List<PaymentType>>(responseBody);
                // 200 OK Status Code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                // Payment Types in List?
                Assert.True(paymentTypeList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_PaymentType()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                // CREATE New Payment Type:
                PaymentType newPaymentType = await createPaymentType(client);
                // GET Payment Type from Database:
                HttpResponseMessage response = await client.GetAsync($"api/paymentType/{newPaymentType.Id}");
                response.EnsureSuccessStatusCode();
                // Turn RESPONSE into JSON:
                string responseBody = await response.Content.ReadAsStringAsync();
                // Turn JSON into C Sharp:
                PaymentType paymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);
                // Return Expected RESPONSE? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Test Payment Type", newPaymentType.Name);
                // Delete Payment Type:
                deletePaymentType(newPaymentType, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_PaymentType_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // GET Payment Type with Huge ID:
                HttpResponseMessage response = await client.GetAsync("api/paymentType/666666666");
                // Return 204 No Content Error:
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Create_And_Delete_PaymentType()
        {
            using (var client = new APIClientProvider().Client)
            {
                // CREATE New Payment Type:
                PaymentType newPaymentType = await createPaymentType(client);
                // Confirm Information:
                Assert.Equal("Test Payment Type", newPaymentType.Name);
                // DELETE Payment Type:
                deletePaymentType(newPaymentType, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_PaymentType_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // DELETE an ID that should NOT Exist in the Database:
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/paymentType/123321");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_PaymentType()
        {
            // CHANGE a Payment Type's Name (New Name):
            string newName = "Jordan Castelloe's IQ is 210!";

            using (HttpClient client = new APIClientProvider().Client)
            {
                // CREATE New Payment Type:
                PaymentType newPaymentType = await createPaymentType(client);
                // CHANGE Name:
                newPaymentType.Name = newName;
                // CONVERT to JSON:
                string modifiedPaymentTypeAsJSON = JsonConvert.SerializeObject(newPaymentType);
                // PUT Request with New Information:
                HttpResponseMessage response = await client.PutAsync($"api/paymentType/{newPaymentType.Id}",
                  new StringContent(modifiedPaymentTypeAsJSON, Encoding.UTF8, "application/json")
                );
                response.EnsureSuccessStatusCode();
                // CONVERT Response to JSON:
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return No Content Status Code:
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                /*GET Section*/
                // GET the Payment Type Previously Edited:
                HttpResponseMessage getPaymentType = await client.GetAsync($"api/paymentType/{newPaymentType.Id}");
                getPaymentType.EnsureSuccessStatusCode();
                string getPaymentTypeBody = await getPaymentType.Content.ReadAsStringAsync();
                PaymentType modifiedPaymentType = JsonConvert.DeserializeObject<PaymentType>(getPaymentTypeBody);
                Assert.Equal(HttpStatusCode.OK, getPaymentType.StatusCode);
                // CONFIRM Update:
                Assert.Equal(newName, modifiedPaymentType.Name);
                // DELETE Modified Payment Type:
                deletePaymentType(modifiedPaymentType, client);
            }
        }
    }
}
