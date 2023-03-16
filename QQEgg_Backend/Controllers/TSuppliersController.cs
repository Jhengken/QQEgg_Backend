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
    public class TSuppliersController : ControllerBase
    {
        private readonly dbXContext _context;

        public TSuppliersController(dbXContext context)
        {
            _context = context;
        }

        // GET: api/TSuppliers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TSuppliers>>> GetTSuppliers()
        {
          if (_context.TSuppliers == null)
          {
              return NotFound();
          }
            return await _context.TSuppliers.ToListAsync();
        }

        // GET: api/TSuppliers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TSuppliers>> GetTSuppliers(int id)
        {
          if (_context.TSuppliers == null)
          {
              return NotFound();
          }
            var tSuppliers = await _context.TSuppliers.FindAsync(id);

            if (tSuppliers == null)
            {
                return NotFound();
            }

            return tSuppliers;
        }

        // PUT: api/TSuppliers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTSuppliers(int id, TSuppliers tSuppliers)
        {
            if (id != tSuppliers.SupplierId)
            {
                return BadRequest();
            }

            _context.Entry(tSuppliers).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TSuppliersExists(id))
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

        // POST: api/TSuppliers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TSuppliers>> PostTSuppliers(TSuppliers tSuppliers)
        {
          if (_context.TSuppliers == null)
          {
              return Problem("Entity set 'dbXContext.TSuppliers'  is null.");
          }
            _context.TSuppliers.Add(tSuppliers);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTSuppliers", new { id = tSuppliers.SupplierId }, tSuppliers);
        }

        // DELETE: api/TSuppliers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTSuppliers(int id)
        {
            if (_context.TSuppliers == null)
            {
                return NotFound();
            }
            var tSuppliers = await _context.TSuppliers.FindAsync(id);
            if (tSuppliers == null)
            {
                return NotFound();
            }

            _context.TSuppliers.Remove(tSuppliers);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TSuppliersExists(int id)
        {
            return (_context.TSuppliers?.Any(e => e.SupplierId == id)).GetValueOrDefault();
        }
    }
}
