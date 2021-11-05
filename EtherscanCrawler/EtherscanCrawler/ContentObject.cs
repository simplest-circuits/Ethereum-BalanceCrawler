namespace EtherscanCrawler
{
    public class ContentObject
    {
        public int Rank { get; set; }
        public string Address { get; set; }
        public string NameTag { get; set; }
        public string Balance { get; set; }
        public string Percentage { get; set; }
        public string TxnCount { get; set; }

        public bool IsContract { get; set; }
    }
}
