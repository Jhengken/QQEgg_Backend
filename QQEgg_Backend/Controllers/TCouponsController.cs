using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QQEgg_Backend.Models;

namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TCouponsController : ControllerBase
    {
        private readonly dbXContext _context;

        public TCouponsController(dbXContext context)
        {
            _context = context;
        }

        // GET: api/TCoupons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TCoupons>>> GetTCoupons()
        {
          if (_context.TCoupons == null)
          {
              return NotFound();
          }
            return await _context.TCoupons.ToListAsync();
        }

        // GET: api/TCoupons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TCoupons>> GetTCoupons(int id)
        {
          if (_context.TCoupons == null)
          {
              return NotFound();
          }
            var tCoupons = await _context.TCoupons.FindAsync(id);

            if (tCoupons == null)
            {
                return NotFound();
            }

            return tCoupons;
        }

        // PUT: api/TCoupons/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTCoupons(int id, TCoupons tCoupons)
        {
            if (id != tCoupons.CouponId)
            {
                return BadRequest();
            }

            _context.Entry(tCoupons).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TCouponsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TCoupons
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TCoupons>> PostTCoupons(TCoupons tCoupons)
        {
          if (_context.TCoupons == null)
          {
              return Problem("Entity set 'dbXContext.TCoupons'  is null.");
          }
            _context.TCoupons.Add(tCoupons);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTCoupons", new { id = tCoupons.CouponId }, tCoupons);
        }

        // DELETE: api/TCoupons/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTCoupons(int id)
        {
            if (_context.TCoupons == null)
            {
                return NotFound();
            }
            var tCoupons = await _context.TCoupons.FindAsync(id);
            if (tCoupons == null)
            {
                return NotFound();
            }

            _context.TCoupons.Remove(tCoupons);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TCouponsExists(int id)
        {
            return (_context.TCoupons?.Any(e => e.CouponId == id)).GetValueOrDefault();
        }
    }
}
