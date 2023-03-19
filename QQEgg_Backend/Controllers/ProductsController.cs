using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using QQEgg_Backend.DTO;
using QQEgg_Backend.Models;


namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly dbXContext _context;

        public ProductsController(dbXContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public IEnumerable<ProductsDTO> GetTProducts()
        {
            var products = _context.TProducts
                .Include(a => a.Supplier)
                .Include(a => a.TPsite)
                .Select(a => a);


            return products.ToList().Select(a => ToDTO(a));
        }
      
        //資料傳遞靜態函式
        private static ProductsDTO ToDTO(TProducts product)
        {
            List<PsiteDTO> psiteDTOs = new List<PsiteDTO>();
                        
            foreach (var tmp in product.TPsite) 
            {
                List<PsiteRoomDTO> psiteRoomDTOs = new List<PsiteRoomDTO>();
                foreach (var room in tmp.TPsiteRoom)
                {
                    PsiteRoomDTO prto = new PsiteRoomDTO
                    {
                        RoomId = room.RoomId,
                        CategoryId = room.CategoryId,
                        HourPrice = room.HourPrice,
                        DatePrice = room.DatePrice,
                        Ping = room.Ping,
                        Image = room.Image,
                        Status = room.Status,
                        Description = room.Description,
                    };
                    psiteRoomDTOs.Add(prto);
                }
                PsiteDTO pdto = new PsiteDTO
                {
                    SiteId = tmp.SiteId,
                    Name = tmp.Name,
                    Image = tmp.Image,
                    OpenTime = tmp.OpenTime,
                    Latitude = tmp.Latitude,
                    Longitude = tmp.Longitude,
                    Address = tmp.Address,
                    Description = tmp.Description,
                    PsiteRoom = psiteRoomDTOs,
                };
                psiteDTOs.Add(pdto);
            }
            return new ProductsDTO
            {
                ProductId = product.ProductId,
                SupplierId = product.SupplierId,
                Name = product.Name,
                Supplier = new SupplierDTO
                {
                    //SupplierId = a.Supplier.SupplierId,
                    Address = product.Supplier.Address,
                    Name = product.Supplier.Name,
                    Phone = product.Supplier.Phone,
                    Email = product.Supplier.Email,
                },
                Psite = psiteDTOs,
            };
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductsDTO>> GetTProducts(int id)
        {
           
            var result = _context.TProducts
                .Include(a => a.Supplier)
                .Include(a => a.TPsite)
                .Where(a => a.ProductId == id)
                .SingleOrDefault();

            return ToDTO(result);
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTProducts(int id, TProducts tProducts)
        {
            if (id != tProducts.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(tProducts).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TProductsExists(id))
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

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<string> Post([FromBody]ProductsDTO pd)
        {


            return "ok";
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTProducts(int id)
        {
            if (_context.TProducts == null)
            {
                return NotFound();
            }
            var tProducts = await _context.TProducts.FindAsync(id);
            if (tProducts == null)
            {
                return NotFound();
            }

            _context.TProducts.Remove(tProducts);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TProductsExists(int id)
        {
            return (_context.TProducts?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
