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
            var result = _context.TPsiteRoom.Include(a=>a.Site).Include(a=>a.Category)
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
                productId = room.Site.ProductId,
                SiteId = room.SiteId,
                CategoryId = room.CategoryId,
                CategoryName = room.Category.Name,
                HourPrice = room.HourPrice,
                DatePrice = room.DatePrice,
                Ping = room.Ping,
                Image = room.Image,
                Status = room.Status,
                RoomDescription = room.Description,
                OpenTime = room.Site.OpenTime,
                Iframe =room.Iframe
            };
        }
        // PUT: api/PsiteRooms/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public string  PutTPsiteRoom([FromBody] PsiteRoomDTO pd)
        {
            var existingPsiteRoom = _context.TPsiteRoom.SingleOrDefault(r => r.RoomId == pd.RoomId);

            if (existingPsiteRoom != null)
            {
                // Update properties for the existing PsiteRoom
                
                
                existingPsiteRoom.CategoryId = pd.CategoryId;
                existingPsiteRoom.HourPrice = pd.HourPrice;
                existingPsiteRoom.DatePrice = pd.DatePrice;
                existingPsiteRoom.Ping = pd.Ping;
                existingPsiteRoom.Image = pd.Image;
                existingPsiteRoom.Status = pd.Status;
                existingPsiteRoom.Description = pd.RoomDescription;              
                existingPsiteRoom.RoomPassWord = pd.RoomPassWord;
                existingPsiteRoom.Iframe = pd.Iframe;
                // ... update other properties as needed

                _context.SaveChanges();
                return "ok";
            }
            else
            {
                return "error";
            }
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
