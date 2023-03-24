namespace QQEgg_Backend.DTO
{
    public class ECPayDetail
    {
        public ECPayDetail()
        {
            PaymentType = "aio";
            ChoosePayment = "Credit";
            TradeDesc = "空間類型";
            EncryptType = "1";
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
        public string? Email { get; set; }
        public string? CustomField2 { get; set; }
        public string? CustomField3 { get; set; }
        public string? CustomField4 { get; set; }
    }
}
