using BCrypt.Net;
using QQEgg_Backend.DTO;
using QQEgg_Backend.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QQEgg_Backend.Abstract
{
    public abstract class CustomerAbstractDTOValidation : IValidatableObject
    {
        public string Name { get; set; }
        [JsonIgnore]
        public bool? Sex { get; set; }
        public string Email { get; set; }

        public string Phone { get; set; }
        public string Password { get; set; }
        [JsonIgnore]
        public DateTime? Birth { get; set; }
        [JsonIgnore]
        public string? CreditCard { get; set; }

        //該[JsonIgnore] 屬性可防止PasswordHash屬性在 api 響應中被序列化和返回
        [JsonIgnore]
        public string? PasswordHash { get; set; } // 新增一個屬性用來表示加密後的密碼

        public void EncryptPassword()
        {
            PasswordHash = BCrypt.Net.BCryptHlper.HashPassword(Password);// 在註冊時將密碼加密後存入 EncryptedPassword 屬性中
        }




        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            dbXContext dbxContext = (dbXContext)validationContext.GetService(typeof(dbXContext));
           
            var FindEmail = from a in dbxContext.TCustomers where a.Email == Email select a;
            var FindPassword = from a in dbxContext.TCustomers where a.Password == BCrypt.Net.BCryptHlper.HashPassword(Password) select a; // 对原始密码进行加密后再查询
            var dto = validationContext.ObjectInstance;
            if (this.GetType() == typeof(CustomersPUTDTO))
            {
                var update = (CustomersPUTDTO)this;
                FindEmail = FindEmail.Where(a => a.Email != update.Email);
                FindPassword = FindPassword.Where(a => a.Password != update.Password); // 这里需要将当前用户排除在外，因为它的密码已经被更新为新密码
            }
            if (FindEmail.FirstOrDefault() != null)
            {
                yield return new ValidationResult("此信箱已被使用", new string[] { "信箱" });
            };
           
        }
    }
}
