using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using QQEgg_Backend.DTO;
using QQEgg_Backend.Models;
using SQLitePCL;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using ZXing.QrCode;
using static IdentityServer4.Models.IdentityResources;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using QQEgg_Backend.Serivce;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly dbXContext _context;
        private readonly QrcodeService _qrcodeService;

        public OrdersController(dbXContext context,QrcodeService qrcodeService)
        {
            _context = context;
            _qrcodeService = qrcodeService;
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
                        //OrderId = o.OrderId,
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

        // POST api/<OrdersController>/payform
        [HttpPost("payform")]
        public async Task<string> ECPayForm([FromBody] OrderPostDTO dto)
        {
            ECPayDetail detail = new ECPayDetail()
            {
                ItemName = dto.ItemName,
                TotalAmount = dto.Price.ToString(),
            };
            var formString = new ECPayService().GetReturnValue(detail);

            //先存起來，付款成功就建立訂單
            OrderData.CustomerID = dto.CustomerId;
            OrderData.ProductID = dto.ProductId;
            OrderData.RoomID = dto.RoomId;
            OrderData.Price = dto.Price;
            OrderData.TradeNo = detail.MerchantTradeNo;
            OrderData.StartDate = dto.StartDate;
            OrderData.EndDate = dto.EndDate;
            OrderData.CouponID = dto.CouponID;

            return formString;
        }

        // POST api/<OrdersController>/return-create
        [HttpPost("return-create")]
        public async Task<string> ReturnCreate()
        {
            var form = Request.Form;
            ECPayResult result = new ECPayService().GetCallbackResult(form);
            //return result.ReceiveObj!;     //要看綠界回傳打開這行

            string rtnCode = form.First(r => r.Key == "RtnCode").Value[0];
            if (rtnCode == "1")
            {
                TCorders cOrder = new TCorders()
                {
                    TradeNo = OrderData.TradeNo,
                    ProductId = OrderData.ProductID,
                    CustomerId = OrderData.CustomerID,
                    StartDate = OrderData.StartDate,
                    EndDate = OrderData.EndDate,
                };

                _context.TCorders.Add(cOrder);
                _context.SaveChanges();
                int userId = (int)cOrder.CustomerId; // 使用訂單中的客戶ID
                await _qrcodeService.SendEmail(userId);
                var orderId = _context.TCorders.OrderBy(o => o.OrderId).LastOrDefault(o => o.TradeNo == OrderData.TradeNo);
                if (orderId != null)
                {
                    TCorderDetail cOrderDetail = new TCorderDetail()
                    {
                        OrderId = orderId.OrderId,
                        RoomId = OrderData.RoomID,
                        Price = OrderData.Price,
                        CouponId = OrderData.CouponID,
                    };
                    _context.TCorderDetail.Add(cOrderDetail);
                    _context.SaveChanges();
                }
            }

            return await Task.FromResult(result.ResponseECPay!);
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
