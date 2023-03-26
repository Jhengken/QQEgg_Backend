using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using QQEgg_Backend.DTO;
using QQEgg_Backend.Models;
using SQLitePCL;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly dbXContext _context;
        public OrdersController(dbXContext context)
        {
            _context = context;
        }
        

        /// <summary>
        /// 把顧客租的訂單顯示出來
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/<OrdersController>/queryAll/{id}
        [HttpGet("queryAll/{id}")]
        public async Task<IEnumerable<OrdersDTO>> AllGet(int id)
        {
            //最新 10 筆訂單
            TCorders order = _context.TCorders.FirstOrDefault(o => o.CustomerId == id);
            if (order != null)
            {
                var result = from a in _context.TCorders select a;

                var orders = await result.Include(o => o.TCorderDetail).ThenInclude(od => od.Room).Include(o => o.TCorderDetail).Include(od => od.Product).Where(o => o.CustomerId == id)
                    .OrderByDescending(o => o.OrderId).Take(10).SelectMany(o =>o.TCorderDetail,(o,od)=> new OrdersDTO
                    {
                        OrderId = o.OrderId,
                        CustomerName = o.Customer.Name,
                        ProductName = o.Product.Name,
                        StartDate = o.StartDate,
                        EndDate = o.EndDate,
                        CancelDate = o.CancelDate,
                        TradeNo = o.TradeNo,
                        CouponId = od.CouponId,
                        Price = od.Price,
                        RoomId = od.RoomId,
                        Discount = od.Coupon.Discount,
                        CategoryName = od.Room.Category.Name
                    }).ToListAsync();
             
                return orders.ToList();
            }
            return null;
        }


        /// <summary>
        /// 取消訂單
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // PUT api/<OrdersController>/cancel/{id}
        [HttpPut("cancel/{id}")]
        public async Task<string> Put(string id)
        {
            string tradeNo = id;

            TCorders order = _context.TCorders.FirstOrDefault(o => o.TradeNo == tradeNo)!;
            if (order != null)
            {
                order.CancelDate = DateTime.Now;
                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return "修改完成";
            }

            return "無此訂單";
        }


        /// <summary>
        /// 搜尋單筆資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET api/<OrdersController>/queryTrade/{id}
        [HttpGet("queryTrade/{id}")]
        public async Task<OrdersDTO> TradeGet(string id)
        {
            string tradeNo = id;

            //使用join
            TCorders order = _context.TCorders.FirstOrDefault(o => o.TradeNo == tradeNo)!;
            if (order != null)
            {
                var o = _context.TCorders.Where(o => o.OrderId == order.OrderId);
                var od = _context.TCorderDetail.Where(od => od.OrderId == order.OrderId);
                var dto = o.Join(od, o => o.OrderId, od => od.OrderId, (o, od) => new OrdersDTO()
                {
                    OrderId = o.OrderId,
                    TradeNo=o.TradeNo,
                    CustomerName = o.Customer.Name,
                    ProductName=o.Product.Name,
                    OrderDate=o.OrderDate,
                    CancelDate=o.CancelDate,
                    StartDate=o.StartDate,
                    EndDate=o.EndDate,
                    Price=od.Price,
                    //CategoryName = od.Room.Category.Name,
                    Discount=od.Coupon.Discount, //如果沒有使用coupon，一樣可以撈，前端再判斷是不是null
                }).ToList();
                return dto[0];
            }
            else
                return null!;   //回傳204，代表成功但沒有內容

        }

        // POST api/<OrdersController>/create
        [HttpPost("create")]
        public async Task<string> Post([FromBody] OrdersDTO dto)
        {
            TCorders cOrder = new TCorders()
            {
                TradeNo = dto.TradeNo,
                ProductId = dto.ProductId,
                CustomerId = dto.CustomerId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
            };
            _context.TCorders.Add(cOrder);
            await _context.SaveChangesAsync();

            var orderId = _context.TCorders.OrderBy(o => o.OrderId).LastOrDefault(o => o.TradeNo == dto.TradeNo);

            if (orderId != null)
            {
                TCorderDetail cOrderDetail = new TCorderDetail()
                {
                    OrderId = orderId.OrderId,
                    RoomId = dto.RoomId,
                    CouponId = dto.CouponId,
                    Price = dto.Price,
                };
                _context.TCorderDetail.Add(cOrderDetail);
                await _context.SaveChangesAsync();
            }
            return "新增成功";
        }


       
        // DELETE api/<OrdersController>/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<string> Delete(string id)
        {
            string tradeNo = id;

            TCorders order = _context.TCorders.FirstOrDefault(o => o.TradeNo == tradeNo)!;
            if (order != null)
            {
                _context.TCorders.Remove(order);
                await _context.SaveChangesAsync();
                return "刪除成功";
            }

            return "無此訂單";
        }
    }
}
