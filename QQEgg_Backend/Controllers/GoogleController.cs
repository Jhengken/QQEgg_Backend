using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Web;
using static Google.Apis.Auth.GoogleJsonWebSignature;
using IdentityServer4.Services;
using System.Net;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using QQEgg_Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using Google.Apis.Oauth2.v2;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Google.Apis.Oauth2.v2.Data;
using static Humanizer.On;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly dbXContext _dbXContext;
        public GoogleController(IConfiguration configuration, IHttpClientFactory httpClientFactory, dbXContext dbXContext)
        {
            _configuration = configuration;
            this.httpClientFactory = httpClientFactory;
            _dbXContext = dbXContext;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AccessTokenModel accessTokenModel)
        {
            try
            {
                string accessToken = accessTokenModel.AccessToken;
                // 創建Google OAuth2服務客戶端
                var googleCredential = GoogleCredential.FromAccessToken(accessToken);
                var oauth2Service = new Oauth2Service(new BaseClientService.Initializer
                {
                    HttpClientInitializer = googleCredential
                });

                // 獲取用戶資訊
                var userInfo = await oauth2Service.Userinfo.Get().ExecuteAsync();

                // 取得使用者 ID 和電子郵件
                string userId = userInfo.Id;
                string userEmail = userInfo.Email;
                string userName = userInfo.Name;

                // 在資料庫中查找使用者
                var user = _dbXContext.TCustomers.FirstOrDefault(u => u.Email == userEmail || u.Email == userEmail);

                if (user == null)
                {
                    // 如果在資料庫中找不到使用者，創建新的使用者記錄
                    user = new TCustomers
                    {
                        Name = userName,
                        Email = userEmail,

                    };

                    _dbXContext.Add(user);
                    _dbXContext.SaveChanges();
                }

                var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.CustomerId.ToString()), // 添加用户 ID 作为 subject
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            new Claim("Name", user.Name),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
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

            catch (Exception ex)
            {
                // 返回錯誤訊息
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("FacebookLogin")]
        public async Task<IActionResult> FacebookLogin([FromBody] AccessTokenModel accessTokenModel)
        {
            try
            {
                string accessToken = accessTokenModel.AccessToken;
                string appId = _configuration["FacebookAuthSettings:AppId"];
                string appSecret = _configuration["FacebookAuthSettings:AppSecret"];
                string verifyTokenUrl = $"https://graph.facebook.com/debug_token?input_token={accessToken}&access_token={appId}|{appSecret}";

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(verifyTokenUrl);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await httpClient.GetAsync("");

                    if (response.IsSuccessStatusCode)
                    {
                        // 驗證成功，繼續處理登錄流程
                        string userId = ""; // 從 Facebook API 獲取的用戶 ID
                        string userEmail = ""; // 從 Facebook API 獲取的用戶電子郵件
                        string userName = ""; // 從 Facebook API 獲取的用戶名稱
                        // 根據 access token 獲取用戶資訊
                        string userInfoUrl = $"https://graph.facebook.com/me?fields=id,email&access_token={accessToken}";
                        HttpResponseMessage userInfoResponse = await httpClient.GetAsync(userInfoUrl);
                        if (userInfoResponse.IsSuccessStatusCode)
                        {
                            var userInfo = await userInfoResponse.Content.ReadAsAsync<JObject>();
                            userId = userInfo["id"].ToString();
                            userEmail = userInfo["email"].ToString();
                            userName = userInfo["email"].ToString();
                            // 在資料庫中查找使用者
                            var user = _dbXContext.TCustomers.FirstOrDefault(u => u.Email == userEmail);
                            if (user == null)
                            {
                                // 如果在資料庫中找不到使用者，創建新的使用者記錄
                                user = new TCustomers
                                {
                                    Name = userName,
                                    Email = userEmail,

                                };
                                _dbXContext.Add(user);
                                _dbXContext.SaveChanges();
                            }
                        }
                        else
                        {
                            return BadRequest("Failed to retrieve user information.");
                        }

                        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), // 添加用户 ID 作为 subject
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            new Claim("Name", userName),
            new Claim(JwtRegisteredClaimNames.Email, userEmail)
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
                    else
                    {
                        // 驗證失敗
                        return BadRequest("Invalid access token.");
                    }
                }
            }
            catch (Exception ex)
            {
                // 返回錯誤訊息
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
