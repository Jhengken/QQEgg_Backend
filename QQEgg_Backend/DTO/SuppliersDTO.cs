using QQEgg_Backend.Abstract;
using System.Text.Json.Serialization;

namespace QQEgg_Backend.DTO
{
    public class SuppliersDTO: SupplierAbstractDTOValidation
    {
        public string? Password { get; set; }
		[JsonIgnore]
		public string? PasswordHash { get; set; } // 新增一個屬性用來表示加密後的密碼

		public void EncryptPassword()
		{
			PasswordHash = BCrypt.Net.BCryptHlper.HashPassword(Password);// 在註冊時將密碼加密後存入 EncryptedPassword 屬性中
		}

	}
}
