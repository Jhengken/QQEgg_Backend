using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using QQEgg_Backend.DTO;
using QQEgg_Backend.Models;

namespace QQEgg_Backend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TSuppliersController : ControllerBase
    {
        private readonly dbXContext _context;
        private readonly IConfiguration _configuration;


        public TSuppliersController(dbXContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

        }

        /// <summary>
        /// 撈出所有資料
        /// </summary>
        /// <returns></returns>
        // GET: api/TSuppliers
        [HttpGet]
        public async Task<IEnumerable<SuppliersDTO>> GetTSuppliers()
        {
            return _context.TSuppliers.Select(sup => new SuppliersDTO
            {
                //SupplierId = sup.SupplierId,
                Name = sup.Name,
                Email = sup.Email,
                Phone = sup.Phone,
                Address= sup.Address,
                Password = sup.Password,
            });
        }


        /// <summary>
        /// 修改編輯業者資料
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tSuppliers"></param>
        /// <returns></returns>
        // PUT: api/TSuppliers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTSuppliers(int id, SuppliersDTO tSuppliers)
        {
            var result = (from s in _context.TSuppliers where s.SupplierId == id select s).SingleOrDefault();
            if (result != null)
            {
                result.Name = tSuppliers.Name;
                result.Email = tSuppliers.Email;
                result.Phone = tSuppliers.Phone;
                result.Address = tSuppliers.Address;
                // 检查是否有更新密码
                //if (!string.IsNullOrEmpty(tSuppliers.Password))
                // {
                //  result.Password = tSuppliers.Password; // 儲存原始密碼
                //  tSuppliers.EncryptPassword(); // 對原始密碼加密
                //  result.Password = tSuppliers.PasswordHash; // 儲存加密後密碼
                // 生成新的JWT令牌
                var claims = new List<Claim>
                            {
                                 new Claim(JwtRegisteredClaimNames.Sub, result.SupplierId.ToString()), // 添加用户 ID 作为 subject
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
                await _context.SaveChangesAsync();
                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }
            else
            {
                return BadRequest("格式錯誤");
            }
        }

       

        // POST: api/TSuppliers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [AllowAnonymous]
        [HttpPost("PostTSuppliers")]
        public async Task<IActionResult> PostTSuppliers([FromBody] SuppliersDTO tSuppliers)
        {
            tSuppliers.EncryptPassword();

            TSuppliers sup = new TSuppliers
            {
                Name = tSuppliers.Name,
                Email = tSuppliers.Email,
                Phone = tSuppliers.Phone,
                Password = tSuppliers.PasswordHash,
                Address= tSuppliers.Address,
                BlackListed = false,
                CreditPoints = 100

            };

            _context.TSuppliers.Add(sup);
            _context.SaveChanges();

            return Ok(new
            { 
                success=true,
                messsage="註冊成功"
            });
        }

        /// <summary>
        /// 登入帳密之後把資訊存入JWT裡面，會秀出JWT的token做使用，這邊設定30分鐘後失效
        /// </summary>
        /// <param name="value"></param>
        /// <returns>TOKEN條碼可以去JWTIO看資料</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> JwtLogin(LoginPostDTO tSuppliers)
        {
            if (tSuppliers.Email != null && tSuppliers.Password != null)
            {
                var SUser = _context.TSuppliers.FirstOrDefault(u => u.Email == tSuppliers.Email);
                var SUserId = _context.TSuppliers.Where(a => a.SupplierId == 1003).Select(a => a.SupplierId).FirstOrDefault();
                if (SUser == null || !BCrypt.Net.BCryptHlper.Verify(tSuppliers.Password, SUser.Password))
                {
                    return BadRequest("帳密錯誤");
                }
                else
                {
                    var claims = new List<Claim>
                            {
                                 new Claim(JwtRegisteredClaimNames.Sub, SUser.SupplierId.ToString()), // 添加用户 ID 作为 subject
                                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                 new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                                 new Claim("Name",SUser.Name),
                                 new Claim(JwtRegisteredClaimNames.Email, tSuppliers.Email),
                                 new Claim("Phone",SUser.Phone),

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

        // DELETE: api/TSuppliers/5
        [HttpDelete("{id}")]
        public async Task<string> DeleteTSuppliers(int id)
        {
            var tSuppliers = await _context.TSuppliers.FindAsync(id);
            if (tSuppliers == null)
            {
                return "刪除失敗";
            }

            _context.TSuppliers.Remove(tSuppliers);
            await _context.SaveChangesAsync();

            return "刪除成功";
        }

        [HttpPost("check-information/{id}")]
        [Authorize]
        public async Task<IActionResult> checkinformation(int id, [FromBody] KeyWordRequest keyWordRequest)
        {
            string KeyWord = keyWordRequest.KeyWord;
            var users = await (from a in _context.TSuppliers where a.SupplierId != id select a).ToListAsync();
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

        private bool TSuppliersExists(int id)
        {
            return (_context.TSuppliers?.Any(e => e.SupplierId == id)).GetValueOrDefault();
        }
    }
}
