using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QQEgg_Backend.DTO;
using QQEgg_Backend.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;

namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly dbXContext _dbxContext;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public ClientController(dbXContext dbxContext, IConfiguration configuration)
        {
            _dbxContext = dbxContext;
            _configuration = configuration;
        }
        /// <summary>
        /// 顧客註冊資料先寫到DTO裡面，黑名單及積分自動產生
        /// </summary>
        /// <param name="value"></param>
        /// <returns>註冊完成</returns>
        [HttpPost("Register")]
        public async Task<string> Register(CustomersDTO value)
        {

            value.EncryptPassword();

            var customer = new TCustomers
            {
                Name = value.Name,
                Email = value.Email,
                Birth = value.Birth,
                Password = value.PasswordHash,// 將加密後的密碼存入資料
                Phone = value.Phone,
                Sex = value.Sex,
                CreditCard = value.CreditCard,
                BlackListed = true,
                CreditPoints = 100

            };

            _dbxContext.TCustomers.Add(customer);
            _dbxContext.SaveChanges();

            return "註冊成功";
        }

        /// <summary>
        /// 登入帳密之後把資訊存入JWT裡面，會秀出JWT的token做使用，這邊設定30分鐘後失效
        /// </summary>
        /// <param name="value"></param>
        /// <returns>TOKEN條碼可以去JWTIO看資料</returns>
        [HttpPost]
        public async Task<IActionResult> JwtLogin(LoginPostDTO value)
        {
            if (value.Email != null && value.Password != null)
            {
                var user = _dbxContext.TCustomers.FirstOrDefault(u => u.Email == value.Email);
                var userId = _dbxContext.TCustomers.Where(a => a.CustomerId == 1003).Select(a => a.CustomerId).FirstOrDefault();
                if (user == null || !BCrypt.Net.BCryptHlper.Verify(value.Password, user.Password))
                {
                    return BadRequest("帳密錯誤");
                }
                else
                {
                    var claims = new List<Claim>
                            {
                                 new Claim(JwtRegisteredClaimNames.Sub, user.CustomerId.ToString()), // 添加用户 ID 作为 subject
                                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                 new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                                 new Claim("Name",user.Name),
                                 new Claim(JwtRegisteredClaimNames.Email, value.Email)

                        };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: DateTime.UtcNow.AddMinutes(10),
                        signingCredentials: signIn);
                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
            }
            else
            {
                return BadRequest("Invalid credentials");
            }
        }

        /// <summary>
        /// 登出後把JWT設為空值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}")]
        public string JwtLoginOut(int id)
        {
            //將當前令牌設為 null
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return "已登出";
        }

        // GET: api/Customers/5
        /// <summary>
        /// 顧客點開帳戶資訊可以查詢到填入吃資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns>回傳資訊</returns>
        [Authorize]
        [HttpGet("id")]
        public async Task<CustomersPUTDTO> GetTCustomers(int id)
        {

            var result = await _dbxContext.TCustomers.FindAsync(id);
            if (result == null)
            {
                return null;
            }

            return new CustomersPUTDTO
            {
                Name = result.Name,
                Email = result.Email,
                Phone = result.Phone,
                Birth = result.Birth,
                CreditCard = result.CreditCard,
                Sex = result.Sex,
                Password = result.Password,
            };
        }

        /// <summary>
        /// 顧客修改資料
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tCustomers"></param>
        /// <returns>修改完成</returns>
        // PUT: api/Customers/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<string> PutTCustomers(int id, [FromBody] CustomersPUTDTO tCustomers)
        {
            var result = (from c in _dbxContext.TCustomers where c.CustomerId == id select c).SingleOrDefault();
            if (result != null)
            {
                result.Name = tCustomers.Name;
                result.Email = tCustomers.Email;
                result.Phone = tCustomers.Phone;
                result.Birth = tCustomers.Birth;
                result.CreditCard = tCustomers.CreditCard;
                result.Sex = tCustomers.Sex;
                // 检查是否有更新密码
                if (!string.IsNullOrEmpty(tCustomers.Password))
                {
                    result.Password = tCustomers.Password; // 儲存原始密碼
                    tCustomers.EncryptPassword(); // 對原始密碼加密
                    result.Password = tCustomers.PasswordHash; // 儲存加密後密碼
                                                               // 生成新的JWT令牌
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_configuration["Jwt:KEY"]);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(JwtRegisteredClaimNames.Name, result.Name),
                            new Claim(JwtRegisteredClaimNames.Email, result.Email)
                        }),
                        Expires = DateTime.UtcNow.AddDays(7),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);
                    return tokenString;
                }
            }

            //_context.Entry(result).State = EntityState.Modified;
            await _dbxContext.SaveChangesAsync();
            return "修改成功";
        }
        private bool TCustomersExists(int id)
        {
            return _dbxContext.TCustomers.Any(e => e.CustomerId == id);
        }


    }
}
