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
using static IdentityServer4.Models.IdentityResources;

namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ClientController : ControllerBase
    {
        private readonly dbXContext _dbxContext;
        private readonly IConfiguration _configuration;

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
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] CustomersDTO value)
        {

            value.EncryptPassword();

            var customer = new TCustomers
            {
                Name = value.Name,
                Email = value.Email,
                // Birth = value.Birth,
                Password = value.PasswordHash,// 將加密後的密碼存入資料
                Phone = value.Phone,
                //Sex = value.Sex,
                //CreditCard = value.CreditCard,
                BlackListed = true,
                CreditPoints = 100

            };

            _dbxContext.TCustomers.Add(customer);
            _dbxContext.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "註冊成功"
            });
        }

        /// <summary>
        /// 登入帳密之後把資訊存入JWT裡面，會秀出JWT的token做使用，這邊設定30分鐘後失效
        /// </summary>
        /// <param name="value"></param>
        /// <returns>TOKEN條碼可以去JWTIO看資料</returns>
        [HttpPost]
        [AllowAnonymous]
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
                                 new Claim(JwtRegisteredClaimNames.Email, value.Email),
                                 new Claim("Phone",user.Phone),

                        };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: DateTime.UtcNow.AddDays(7),
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
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // 回傳過期的 JWT token，讓前端清除 token
            return Ok(new { token = "" });
        }

        // GET: api/Customers/5
        /// <summary>
        /// 顧客點開帳戶資訊可以查詢到填入資料
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
                //Birth = result.Birth,
                //CreditCard = result.CreditCard,
                //Sex = result.Sex,
                //Password = result.Password,
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
        [HttpPut("id")]
        public async Task<IActionResult> PutTCustomers(int id, [FromBody] CustomersPUTDTO tCustomers)
        {
            var result = (from c in _dbxContext.TCustomers where c.CustomerId == id select c).SingleOrDefault();
            if (result != null)
            {
                result.Name = tCustomers.Name;
                result.Email = tCustomers.Email;
                result.Phone = tCustomers.Phone;
                //result.Birth = tCustomers.Birth;
                //result.CreditCard = tCustomers.CreditCard;
                //result.Sex = tCustomers.Sex;
                // 检查是否有更新密码
                //if (!string.IsNullOrEmpty(tCustomers.Password))
                //{
                //    result.Password = tCustomers.Password; // 儲存原始密碼
                //    tCustomers.EncryptPassword(); // 對原始密碼加密
                //    result.Password = tCustomers.PasswordHash; // 儲存加密後密碼
                // 生成新的JWT令牌
                var claims = new List<Claim>
                            {
                                 new Claim(JwtRegisteredClaimNames.Sub, result.CustomerId.ToString()), // 添加用户 ID 作为 subject
                                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                 new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                                 new Claim("Name",result.Name),
                                 new Claim(JwtRegisteredClaimNames.Email, result.Email)


                        };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: signIn);
                await _dbxContext.SaveChangesAsync();
                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
            else
            {
                return BadRequest("格式錯誤");
            }
        }





        [HttpPost("check-information/{id}")]
        [Authorize]
        public async Task<IActionResult> checkinformation(int id, [FromBody]KeyWordRequest keyWordRequest)
        {
            string KeyWord = keyWordRequest.KeyWord;
            var users = await (from a in _dbxContext.TCustomers where a.CustomerId != id select a).ToListAsync();
            if (users != null)
            {
                if (users.Any(u => u.Email == KeyWord))
                {
                    return BadRequest(new { message = "此信箱已被使用" });
                }
                if (users.Any(u => u.Phone == KeyWord))
                {
                    return BadRequest(new { message = "此手機已被使用" });
                }
            }
            return Ok();
        }

        //GET: api/TCoupons
        /// <summary>
        /// 顧客打開優惠卷使用列出顧客的優惠卷，如果沒有則顯示空白
        /// </summary>
        /// <returns></returns>
        [HttpGet("Coupons")]
        [Authorize]
        public async Task<ActionResult<TCoupons>> GetCustomerTCoupons()
        {
            var customerId = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var customer = await (from c in _dbxContext.TCustomers select c).FirstOrDefaultAsync(c => c.Email == customerId);
            var coupons = (from c in _dbxContext.TCoupons select c).FirstOrDefault();

            //判斷顧客的帳號和黑名單
            if (customerId != null && customer.BlackListed == true)
            {
                //判斷顧客的優惠卷，秀出可以使用的優惠卷
                //if (customer.CreditPoints >= coupons.HowPoint)
                //{
                    // 從資料庫中獲取該顧客的優惠卷資料
                    var result = _dbxContext.TCoupons.ToList();
                    return Ok(result);
                //}
            }
            //直接顯示登入畫面
            return Unauthorized();
        }


        /// <summary>
        /// 使用優惠卷
        /// </summary>
        /// <param name="couponId"></param>
        /// <returns></returns>
        [HttpPost("{couponId}")]
        [Authorize]
        public async Task<IActionResult> UseCoupon(int couponId)
        {
            // 從資料庫中獲取優惠券
            var coupon = _dbxContext.TCoupons.FirstOrDefault(c => c.CouponId == couponId);

            if (coupon.Quantity > 0 && coupon.Available == true)
            {
                // 更新優惠券數量
                coupon.Quantity -= 1;
                await _dbxContext.SaveChangesAsync();

                // 返回使用成功的提示
                return Ok("使用優惠券成功！");
            }
            else
            {
                // 返回使用失敗的提示
                return BadRequest("該優惠券已經用完了！");
            }
        }


        [HttpGet("ListCoupon")]
        [AllowAnonymous]
        public async Task<IEnumerable<CouponDTO>> ListCoupon()
        {
            return _dbxContext.TCoupons.Select(c => new CouponDTO
            {
                CouponId = c.CouponId,
                Code = c.Code,
                HowPoint = c.HowPoint,
                Discount = c.Discount,
            }).ToList();

        }



        private static Dictionary<string, List<string>> _userCouponClaims = new Dictionary<string, List<string>>();

        [HttpGet("claimCoupon/{id}")]
        [Authorize]
        public IActionResult ClaimCoupon(int id)
        {
            string code = id.ToString();
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);


            if (userIdClaim == null)
            {
                return BadRequest(new { success = false, message = "沒讀到資料" });
            }

            string userId = userIdClaim.Value;
            if (int.TryParse(userIdClaim.Value, out int UserId))
            {
                var userPoints = _dbxContext.TCustomers
                                  .Where(a => a.CustomerId == UserId)
                                  .Select(a => a.CreditPoints)
                                  .FirstOrDefault();
                var requiredPoints = _dbxContext.TCoupons.Where(c => c.Code == code).Select(a => a.HowPoint).SingleOrDefault();
                if (userPoints <= requiredPoints)
                {
                    return BadRequest(new { success = false, message = "積分不足，無法領取優惠券" });
                }
            }

            
            if (_userCouponClaims.ContainsKey(userId) && _userCouponClaims[userId].Contains(code))
            {
                return BadRequest(new { success = false, message = "領過優惠卷" });
            }

            if (!_userCouponClaims.ContainsKey(userId))
            {
                _userCouponClaims[userId] = new List<string>();
            }
            _userCouponClaims[userId].Add(code);

            return Ok(new { success = true, message = "優惠卷成功領取" });
        }

        private bool TCustomersExists(int id)
        {
            return _dbxContext.TCustomers.Any(e => e.CustomerId == id);
        }


    }
}
