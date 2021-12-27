using System.Text.Json.Nodes;
using System.Net;

namespace stock_quote_alert
{
    internal class QuoteHandler
    {
        public List<Stock> stocks { get; set; } = new List<Stock>();
        public static HttpClient client = new HttpClient();
        public QuoteHandler(string[] args)
        {
            for (int counter = 0; counter < args.Count(); counter += 3)
            {
                stocks.Add(new Stock(args[counter], args[counter + 1], args[counter + 2]));
            }
        }


        public void UpdatePrice(int i)
        {
            string searchAddr = "https://brapi.ga/api/quote/" + stocks[i].name;
            WebRequest wrGETURL = WebRequest.Create(searchAddr);
            Stream objStream = wrGETURL.GetResponse().GetResponseStream();
            StreamReader reader = new StreamReader(objStream);
            string jsonStr = reader.ReadToEnd();
            JsonNode resultsNode = JsonNode.Parse(jsonStr);
            stocks[i].current = resultsNode["results"][0]["regularMarketPrice"].GetValue<float>();
        }
    }
    public class Stock
    {
        public string name { get; set; }
        public float current { get; set; }
        public float min { get; set; }
        public float max { get; set; }
        public Stock(string name, string min, string max)
        {
            this.name = name;
            this.min = float.Parse(min);
            this.max = float.Parse(max);
        }
    }
}
