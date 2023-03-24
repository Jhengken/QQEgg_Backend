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

namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleController : ControllerBase
    {
        /// <summary>
        /// 驗證 Google 登入授權
        /// </summary>
        /// <returns></returns>
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory httpClientFactory;
        public GoogleController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            if (!this.Request.Query.TryGetValue("code", out var code))
            {
                return this.StatusCode(400);
            }

            var (accessToken, idToken) = await this.ExchangeAccessToken(code);

            if (accessToken == null)
            {
                return this.StatusCode(400);
            }
            else
            {
                return Ok("我收到了");
            }
            // TODO: Save AccessToken and IdToken
            
            // TODO: User Login

            return this.Redirect("/");
        }

        private async Task<(string, string)> ExchangeAccessToken(string code)
        {
            var client = this.httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "AccessTokenUrl");

            request.Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "authorization_code",
                    ["code"] = code,
                    ["redirect_uri"] = "RedirectURI",
                    ["client_id"] = "ClientId",
                    ["client_secret"] = "ClientSecret"
                });

            var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK) return (null, null);

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonNode.Parse(content);

            return (result["access_token"].GetValue<string>(), result["id_token"].GetValue<string>());
        }
    }
}
