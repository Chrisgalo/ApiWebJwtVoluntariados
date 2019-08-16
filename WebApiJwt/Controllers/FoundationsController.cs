using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using WebApiJwt.Models;

namespace WebApiJwt.Controllers
{
    [Authorize]
    public class FoundationsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Foundations
        [ActionName("GET")]
        public IQueryable<Foundation> GetFoundations()
        {
            return db.Foundations;
        }

        // GET: api/Foundations/5
        [ActionName("GET/id")]
        [ResponseType(typeof(Foundation))]
        public async Task<IHttpActionResult> GetFoundation(int id)
        {
            Foundation foundation = await db.Foundations.FindAsync(id);
            if (foundation == null)
            {
                return NotFound();
            }

            return Ok(foundation);
        }

        // PUT: api/Foundations/5
        [ActionName("PUT")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutFoundation(int id, Foundation foundation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != foundation.idFoundation)
            {
                return BadRequest();
            }

            db.Entry(foundation).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoundationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Foundations
        [ActionName("POST")]
        [ResponseType(typeof(Foundation))]
        public async Task<IHttpActionResult> PostFoundation(Foundation foundation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Foundations.Add(foundation);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = foundation.idFoundation }, foundation);
        }

        // DELETE: api/Foundations/5
        [ActionName("DELETE")]
        [ResponseType(typeof(Foundation))]
        public async Task<IHttpActionResult> DeleteFoundation(int id)
        {
            Foundation foundation = await db.Foundations.FindAsync(id);
            if (foundation == null)
            {
                return NotFound();
            }

            db.Foundations.Remove(foundation);
            await db.SaveChangesAsync();

            return Ok(foundation);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool FoundationExists(int id)
        {
            return db.Foundations.Count(e => e.idFoundation == id) > 0;
        }
    }
}