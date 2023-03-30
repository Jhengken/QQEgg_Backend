using Microsoft.AspNetCore.Mvc;
using QQEgg_Backend.DTO;
using QQEgg_Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace QQEgg_Backend.Serivce
{
    public class ForgetPasswordSerivce
    {
        private readonly dbXContext _dbXContext;

        public ForgetPasswordSerivce(dbXContext dbXContext)
        {
            _dbXContext = dbXContext;
        }


        public async Task<IActionResult> ResetPassword(string userEmail)
        {
            // 1. 找資料庫是否有這筆資料
            var user = _dbXContext.TCustomers.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return new NotFoundResult(); 
            }

            // 2. 生成新密碼
            string newPassword = GenerateRandomPassword();

            // 3. 哈希新密碼
            string hashedPassword = HashPassword(newPassword);

            // 4. 更新數據庫中的密碼
            user.Password = hashedPassword;
            _dbXContext.SaveChanges();

            // 5. 將新密碼發送給用戶
            await SendNewPasswordByEmail(userEmail, newPassword);

            return new OkResult(); 
        }

        /// <summary>
        /// 使用隨機亂數產生8個位數的高強度密碼
        /// </summary>
        /// <returns></returns>
        private string GenerateRandomPassword()
        {
            // 產生 8 位隨機密碼
            const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            var random = new RNGCryptoServiceProvider();  //生成安全隨機數。它使用加密強度的隨機數生成器，可以提供更高的隨機性和更好的安全性
            byte[] bytes = new byte[8];
            random.GetBytes(bytes);
            StringBuilder password = new StringBuilder();
            foreach (byte b in bytes)
            {
                password.Append(allowedChars[b % allowedChars.Length]);
            }
            return password.ToString();
        }

        private string HashPassword(string password)
        {
            // 使用 bcrypt 將密碼加密
            const int workFactor = 12; // 定義工作因子，通常值在 10 到 31 之間
            string hashedPassword = BCrypt.Net.BCryptHlper.HashPassword(password, workFactor);
            return hashedPassword;
        }

        /// <summary>
        /// 寄送新密碼給顧客端
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task SendNewPasswordByEmail(string email, string password)
        {
            var user = _dbXContext.TCustomers.FirstOrDefault(u => u.Email == email);
            // 發送包含新密碼的電子郵件
            string subject = "想享Xiang會員忘記密碼處理";
            string body = $"親愛的 {user.Name} 會員，您好: " +
       $"您的新密碼為 : {password} <br>" +
       $"請點擊以下按鈕登入並重設您的密碼: <br><br>" +
       $"<a href='http://localhost:3000' style='display: inline-block; background-color: #007bff; color: #ffffff; text-decoration: none; padding: 12px 24px; font-size: 16px; border-radius: 4px;'>前往官網</a>";
            using var message = new MailMessage("noreply@example.com", email, subject, body);
            message.IsBodyHtml = true;  //加這段就可以讓body吃的html內文
            using var client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;
            client.Credentials = new NetworkCredential("sam831020ya@gmail.com", "nwvuoijokntfhtcb");
            client.EnableSsl = true;
            await client.SendMailAsync(message);
        }
    }
}
