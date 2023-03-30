using Microsoft.AspNetCore.Mvc;
using OpenAI_API.Completions;
using OpenAI_API;
using QQEgg_Backend.DTO;
using OpenAI_API.Models;

namespace QQEgg_Backend.Controllers
{
    public class CheatGptController : Controller
    {
        [HttpPost]
        [Route("getanswer")]
        public IActionResult Post([FromBody] ChatbotRequestDto request)
        {
            string apiKey = "";
            string answer = string.Empty;
            var openai = new OpenAIAPI(apiKey);
            CompletionRequest completion = new CompletionRequest
            {
                Prompt = $"使用繁體中文進行對話\n\nUser: {request.Text}\nAI:",
                Model = Model.DavinciText,
                MaxTokens = 4000
            };
            var result = openai.Completions.CreateCompletionAsync(completion).Result;

            if (result != null)
            {
                foreach (var item in result.Completions)
                {
                    answer = item.Text;
                }
                return Ok(answer);
            }
            else
            {
                return BadRequest("Not found");
            }
        }
    }
}
