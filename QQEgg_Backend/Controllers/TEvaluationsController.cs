using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QQEgg_Backend.DTO;
using QQEgg_Backend.Models;

namespace QQEgg_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TEvaluationsController : ControllerBase
    {
        private readonly dbXContext _dbXContext;

        public TEvaluationsController(dbXContext dbXContext)
        {
            _dbXContext = dbXContext;
        }


        /// <summary>
        /// 點選房間後取得id資料，把id資料放進來做查詢，查到結果逐一列出
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Evaluations/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IEnumerable<EvaluationDTO>> GetTEvaluations(int id)
        {
            var room = await _dbXContext.TPsiteRoom.FindAsync(id);

            if (room != null)
            {
                var result = _dbXContext.TEvaluations.Include(e => e.Customer).Include(e => e.Title).Select(e => new EvaluationDTO
                {
                    CustomerName = e.Customer.Name,
                    Title = e.Title.TitleName,
                    Date = e.Date,
                    Description = e.Description,
                    Star = e.Star,
                });
                return await result.ToListAsync();
            }
            return null;
        }
        /// <summary>
        /// 讀到RoomID資料，顧客在此房間評論回傳到sever做修改
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tEvaluations"></param>
        /// <returns></returns>
        // PUT: api/Evaluations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<string> PutTEvaluations(int id, EvaluationDTO Evaluations)
        {
            var room = await _dbXContext.TPsiteRoom.FindAsync(id);
            if (id != Evaluations.RoomID)
            {
                return "找不到資料";
            }
            var result = _dbXContext.TEvaluations.Include(e => e.Customer).Include(e => e.Title).Select(e => new EvaluationDTO
            {
                CustomerName = e.Customer.Name,
                Title = e.Title.TitleName,
                Date = DateTime.Now,
                Description = e.Description,
                Star = e.Star,
            });
            _dbXContext.Entry(result).State = EntityState.Modified;
            await _dbXContext.SaveChangesAsync();
            return "發送成功";
        }


        /// <summary>
        /// 使用者把資料存進去
        /// </summary>
        /// <param name="evaluationDTO"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<TEvaluations>> PostTEvaluations(EvaluationDTO evaluationDTO)
        {
            var customerId = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var customer = await (from c in _dbXContext.TCustomers select c).FirstOrDefaultAsync(c => c.Email == customerId);
            if (customer == null)
            {
                return BadRequest("找不到該顧客");
            }

            var title = await (from c in _dbXContext.TEtitle select c).FirstOrDefaultAsync(c => c.TitleId.ToString() == evaluationDTO.Title);
            if (title == null)
            {
                return BadRequest("找不到該標題");
            }

            var tEvaluations = new TEvaluations
            {
                CustomerId = customer.CustomerId,
                TitleId = title.TitleId,
                Description = evaluationDTO.Description,
                Star = evaluationDTO.Star,
            };

            _dbXContext.TEvaluations.Add(tEvaluations);
            await _dbXContext.SaveChangesAsync();

            var result = new EvaluationDTO
            {
                EvaluationId = tEvaluations.EvaluationId,
                CustomerName = customer.Name,
                Title = title.TitleName,
                Date = tEvaluations.Date,
                Description = tEvaluations.Description,
                Star = tEvaluations.Star,
            };

            return CreatedAtAction(nameof(GetTEvaluations), new { id = tEvaluations.EvaluationId }, result);
        }
        // GET: api/TEtitles/Title
        /// <summary>
        /// 讓前端可以選Title規格
        /// </summary>
        /// <returns></returns>
        [HttpGet("Title")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TEtitle>>> GetTEtitle()
        {
            if (_dbXContext.TEtitle == null)
            {
                return NotFound();
            }
            return await _dbXContext.TEtitle.ToListAsync();
        }

        private bool TEvaluationsExists(int id)
        {
            return (_dbXContext.TEvaluations?.Any(e => e.EvaluationId == id)).GetValueOrDefault();
        }
    }
}
