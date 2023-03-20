using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QQEgg_Backend.DTO;
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
        public async Task<IEnumerable<SuppliersDTO>> GetTSuppliers()
        {
            return _context.TSuppliers.Select(sup => new SuppliersDTO
            {
                SupplierId = sup.SupplierId,
                Name = sup.Name,
                Email = sup.Email,
                Phone = sup.Phone,
                Password = sup.Password,
            });
        }

        // GET: api/TSuppliers/5
        [HttpGet("{id}")]
        public async Task<SuppliersDTO> GetTSuppliers(int id)
        {
            var tSuppliers = await _context.TSuppliers.FindAsync(id);

            if (tSuppliers == null)
            {
                return null;
            }

            return new SuppliersDTO
            {
                SupplierId = tSuppliers.SupplierId,
                Name = tSuppliers.Name,
                Email = tSuppliers.Email,
                Phone = tSuppliers.Phone,
                Password = tSuppliers.Password,
            };
        }

        // PUT: api/TSuppliers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<string> PutTSuppliers(int id, SuppliersDTO tSuppliers)
        {
            if (id != tSuppliers.SupplierId)
            {
                return "參數錯誤";
            }
            TSuppliers? sup = await _context.TSuppliers.FindAsync(tSuppliers.SupplierId);
            _context.Entry(sup).State = EntityState.Modified;
            sup.Name = tSuppliers.Name;
            sup.Phone = tSuppliers.Phone;
            sup.Email = tSuppliers.Email;
            sup.Password = tSuppliers.Password;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TSuppliersExists(id))
                {
                    return "記錄錯誤";
                }
                else
                {
                    throw;
                }
            }

            return "修改成功";
        }

        // POST: api/TSuppliers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<string> PostTSuppliers(SuppliersDTO tSuppliers)
        {
            TSuppliers sup = new TSuppliers
            {
                Name = tSuppliers.Name,
                Email = tSuppliers.Email,
                Phone = tSuppliers.Phone,
                Password = tSuppliers.Password,

            };

            _context.TSuppliers.Add(sup);
            await _context.SaveChangesAsync();

            return "註冊成功";
        }

        // DELETE: api/TSuppliers/5
        [HttpDelete("{id}")]
        public async Task<string> DeleteTSuppliers(int id)
        {
            var tSuppliers = await _context.TSuppliers.FindAsync(id);
            if (tSuppliers == null)
            {
                return "刪除失敗";
            }

            _context.TSuppliers.Remove(tSuppliers);
            await _context.SaveChangesAsync();

            return "刪除成功";
        }

        private bool TSuppliersExists(int id)
        {
            return (_context.TSuppliers?.Any(e => e.SupplierId == id)).GetValueOrDefault();
        }
    }
}
