namespace QQEgg_Backend.DTO
{
    public class ECPayDetail
    {
        public ECPayDetail()
        {
            MerchantTradeNo = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
            MerchantTradeDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            PaymentType = "aio";
            ChoosePayment = "Credit";
            TradeDesc = "空間類型";
            EncryptType = "1";
            ClientBackURL = $"http://localhost:3000/roomguide/";  //付款完成後導入的網址
        }

        public string? MerchantID { get; set; }
        public string? MerchantTradeNo { get; set; }
        public string? MerchantTradeDate { get; set; }
        public string? PaymentType { get; set; }
        public string? TotalAmount { get; set; }
        public string? TradeDesc { get; set; }
        public string? ItemName { get; set; }
        public string? ChoosePayment { get; set; }
        public string? EncryptType { get; set; }
        public string? ReturnURL { get; set; }
        public string? ClientBackURL { get; set; }  //顧客按下去會導回的Url
        //public string? OrderResultURL { get; set; }  //POST到前端
        public string? CustomField1 { get; set; }
        public string? CustomField2 { get; set; }
        public string? CustomField3 { get; set; }
        public string? CustomField4 { get; set; }
    }
}
