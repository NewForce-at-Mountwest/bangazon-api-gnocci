using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using BangazonAPI.Data;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


namespace BangazonAPI.Controllers
{

    //Ticket #3: Allow Developers to Access Payment Type Resource

    //# Feature Criteria:
    // - `GET`
    // - `POST`
    // - `PUT`
    // - `DELETE`
    // - User should be able to GET a list, and GET a single item.


//Class to GET all, POST, PUT, and DELETE payment type in Bamngazon API:
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypeController : Controller
    {
        //private readonly IConfiguration _config;

        //public PaymentTypeController(IConfiguration config)

        //Empty Variable _context that references Context[.cs] class:
        private Context _context;
        public PaymentTypeController(Context ctx)
        {
            _context = ctx;
            //_config = config;
        }

        //public SqlConnection Connection
        //{

        //    get
        //    {
        //        return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        //    }

        //}


        // GET a List, and GET a Single Item:
        [HttpGet]
        public IActionResult Get()
        {
            IQueryable<object> PaymentType = from paymentType in _context.PaymentType select paymentType;

            if (PaymentType == null)
            {
                return NotFound();
            }

            return Ok(PaymentType);

        }


        // GET a Single Payment Type by "Id":
        [HttpGet("{Id}", Name = "GetSinglePaymentType")]
        public IActionResult Get([FromRoute] int Id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                PaymentType PaymentType = _context.PaymentType.Single(m => m.Id == Id);

                if (PaymentType == null)
                {
                    return NotFound();
                }

                return Ok(PaymentType);
            }
            catch (System.InvalidOperationException ex)
            {
                return NotFound(ex);
            }
        }


        // POST New Payment Type:
        [HttpPost]
        public IActionResult Post([FromBody] PaymentType newPaymentType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.PaymentType.Add(newPaymentType);

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (PaymentTypeExists(newPaymentType.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetSinglePaymentType", new { id = newPaymentType.Id }, newPaymentType);
        }


        // "HELPER METHOD" checks if PaymentType Exists in the Database:
        private bool PaymentTypeExists(int Id)
        {
            return _context.PaymentType.Count(e => e.Id == Id) > 0;
        }


        // EDIT Payment Type Object:
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] PaymentType modifiedPaymentType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != modifiedPaymentType.Id)
            {
                return BadRequest();
            }

            _context.Entry(modifiedPaymentType).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }


        // DELETE a Payment Type:
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PaymentType paymentType = _context.PaymentType.Single(m => m.Id == id);
            if (paymentType == null)
            {
                return NotFound();
            }

            _context.PaymentType.Remove(paymentType);
            _context.SaveChanges();

            return Ok(paymentType);
        }
    }
}