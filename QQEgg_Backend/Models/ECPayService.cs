using QQEgg_Backend.DTO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace QQEgg_Backend.Models
{
    public class ECPayService
    {
        public IConfiguration config { get; set; }
        public ECPayService()
        {
            config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
        }

        public string GetReturnValue(ECPayDetail detail)
        {
            detail.MerchantID = config["MerchantID"];
            detail.MerchantTradeNo = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
            detail.MerchantTradeDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            detail.ReturnURL = $"{config["HostURL"]}/api/ECPay/ECPayReturn";
            //付款完成後導入的網址
            //detail.OrderResultURL = $"{config["HostURL"]}/api/ECPay/ECPayReturn";  //要看綠界回傳打開這行

            //CheckMacValue檢查碼
            //裡面有順序跟加上HashKey、IV、UrlEndcode、雜湊
            Dictionary<string, string> dic = ChangeForDictionary(detail);
            dic["CheckMacValue"] = GetCheckMacValue(dic);
            StringBuilder strForm = new StringBuilder();
            strForm.AppendFormat("<form id='payForm' action='{0}' method='post'>", "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5");
            foreach (KeyValuePair<string, string> kvp in dic)
            {
                strForm.AppendFormat($"<input type='hidden' name='{kvp.Key}' value='{kvp.Value}' />");
            }
            strForm.Append("</form>");

            return strForm.ToString();
        }

        private Dictionary<string, string> ChangeForDictionary(ECPayDetail detail)
        {
            Type t = detail.GetType();
            PropertyInfo[] ps = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Dictionary<string, string> order = new Dictionary<string, string>();
            foreach (PropertyInfo p in ps)
            {
                string key = p.Name;
                object o = p.GetValue(detail);
                string value = "";
                if (o != null)
                    value = o.ToString();
                order.Add(key, value);
            }

            return order;
        }

        private string? GetCheckMacValue(Dictionary<string, string> dic)
        {
            //做成string list，排順序順便加上等號
            var para = dic.Keys.OrderBy(x => x).Select(key => $"{key}={dic[key]}");
            string checkValue = String.Join("&", para);

            string HashKey = $"HashKey={config["HashKey"]}&";
            string HashIV = $"&HashIV={config["HashIV"]}";
            checkValue = HashKey + checkValue + HashIV;

            //規定要UrlEncode後轉小寫，雜湊後再轉大寫
            checkValue = HttpUtility.UrlEncode(checkValue).ToLower();
            checkValue = EncryptSHA256(checkValue);

            return checkValue.ToUpper();
        }

        private string EncryptSHA256(string checkValue)
        {
            string result = "";
            using (SHA256 myHash256 = SHA256.Create())
            {
                byte[] cvByte = Encoding.UTF8.GetBytes(checkValue);
                byte[] hash = myHash256.ComputeHash(cvByte);
                if (hash != null)
                {
                    result = BitConverter.ToString(hash).Replace("-", "");
                }
            }

            return result;
        }

        //拿回來的form加密成checkMacValue後跟他來的checkMacValue比對，如果一樣就回傳1|OK
        public ECPayResult GetCallbackResult(IFormCollection form)
        {
            ECPayResult result = new ECPayResult();
            StringBuilder receive =new StringBuilder();

            //查看拿回來的資料
            var dic = form.ToDictionary(p => p.Key, p => p.Value).OrderBy(o => o.Key);
            foreach (var item in dic)
                if (item.Key != "CheckMacValue")
                    receive.AppendFormat(item.Key + "=" + item.Value + "&");
            result.ReceiveObj =receive.ToString().Replace("&", "\r\n");

            //解密CheckMacValue
            string HashKey = $"HashKey={config["HashKey"]}";
            string HashIV = $"HashIV={config["HashIV"]}";
            string checkValue = HashKey + "&"+ receive.ToString()+ HashIV;
            checkValue = HttpUtility.UrlEncode(checkValue).ToLower();
            checkValue = EncryptSHA256(checkValue).ToUpper();
            if (checkValue == form["CheckMacValue"])
                result.ResponseECPay = "1|OK";
            else
                result.ResponseECPay = "0|ERROR";
            return result;
        }

    }
}
