using System;
using System.Collections.Generic;
using System.Linq;
using BangazonAPI.Data;
using BangazonAPI.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;


//Ticket #3: Allow Developers to Access Payment Type Resource

//# Feature Criteria:
// - `GET`
// - `POST`
// - `PUT`
// - `DELETE`
// - User should be able to GET a list, and GET a single item.

//## Testing Criteria
// - Write a testing class and test methods that validate the GET single, GET all, POST, PUT, and DELETE operations work as expected.


namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    public class PaymentTypeController : Controller
    {

    }
}