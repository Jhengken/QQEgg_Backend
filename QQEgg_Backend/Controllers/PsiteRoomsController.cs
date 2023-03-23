using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QQEgg_Backend.DTO;
using QQEgg_Backend.Models;

namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PsiteRoomsController : ControllerBase
    {
        private readonly dbXContext _context;

        public PsiteRoomsController(dbXContext context)
        {
            _context = context;
        }

        // GET: api/PsiteRooms
        [HttpGet]
        public IEnumerable<PsiteRoomDTO> GetTPsiteRoom()
        {
            var result = _context.TPsiteRoom.Select(a => a);  
            
            return result.ToList().Select(a=> PRDTO(a));
        }

        // GET: api/PsiteRooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PsiteRoomDTO>> GetTPsiteRoom(int id)
        {
            var result = _context.TPsiteRoom
                .Where(a => a.RoomId == id).SingleOrDefault();
            if (result == null)
            {
                return NotFound();
            }

            return PRDTO(result);
        }

        // GET: api/GetCategory
        [HttpGet("Category")]
        public IEnumerable<CategoryDTO> GetCategory()
        {
            return _context.TCategory.Select(c => new CategoryDTO()
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
            });
        }

        private static PsiteRoomDTO PRDTO(TPsiteRoom room)
        {                      
            return new PsiteRoomDTO
            {
                RoomId = room.RoomId,
                CategoryId = room.CategoryId,
                HourPrice = room.HourPrice,
                DatePrice = room.DatePrice,
                Ping = room.Ping,
                Image = room.Image,
                Status = room.Status,
                RoomDescription = room.Description,
            };
        }
        // PUT: api/PsiteRooms/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTPsiteRoom(int id, TPsiteRoom tPsiteRoom)
        {
            if (id != tPsiteRoom.RoomId)
            {
                return BadRequest();
            }

            _context.Entry(tPsiteRoom).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TPsiteRoomExists(id))
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

        // POST: api/PsiteRooms
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TPsiteRoom>> PostTPsiteRoom(TPsiteRoom tPsiteRoom)
        {
          if (_context.TPsiteRoom == null)
          {
              return Problem("Entity set 'dbXContext.TPsiteRoom'  is null.");
          }
            _context.TPsiteRoom.Add(tPsiteRoom);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTPsiteRoom", new { id = tPsiteRoom.RoomId }, tPsiteRoom);
        }

        // DELETE: api/PsiteRooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTPsiteRoom(int id)
        {
            if (_context.TPsiteRoom == null)
            {
                return NotFound();
            }
            var tPsiteRoom = await _context.TPsiteRoom.FindAsync(id);
            if (tPsiteRoom == null)
            {
                return NotFound();
            }

            _context.TPsiteRoom.Remove(tPsiteRoom);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TPsiteRoomExists(int id)
        {
            return (_context.TPsiteRoom?.Any(e => e.RoomId == id)).GetValueOrDefault();
        }
    }
}
