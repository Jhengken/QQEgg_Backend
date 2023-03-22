using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using QQEgg_Backend.DTO;
using QQEgg_Backend.Models;
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

        // GET: api/<OrdersController>/queryAll/{id}
        [HttpGet("queryAll/{id}")]
        public async Task<List<OrdersDTO>> AllGet(int id)
        {
            int customerId = id;

            //使用Join，挑最新10筆訂單
            TCorders order = _context.TCorders.FirstOrDefault(o => o.CustomerId == customerId);
            if (order != null)
            {
                var o = _context.TCorders.Where(o => o.CustomerId == order.CustomerId).OrderByDescending(o=>o.OrderId).Take(10);
                var dto = o.Join(_context.TCorderDetail, o => o.OrderId, od => od.OrderId, (o, od) => new OrdersDTO()
                {
                    OrderId = o.OrderId,
                    TradeNo = o.TradeNo,
                    CustomerName = o.Customer.Name,
                    ProductName = o.Product.Name,
                    OrderDate = o.OrderDate,
                    CancelDate = o.CancelDate,
                    StartDate = o.StartDate,
                    EndDate = o.EndDate,
                    Price = od.Price,
                    CategoryName = od.Room.Category.Name,
                    Discount = od.Coupon.Discount, //如果沒有使用coupon，一樣可以撈，前端再判斷是不是null
                }).ToList();
                return dto;
            }
            else
                return null!;   //回傳204，代表成功但沒有內容
        }

        // GET api/<OrdersController>/queryTrade/{id}
        [HttpGet("queryTrade/{id}")]
        public async Task<OrdersDTO> TradeGet(int id)
        {
            int tradeNo = id;

            //使用join
            TCorders order = _context.TCorders.FirstOrDefault(o => o.TradeNo == tradeNo);
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
                    CategoryName = od.Room.Category.Name,
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

        // PUT api/<OrdersController>/cancel/{id}
        [HttpPut("cancel/{id}")]
        public async Task<string> Put(int id)
        {
            int tradeNo = id;

            TCorders order = _context.TCorders.FirstOrDefault(o => o.TradeNo == tradeNo);
            if (order != null)
            {
                order.CancelDate = DateTime.Now;
                _context.Entry(order).State=EntityState.Modified;
                await _context.SaveChangesAsync();
                return "修改完成";
            }

            return "無此訂單";
        }

        // DELETE api/<OrdersController>/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<string> Delete(int id)
        {
            int tradeNo = id;

            TCorders order = _context.TCorders.FirstOrDefault(o => o.TradeNo == tradeNo);
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
