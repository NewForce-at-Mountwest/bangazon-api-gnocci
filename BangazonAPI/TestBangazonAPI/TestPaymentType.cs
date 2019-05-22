
using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using TestStudentExercisesAPI;

namespace BangazonAPITest
{
    //Testing Payment Type:
    public class TestPaymentType
    {
        public async Task<PaymentType> createPaymentType(HttpClient client)
        {
            //Create A Payment Type:
            PaymentType newPaymentType = new PaymentType
            {
                AcctNumber = 123456789,
                Name = "Visa"
            };

            //Making it JSON:
            string paymentTypeAsJson = JsonConvert.SerializeObject(newPaymentType);

            //Ensure Successful Transfer to "api/paymentType":
            HttpResponseMessage response = await client.PostAsync(
                "api/paymentType",
                new StringContent(paymentTypeAsJson, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            PaymentType anotherPaymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return anotherPaymentType;

        }

        //Delete when Testing POST:
        public async Task deletePaymentType(PaymentType visa, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/paymentType/{visa.Id}");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }

        [Fact]

        //Test to Get Every Payment Type in the Database:
        public async Task Test_Get_All_PaymentTypes()
        {

            // Use HTTP Client:
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call Route to Get Every Payment Type and 'await' for a Response:
                HttpResponseMessage response = await client.GetAsync("api/paymentType");

                response.EnsureSuccessStatusCode();

                // Read Response Body as JSON:
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a List of Payment Type Instances:
                List<PaymentType> paymentTypeList = JsonConvert.DeserializeObject<List<PaymentType>>(responseBody);

                // 200 OK Status Code Generated?:
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Any Payment Type(s) in the List?:
                Assert.True(paymentTypeList.Count > 0);
            }
        }

        [Fact]

        //Single Test to Get Single Payment Type:
        public async Task Single_PaymentType_Test()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create New Payment Type:
                PaymentType newPaymentType = await createPaymentType(client);

                // Get [above] Payment Type from api/paymentType/
                HttpResponseMessage response = await client.GetAsync($"api/paymentType/{newPaymentType.Id}");

                response.EnsureSuccessStatusCode();

                // Turn Response into JSON:
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                PaymentType paymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                // Check to see if Response is == to Code:
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(123456789, newPaymentType.AcctNumber);
                Assert.Equal("Visa", newPaymentType.Name);

                // Delete the Payment Type:
                deletePaymentType(newPaymentType, client);
            }
        }

        [Fact]

        //Test to see if Error is Generated from a Id that does NOT Exist:
        public async Task Test_Get_NonExitant_Customer_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to Get a Payment Type with a Make-Believe ID:
                HttpResponseMessage response = await client.GetAsync("api/paymentType/987654321");

                // This should Result in a "204 No Content Error":
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        //Test to Create a Payment Type and Delete (Delete will actually Store in Function):
        [Fact]
        public async Task Test_Create_And_Delete_PaymentType()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create Test:
                PaymentType newPaymentType = await createPaymentType(client);

                // Info should Check Out:
                Assert.Equal(123456789, newPaymentType.AcctNumber);
                Assert.Equal("Visa", newPaymentType.Name);

                // Delete Test:
                deletePaymentType(newPaymentType, client);
            }
        }


        //Test to Delete Payment Type that does NOT Exist in the Database:
        [Fact]
        public async Task Test_Delete_NonExistent_PaymentType_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Delete an ID that does NOT Exist in the Database:
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/paymentType/9878987");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }


        [Fact]
        //Test Edit Payment Type:
        public async Task Test_Modify_PaymentType()
        {

            // Change Payment Type Account Number: 
            int newAcctNumber = 987654321;

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a NEW Payment Type:
                PaymentType newPaymentType = await createPaymentType(client);

                // Change their first name
                newPaymentType.AcctNumber = newAcctNumber;

                // Convert to JSON:
                string modifiedPaymentTypAsJSON = JsonConvert.SerializeObject(newPaymentType);

                // PUT Request with NEW Information:
                HttpResponseMessage response = await client.PutAsync(
                    $"api/paymentType/{newPaymentType.Id}",
                    new StringContent(modifiedPaymentTypAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert Response to JSON:
                string responseBody = await response.Content.ReadAsStringAsync();

                // "No Content" Status Code:
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                GET Section:
                */
                // Try to GET the Edited Payment Type:
                HttpResponseMessage getPaymentType = await client.GetAsync($"api/paymentType/{newPaymentType.Id}");
                getPaymentType.EnsureSuccessStatusCode();

                string getPaymentTypeBody = await getPaymentType.Content.ReadAsStringAsync();
                PaymentType modifiedPaymentType = JsonConvert.DeserializeObject<PaymentType>(getPaymentTypeBody);

                Assert.Equal(HttpStatusCode.OK, getPaymentType.StatusCode);

                // Make sure his name was in fact updated
                Assert.Equal(newAcctNumber, modifiedPaymentType.AcctNumber);

                // delete
                deletePaymentType(modifiedPaymentType, client);
            }
        }

    }
}
