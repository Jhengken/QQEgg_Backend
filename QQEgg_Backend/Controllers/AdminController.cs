using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QQEgg_Backend.Models;

namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly dbXContext _dbXContext;

        public AdminController(dbXContext dbXContext)
        {
            _dbXContext = dbXContext;
        }

       //GET: api/TCoupons
       /// <summary>
       /// 顧客打開優惠卷使用列出顧客的優惠卷，如果沒有則顯示空白
       /// </summary>
       /// <returns></returns>
       [HttpGet]
        public async Task<ActionResult<IEnumerable<TCoupons>>> GetCustomerTCoupons()
        {
            var customerId = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var customer = await _dbXContext.TCustomers.FindAsync(customerId);

            //判斷顧客的帳號和黑名單
            if (customerId != null && customer.BlackListed == true)
            {
                //如果有且不是黑名單，秀出可以使用的優惠卷

                // 從資料庫中獲取該顧客的優惠卷資料


            }
            else
            {
                //直接顯示登入畫面
                return Unauthorized();
            }

        }

        // GET: api/Admin/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TCoupons>> GetTCoupons(int id)
        {
          if (_dbXContext.TCoupons == null)
          {
              return NotFound();
          }
            var tCoupons = await _dbXContext.TCoupons.FindAsync(id);

            if (tCoupons == null)
            {
                return NotFound();
            }

            return tCoupons;
        }

        // PUT: api/Admin/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTCoupons(int id, TCoupons tCoupons)
        {
            if (id != tCoupons.CouponId)
            {
                return BadRequest();
            }

            _dbXContext.Entry(tCoupons).State = EntityState.Modified;

            try
            {
                await _dbXContext.SaveChangesAsync();
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

        // POST: api/Admin
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TCoupons>> PostTCoupons(TCoupons tCoupons)
        {
          if (_dbXContext.TCoupons == null)
          {
              return Problem("Entity set 'dbXContext.TCoupons'  is null.");
          }
            _dbXContext.TCoupons.Add(tCoupons);
            await _dbXContext.SaveChangesAsync();

            return CreatedAtAction("GetTCoupons", new { id = tCoupons.CouponId }, tCoupons);
        }

        // DELETE: api/Admin/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTCoupons(int id)
        {
            if (_dbXContext.TCoupons == null)
            {
                return NotFound();
            }
            var tCoupons = await _dbXContext.TCoupons.FindAsync(id);
            if (tCoupons == null)
            {
                return NotFound();
            }

            _dbXContext.TCoupons.Remove(tCoupons);
            await _dbXContext.SaveChangesAsync();

            return NoContent();
        }

        private bool TCouponsExists(int id)
        {
            return (_dbXContext.TCoupons?.Any(e => e.CouponId == id)).GetValueOrDefault();
        }
    }
}
