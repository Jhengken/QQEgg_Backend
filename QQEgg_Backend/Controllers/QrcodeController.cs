

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QQEgg_Backend.Models;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ZXing.QrCode;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Drawing;

namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QrcodeController : ControllerBase

    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<QrcodeController> _logger;
        private readonly dbXContext _dbXContext;
        private readonly byte[] _aesKey;
        private readonly byte[] _aesIv;

        public QrcodeController(IConfiguration configuration, ILogger<QrcodeController> logger, dbXContext dbXContext)
        {
            _configuration = configuration;
            _logger = logger;
            _dbXContext = dbXContext;
            _aesKey = new byte[32]; // 產生 256 bits 的 key
            _aesIv = new byte[16]; // 產生 128 bits 的 IV
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(_aesKey);
                rng.GetBytes(_aesIv);
            }
        }

        //     [Authorize]
        //[HttpPost("generate")]
        //public async Task<IActionResult> GenerateQRCode()
        //{
        //    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        //    var username = User.FindFirst(ClaimTypes.Name)?.Value;

        //    // Replace this with your own logic to generate the encrypted data for the QR code
        //    // Here we are just creating a simple string for demonstration purposes
        //    var encryptedData = $"{userId}:{username}:{DateTime.UtcNow}";

        //    var qrCode = GenerateQRCodeImage(encryptedData);

        //    // Encode the QR code image as a Base64 string
        //    var base64Image = ConvertImageToBase64(qrCode);

        //    // Return the Base64 string as a JSON object
        //    return Ok(new { QRCode = base64Image });
        //}

        //public byte[] ImageToByArray(System.Drawing.Image img)
        //{
        //    using (var ms = new MemoryStream())
        //    {
        //        img.Save(ms, img.RawFormat);
        //        return ms.ToArray();
        //    }
        //}

        /// <summary>
        /// 產生Qrcode資料
        /// </summary>
        /// <returns></returns>
        [HttpPost("generate")]
        public async Task<IActionResult>  GenerateQRCode1()
        {
            var productCode = _dbXContext.TPsiteRoom.Select(a => a.RoomId).FirstOrDefault().ToString();
            var roomPassword = _dbXContext.TPsiteRoom.Select(a => a.Description).FirstOrDefault().ToString();
            //var expirationDate = _dbXContext.TCorders.Select(a=>a.EndDate).FirstOrDefault();   //抓DB顧客房間預定的最後日期
            //var expirationDateStr = expirationDate.HasValue ? expirationDate.Value.ToString("yyyy-MM-dd") : "";//轉換日期為字串
            // 將產品代號和房間密碼用 AES 加密
            //var aesKey = new byte[32]; // 產生 256 bits 的 key
            //var aesIv = new byte[16]; // 產生 128 bits 的 IV
            //using (var rng = RandomNumberGenerator.Create())
            //{
            //    rng.GetBytes(aesKey);
            //    rng.GetBytes(aesIv);
            //}
            var encryptedProductCode = Encrypt(productCode, _aesKey, _aesIv);
            var encryptedRoomPassword = Encrypt(roomPassword, _aesKey, _aesIv);
            Console.WriteLine($"{encryptedProductCode},{encryptedRoomPassword}");
            var expirationDate = DateTime.Now.AddDays(1);
            var expirationDateStr = expirationDate.ToString("yyyy-MM-dd HH:mm:ss");

            // 將加密後的產品代號和房間密碼加入 QR Code 中
            var qrText = $"{encryptedProductCode};{encryptedRoomPassword};{expirationDateStr}"; // 添加失效日期到QR码文本中
            Console.WriteLine(qrText);


            Byte[] byteArray;
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
                    Margin = margin
                }
            };
            var pixelData = qrCodeWriter.Write(qrText);
            // 添加Logo到QR码
          
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
                    // 添加Logo到QR码
                    //var logo = new System.Drawing.Bitmap(@"C:\Users\Acer\OneDrive\OneNote 上傳\後端C#\QQEgg_Backend\images\poop (1).png"); // 读取 logo 图片
                    //var g = Graphics.FromImage(bitmap);
                    //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    //g.DrawImage(logo, new System.Drawing.Rectangle((bitmap.Width - logo.Width) / 2, (bitmap.Height - logo.Height) / 2, logo.Width, logo.Height));
                    // save to folder
                    //string fileGuid = Guid.NewGuid().ToString().Substring(0, 4);
                    //bitmap.Save(Server.MapPath("~/qrr") + "/file-" + fileGuid + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    // save to stream as PNG
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return File(ms.ToArray(), "image/png");

                }

            }

        }
        private byte[] GenerateQRCode()
        {
            var productCode = _dbXContext.TPsiteRoom.Select(a => a.RoomId).FirstOrDefault().ToString();
            var roomPassword = _dbXContext.TPsiteRoom.Select(a => a.Description).FirstOrDefault().ToString();
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
            var expirationDate = DateTime.Now.AddDays(1);
            var expirationDateStr = expirationDate.ToString("yyyy-MM-dd HH:mm:ss");

            // 將加密後的產品代號和房間密碼加入 QR Code 中
            var qrText = $"{encryptedProductCode};{encryptedRoomPassword};{expirationDateStr}"; // 添加失效日期到QR码文本中
            Console.WriteLine(qrText);

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
                    Margin = margin
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
                    //var logo = new System.Drawing.Bitmap(@"C:\Users\Acer\OneDrive\OneNote 上傳\後端C#\QQEgg_Backend\images\poop (1).png"); // 读取 logo 图片
                    //var g = Graphics.FromImage(bitmap);
                    //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    //g.DrawImage(logo, new System.Drawing.Rectangle((bitmap.Width - logo.Width) / 2, (bitmap.Height - logo.Height) / 2, logo.Width, logo.Height));

                    // save to folder
                    //string fileGuid = Guid.NewGuid().ToString().Substring(0, 4);
                    //bitmap.Save(Server.MapPath("~/qrr") + "/file-" + fileGuid + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    // save to stream as PNG
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }
        /// <summary>
        /// 比對顧客拿到的qrcode去跟訂房時間最比較，超過時間則不得進入
        /// </summary>
        /// <param name="qrCode"></param>
        /// <returns></returns>
        [HttpPost("check-expiration")]
        public IActionResult CheckExpiration([FromBody] string qrCode)
        {
            // 解密QR码中的文本，提取失效日期
            var fields = qrCode.Split(';');

            var expirationDateStr = fields.ElementAtOrDefault(2);
            if (DateTime.TryParse(expirationDateStr, out DateTime expirationDate))
            {
                // 比较当前日期与失效日期
                if (expirationDate < DateTime.Now)
                {
                    // 过期
                    return BadRequest("QR code has expired.");
                }
                else
                {
                    // 未过期，可以进入房间
                    return Ok();
                }
            }


            // 如果解密或解析失败，则认为QR码过期
            return BadRequest("QR code has expired.");
        }
        //// AES加密
        //private static string AesEncryptString(string plaintext, byte[] key, byte[] iv)
        //{
        //    using var aes = Aes.Create();
        //    aes.Key = key;
        //    aes.IV = iv;
        //    aes.Padding = PaddingMode.PKCS7;
        //    aes.Mode = CipherMode.CBC;

        //    using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        //    using var ms = new MemoryStream();
        //    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        //    {
        //        using var sw = new StreamWriter(cs);
        //        sw.Write(plaintext);
        //    }

        //    var encrypted = ms.ToArray();
        //    return Convert.ToBase64String(encrypted);
        //}

        //private string desEncryptBase64(string source, byte[] key, byte[] iv) 
        //{
        //      using var aes = Aes.Create();
        //        byte[] dataByteArray = Encoding.UTF8.GetBytes(source);
        //        aes.Key = key;
        //        aes.IV = iv;
        //        string encrypt = "";
        //        using (MemoryStream ms = new MemoryStream())
        //        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write)) { 
        //        cs.Write(dataByteArray, 0, dataByteArray.Length);
        //        cs.FlushFinalBlock();
        //        encrypt = Convert.ToBase64String(ms.ToArray());
        //        return encrypt;
        //    }
        //}
        //private string desDecryptBase64(string encrypt, byte[] key, byte[] iv)
        //{
        //    try
        //    {
        //        byte[] dataByteArray = Convert.FromBase64String(encrypt);
        //        using var aes = Aes.Create();
        //        aes.Key = key;
        //        aes.IV = iv;
        //        string decrypt = "";
        //        using (MemoryStream ms = new MemoryStream())
        //        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
        //        {
        //            cs.Write(dataByteArray, 0, dataByteArray.Length);
        //            cs.FlushFinalBlock();
        //            decrypt = Encoding.UTF8.GetString(ms.ToArray());
        //            return decrypt;
        //        }
        //    }
        //    catch (System.FormatException ex)
        //    {
        //        throw new ArgumentException("Invalid Base64 string", nameof(encrypt), ex);
        //    }
        //}
        ////解密
        //private string AesDecryptString(string ciphertext, byte[] key, byte[] iv)
        //{

        //    using var aes = Aes.Create();
        //    aes.Key = key;
        //    aes.IV = iv;


        //    byte[] dataByArray = Convert.FromBase64String(ciphertext);
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        //        {
        //            cs.Write(dataByArray, 0, dataByArray.Length);
        //            cs.FlushFinalBlock();
        //            return Encoding.UTF8.GetString(ms.ToArray());
        //        }
        //    }

        //}

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


        /// <summary>
        /// 以下兩行只是測試是不適Base64位元
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet("{input}")]
        public IActionResult CheckBase64(string input)
        {
            bool isBase64 = IsBase64(input);
            return Ok(new { input = input, isBase64 = isBase64 });
        }
        private static bool IsBase64(string s)
        {
            string regex = @"^[A-Za-z0-9+/]{4}(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{2}==)?$";
            return Regex.IsMatch(s, regex);
        }



        //[HttpPost]
        //public async Task<IActionResult> SendEmail(int userid)
        //{
        //    var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        //    var jwtHandler = new JwtSecurityTokenHandler();
        //    var jwtToken = jwtHandler.ReadJwtToken(token);

        //    var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        //    int userId = int.Parse(subClaim.Value);
        //    // 從資料庫讀取使用者的電子郵件地址
        //    var user = await _dbXContext.TCustomers.FindAsync(userId);
        //    string receiveMail = user.Email;
        //    string subject = "想享訂房資訊";
        //    // 呼叫 GenerateQRCode 方法來取得 QR Code 的圖片資料
        //    byte[] qrCodeData = GenerateQRCode();

        //    // 將 QR Code 的圖片資料轉換成 Base64 字串，以便在 HTML 內容中嵌入圖片
        //    string qrCodeBase64 = Convert.ToBase64String(qrCodeData);
        //    string qrCodeHtml = $"<img src=\"data:image/png;base64,{qrCodeBase64}\" alt=\"QR Code\">";
        //    string body = string.Format($"親愛的 {user.Name}，<br><br>您好！這是你租房間的qrcode，拿著qrcode到門口掃描就可以進出了!<br><br>感謝!!!");
        //    // 建立 MailMessage 物件並設定內容
        //    MailMessage message = new MailMessage();
        //    message.From = new MailAddress("sam831020ya@gmail.com");
        //    message.To.Add(new MailAddress(receiveMail));
        //    message.Subject = subject;
        //    message.Body = body;
        //    message.IsBodyHtml = true;
        //    // 添加 QR Code 圖檔作為附件
        //    using (MemoryStream stream = new MemoryStream(qrCodeData))
        //    {
        //        Attachment attachment = new Attachment(stream, "qrcode.png", "image/png");
        //        message.Attachments.Add(attachment);

        //        // 建立 SmtpClient 物件並發送郵件
        //        using (SmtpClient client = new SmtpClient())
        //        {
        //            client.Host = "smtp.gmail.com";
        //            client.Port = 587;
        //            client.UseDefaultCredentials = false;
        //            client.Credentials = new NetworkCredential("sam831020ya@gmail.com", "nwvuoijokntfhtcb");
        //            client.EnableSsl = true;

        //            await client.SendMailAsync(message);
        //        }
        //    }
        //    return Ok();
        //}
        [HttpPost]
        public async Task<IActionResult> SendEmail(int userId)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token);

            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
             userId = int.Parse(subClaim.Value);
            // 從資料庫讀取使用者的電子郵件地址
            var user = await _dbXContext.TCustomers.FindAsync(userId);
            string receiveMail = user.Email;
            string subject = "想享訂房資訊";
            // 呼叫 GenerateQRCode 方法來取得 QR Code 的圖片資料
            byte[] qrCodeData = GenerateQRCode();

            // 將 QR Code 的圖片資料轉換成 Base64 字串，以便在 HTML 內容中嵌入圖片
            string qrCodeBase64 = Convert.ToBase64String(qrCodeData);
            string qrCodeHtml = $"<img src=\"data:image/png;base64,{qrCodeBase64}\" alt=\"QR Code\">";
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

