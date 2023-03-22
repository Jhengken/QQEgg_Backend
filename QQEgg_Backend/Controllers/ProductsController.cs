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
                .ThenInclude(s => s.TPsiteRoom)
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
                        RoomDescription = room.Description,
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
                    SiteDescription = tmp.Description,
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
                .ThenInclude(s => s.TPsiteRoom)
                .Where(a => a.ProductId == id)
                .SingleOrDefault();

            if(result == null)
                return null!;
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
            TProducts product = new TProducts()
            {
                SupplierId=pd.SupplierId,
                Name = pd.Name,
            };
            _context.TProducts.Add(product);
            await _context.SaveChangesAsync();
            return "新增 Product 成功";
        }

        [HttpPost("PSite")]
        public async Task<string> PSitePost([FromBody] PsiteDTO pd)
        {
            TPsite psite = new TPsite()
            {
                ProductId = pd.ProductId,
                Name=pd.Name,
                Image=pd.Image,
                OpenTime=pd.OpenTime,
                Latitude=pd.Latitude,
                Longitude=pd.Longitude,
                Address=pd.Address,
                Description=pd.SiteDescription
            };
            _context.TPsite.Add(psite);
            await _context.SaveChangesAsync();
            return "新增 Site 成功";
        }

        [HttpPost("PSiteRoom")]
        public async Task<string> PSiteRoomPost([FromBody] PsiteRoomDTO pd)
        {
            TPsiteRoom psiteRoom = new TPsiteRoom()
            {
                SiteId = pd.SiteId,
                CategoryId = pd.CategoryId,
                HourPrice = pd.HourPrice,
                DatePrice = pd.DatePrice,
                Ping = pd.Ping,
                Image = pd.Image,
                Status = pd.Status,
                Description = pd.RoomDescription
            };
            _context.TPsiteRoom.Add(psiteRoom);
            await _context.SaveChangesAsync();
            return "新增 PSiteRoom 成功";
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<string> DeleteTProducts(int id)
        {
            if (_context.TProducts == null)
            {
                return "查無資料";
            }
            var tProducts = await _context.TProducts.FindAsync(id);
            if (tProducts == null)
            {
                return "查無資料";
            }

            _context.TProducts.Remove(tProducts);
            await _context.SaveChangesAsync();

            return "刪除 Product 成功";

        }

        [HttpDelete("PSite/{id}")]
        public async Task<string> DeleteTPSite(int id)
        {
            var psite = await _context.TPsite.FindAsync(id);
            if (psite == null)
            {
                return "查無資料";
            }

            _context.TPsite.Remove(psite);
            await _context.SaveChangesAsync();
            return "刪除 PSite 成功";
        }

        [HttpDelete("PSiteRoom/{id}")]
        public async Task<string> DeleteTPSiteRoom(int id)
        {
            var psiteroom = await _context.TPsiteRoom.FindAsync(id);
            if (psiteroom == null)
            {
                return "查無資料";
            }

            _context.TPsiteRoom.Remove(psiteroom);
            await _context.SaveChangesAsync();
            return "刪除 PSiteRoom 成功";
        }
        private bool TProductsExists(int id)
        {
            return (_context.TProducts?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
