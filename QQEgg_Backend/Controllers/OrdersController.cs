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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly dbXContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<QrcodeController> _logger;
        private readonly byte[] _aesKey;
        private readonly byte[] _aesIv;

        public OrdersController(dbXContext context, IConfiguration configuration, ILogger<QrcodeController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _aesKey = new byte[32]; // 產生 256 bits 的 key
            _aesIv = new byte[16]; // 產生 128 bits 的 IV
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(_aesKey);
                rng.GetBytes(_aesIv);
            }
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
                int userId = (int)cOrder.CustomerId; // 使用訂單中的客戶ID
                await SendEmail(userId);
                _context.TCorders.Add(cOrder);
                _context.SaveChanges();

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
        private byte[] GenerateQRCode()
        {
            var productCode = _context.TPsiteRoom.Select(a => a.RoomId).FirstOrDefault().ToString();
            var roomPassword = _context.TPsiteRoom.Select(a => a.RoomPassWord).FirstOrDefault().ToString();
            var aesKey = new byte[32]; // 產生 256 bits 的 key
            var aesIv = new byte[16]; // 產生 128 bits 的 IV
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(aesKey);
                rng.GetBytes(aesIv);
            }
            var encryptedProductCode = Encrypt(productCode, aesKey, aesIv);
            var encryptedRoomPassword = Encrypt(roomPassword, aesKey, aesIv);
            Console.WriteLine($"{encryptedProductCode},{encryptedRoomPassword}");

            var expirationDateObj = _context.TCorders.Select(a => a.EndDate).FirstOrDefault();

            if (expirationDateObj != null)
            {
                DateTime expirationDate = DateTime.Parse(expirationDateObj.ToString());
                var expirationDateStr = expirationDate.ToString("yyyy-MM-dd HH:mm:ss");
                // 將加密後的產品代號和房間密碼加入 QR Code 中
                var qrText = $"{encryptedProductCode};{encryptedRoomPassword};{expirationDateStr}"; // 添加失效日期到QR码文本中
                var width = 500; // width of the QR Code
                var height = 500; // height of the QR Code
                var margin = 0;
                var qrCodeWriter = new ZXing.BarcodeWriterPixelData
                {
                    Format = ZXing.BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Height = height,
                        Width = width,
                        Margin = margin,
                        PureBarcode = false,
                        ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.H
                        //調整二維碼的糾錯等級：提高二維碼的糾錯等級，使其能夠容納更多的損壞資訊。在生成二維碼時，您需要指定糾錯等級。糾錯等級有四個，分別是 L（7 %），M（15 %），Q（25 %）和 H（30 %）。
                    }
                };
                var pixelData = qrCodeWriter.Write(qrText);
                // creating a PNG bitmap from the raw pixel data; if only black and white colors are used it makes no difference if the raw pixel data is BGRA oriented and the bitmap is initialized with RGB
                using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
                {
                    using (var ms = new MemoryStream())
                    {
                        var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        try
                        {
                            // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image
                            System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                        }
                        finally
                        {
                            bitmap.UnlockBits(bitmapData);
                        }
                        var logo = new System.Drawing.Bitmap(@"C:\Users\Acer\OneDrive\OneNote 上傳\256.png"); // 读取 logo 图片
                        var g = Graphics.FromImage(bitmap);
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(logo, new System.Drawing.Rectangle((bitmap.Width - logo.Width) / 2, (bitmap.Height - logo.Height) / 2, logo.Width, logo.Height));

                        // save to folder
                        string fileGuid = Guid.NewGuid().ToString().Substring(0, 4);

                        // save to stream as PNG
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 驗證key和iv的長度(AES只有三種長度適用)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        public static void Validate_KeyIV_Length(byte[] key, byte[] iv)
        {
            //驗證key和iv都必須為128bits或192bits或256bits
            List<int> LegalSizes = new List<int>() { 128, 192, 256 };
            int keyBitSize = key.Length * 8;
            int ivBitSize = iv.Length * 8;
            if (!LegalSizes.Contains(keyBitSize) || !LegalSizes.Contains(ivBitSize))
            {
                throw new Exception($@"key或iv的長度不在128bits、192bits、256bits其中一個，輸入的key bits:{keyBitSize},iv bits:{ivBitSize}");
            }
        }

        /// <summary>
        /// 加密後回傳base64String，相同明碼文字編碼後的base64String結果會相同(類似雜湊)，除非變更key或iv
        /// 如果key和iv忘記遺失的話，資料就解密不回來
        /// base64String若使用在Url的話，Web端記得做UrlEncode
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="plain_text"></param>
        /// <returns></returns>
        public static string Encrypt(string plain_text, byte[] key, byte[] iv)
        {
            Validate_KeyIV_Length(key, iv);
            using Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC; //非必須，但加了較安全
            aes.Padding = PaddingMode.PKCS7; //非必須，但加了較安全

            ICryptoTransform transform = aes.CreateEncryptor(key, iv);

            byte[] bPlainText = Encoding.UTF8.GetBytes(plain_text); //明碼文字轉byte[]
            byte[] outputData = transform.TransformFinalBlock(bPlainText, 0, bPlainText.Length); //加密
            return Convert.ToBase64String(outputData);
        }
        /// <summary>
        /// 解密後，回傳明碼文字
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static string Decrypt(string base64String, byte[] key, byte[] iv)
        {
            Validate_KeyIV_Length(key, iv);

            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = aes.CreateDecryptor(key, iv);

            byte[] encryptedBytes = Convert.FromBase64String(base64String);

            byte[] decryptedBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                {
                    cs.Write(encryptedBytes, 0, encryptedBytes.Length);
                    cs.FlushFinalBlock();
                    decryptedBytes = ms.ToArray();
                }
            }

            return Encoding.UTF8.GetString(decryptedBytes);

            //Validate_KeyIV_Length(key, iv);
            //Aes aes = Aes.Create();
            //aes.Mode = CipherMode.CBC;//非必須，但加了較安全
            //aes.Padding = PaddingMode.PKCS7;//非必須，但加了較安全

            //ICryptoTransform transform = aes.CreateDecryptor(key,iv);
            //byte[] bEnBase64String = null;
            //byte[] outputData = null;
            //try
            //{
            //    bEnBase64String = Convert.FromBase64String(base64String);//有可能base64String格式錯誤
            //    outputData = transform.TransformFinalBlock(bEnBase64String, 0, bEnBase64String.Length);//有可能解密出錯
            //}
            //catch (Exception ex)
            //{
            //    //todo 寫Log
            //    throw new Exception($@"解密出錯:{ex.Message}");
            //}

            ////解密成功
            //return Encoding.UTF8.GetString(outputData);

        }
        [HttpPost]
   
        public async Task<IActionResult> SendEmail(int userId)
        {
            //var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            //var jwtHandler = new JwtSecurityTokenHandler();
            //var jwtToken = jwtHandler.ReadJwtToken(token);

            //var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            //userId = int.Parse(subClaim.Value);
            // 從資料庫讀取使用者的電子郵件地址
            var user = await _context.TCustomers.FindAsync(userId);
            string receiveMail = user.Email;
            string subject = "想享訂房資訊";
            // 呼叫 GenerateQRCode 方法來取得 QR Code 的圖片資料
            byte[] qrCodeData = GenerateQRCode();


            // 將 QR Code 的圖片資料轉換成 Base64 字串，以便在 HTML 內容中嵌入圖片
            string qrCodeBase64 = Convert.ToBase64String(qrCodeData);
            string qrCodeHtml = $"<img src=\"data:image/png;base64,{qrCodeBase64}\" alt=\"QR Code\">";
            ECPayService ecPayService = new ECPayService();
            ECPayDetail detail = new ECPayDetail();
            string paymentHtml = ecPayService.GetReturnValue(detail);
            string body = $"親愛的 {user.Name}，<br><br>您好！這是你租房間的qrcode，拿著qrcode到門口掃描就可以進出了!<br><br>感謝!!!";

            // 建立 MailMessage 物件並設定內容
            MailMessage message = new MailMessage();
            message.From = new MailAddress("sam831020ya@gmail.com");
            message.To.Add(new MailAddress(receiveMail));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            // 建立 Attachment 物件並設定內容
            MemoryStream qrCodeStream = new MemoryStream(qrCodeData);
            Attachment qrCodeAttachment = new Attachment(qrCodeStream, "qrcode.png", "image/png");
            message.Attachments.Add(qrCodeAttachment);

            // 建立 SmtpClient 物件並發送郵件
            using (SmtpClient client = new SmtpClient())
            {
                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("sam831020ya@gmail.com", "nwvuoijokntfhtcb");
                client.EnableSsl = true;
                await client.SendMailAsync(message);
            }

            // 釋放資源並回傳訊息
            qrCodeStream.Dispose();
            qrCodeAttachment.Dispose();
            return Ok();
        }
    }
}
