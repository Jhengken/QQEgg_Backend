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
    public class TCouponsController : ControllerBase
    {
        private readonly dbXContext _context;

        public TCouponsController(dbXContext context)
        {
            _context = context;
        }

        // GET: api/TCoupons
        [HttpGet]
        public async Task<IEnumerable<CouponsDTO>> GetTCoupons()
        {
          //if (_context.TCoupons == null)
          //{
          //    return NotFound();
          //}
            return _context.TCoupons.Select(cou=> new CouponsDTO {
                CouponId= cou.CouponId,
                Code= cou.Code,
                Discount= cou.Discount,
                ExpiryDate= cou.ExpiryDate,
            });
        }

        // GET: api/TCoupons/5
        [HttpGet("{id}")]
        public async Task<CouponsDTO> GetTCoupons(int id)
        {
          //if (_context.TCoupons == null)
          //{
          //    return NotFound();
          //}
            var tCoupons = await _context.TCoupons.FindAsync(id);

            if (tCoupons == null)
            {
                return null;
            }

            return new CouponsDTO { 
                CouponId= tCoupons.CouponId,
                Code= tCoupons.Code,
                Discount= tCoupons.Discount,
                ExpiryDate= tCoupons.ExpiryDate,
            };
        }

        // PUT: api/TCoupons/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")] //好像有錯??
        public async Task<string> PutTCoupons(int id, CouponsDTO tCoupons)
        {
            if (id != tCoupons.CouponId)
            {
                return "優惠券參數錯誤";
            }

            TCoupons? cou = await _context.TCoupons.FindAsync(tCoupons.CouponId);
            cou.CouponId= tCoupons.CouponId;
            cou.Code= tCoupons.Code;
            cou.Discount= tCoupons.Discount;
            cou.ExpiryDate = tCoupons.ExpiryDate;
            _context.Entry(cou).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TCouponsExists(id))
                {
                    return "優惠券不存在";
                }
                else
                {
                    throw;
                }
            }

            return "優惠券修改成功";
        }

        // POST: api/TCoupons
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<string> PostTCoupons(CouponsDTO tCoupons)
        {
            //if (_context.TCoupons == null)
            //{
            //    return Problem("Entity set 'dbXContext.TCoupons'  is null.");
            //}

            TCoupons cou = new TCoupons
            {
                //CouponId = tCoupons.CouponId, //Id是自動產生不能填
                Code = tCoupons.Code,
                Discount = tCoupons.Discount,
                ExpiryDate = tCoupons.ExpiryDate,
            };
            _context.TCoupons.Add(cou);
            await _context.SaveChangesAsync();

            return "優惠券新增成功";
        }

        // DELETE: api/TCoupons/5
        [HttpDelete("{id}")]
        public async Task<string> DeleteTCoupons(int id)
        {
            //if (_context.TCoupons == null)
            //{
            //    return NotFound();
            //}
            var tCoupons = await _context.TCoupons.FindAsync(id);
            if (tCoupons == null)
            {
                return "優惠券刪除失敗";
            }

            _context.TCoupons.Remove(tCoupons);
            await _context.SaveChangesAsync();

            return "優惠券刪除成功";
        }

        private bool TCouponsExists(int id)
        {
            return (_context.TCoupons?.Any(e => e.CouponId == id)).GetValueOrDefault();
        }
    }
}
