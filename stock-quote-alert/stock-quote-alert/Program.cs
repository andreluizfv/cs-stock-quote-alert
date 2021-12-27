
using System.Globalization;

namespace stock_quote_alert
{
    public enum Decision
    {
        Buy,
        Sell
    }
    public class Program
    {
        public static Tuple<string, Decision> MakePair(string key1, Decision key2)
        {
            return Tuple.Create(key1, key2);
        }
        public static void Main(string[] args)
        {
            var alreadyInformed = new Dictionary<Tuple<string, Decision>, bool>();
            DateTime localDate = DateTime.Now;
            CultureInfo culture = new CultureInfo("pt-BR");
            int checkDelay = 300000;
            TimeSpan timeSpam = new TimeSpan(0, 0, checkDelay / 1000);
            MailHandler mailHandler;
            QuoteHandler quoteHandler;
            string pathAddFile = "..\\..\\..\\emails.txt", pathSettF = "..\\..\\..\\smtp_settings.json";
            FileInfo fInfo = new FileInfo(pathAddFile);


            if (!fInfo.Exists)
            {
                throw new FileNotFoundException("The file with emails was not found.");
            }

            fInfo = new FileInfo(pathSettF);

            if (!fInfo.Exists)
            {
                throw new FileNotFoundException("The file with smtp settings was not found.");
            }
            else
            {
                mailHandler = new MailHandler(new StreamReader(pathAddFile), new StreamReader(pathSettF));
            }

            if (args.Count() < 3 || args.Count() % 3 != 0)
            {
                throw new ArgumentException("Arguments were not properly sent (correct example: .\\projeto_cotacao_email.exe MGLU3 8.15 8.25 PETR4 25 26).");
            }
            else quoteHandler = new QuoteHandler(args);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Monitoring at " + localDate.ToString(culture)+  " prices of:");
                for (int i = 0; i < quoteHandler.stocks.Count(); i++)
                {
                    try
                    {
                        quoteHandler.UpdatePrice(i);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("There was an error: " + e.Message + $"\n{quoteHandler.stocks[i].name} was jumped.");
                        continue;
                    }
                    Console.WriteLine(quoteHandler.stocks[i].name + " : " + quoteHandler.stocks[i].current);
                    if (quoteHandler.stocks[i].current < quoteHandler.stocks[i].min &&
                        !alreadyInformed.GetValueOrDefault(MakePair(quoteHandler.stocks[i].name, Decision.Buy)))
                    {
                        Console.WriteLine("sending email: time to buy " + quoteHandler.stocks[i].name);
                        mailHandler.InformAlert(quoteHandler.stocks[i].name, quoteHandler.stocks[i].current, quoteHandler.stocks[i].min);
                        alreadyInformed[MakePair(quoteHandler.stocks[i].name, Decision.Buy)] = true;
                    }
                    else if (quoteHandler.stocks[i].current > quoteHandler.stocks[i].max &&
                        !alreadyInformed.GetValueOrDefault(MakePair(quoteHandler.stocks[i].name, Decision.Sell)))
                    {
                        Console.WriteLine("sending email: time to sell " + quoteHandler.stocks[i].name);
                        mailHandler.InformAlert(quoteHandler.stocks[i].name, quoteHandler.stocks[i].current, quoteHandler.stocks[i].max);
                        alreadyInformed[MakePair(quoteHandler.stocks[i].name, Decision.Sell)] = true;
                    }
                    else if ((quoteHandler.stocks[i].min < quoteHandler.stocks[i].current &&
                        quoteHandler.stocks[i].current < quoteHandler.stocks[i].max))
                    {
                        if (alreadyInformed.GetValueOrDefault(MakePair(quoteHandler.stocks[i].name, Decision.Buy)))
                        {
                            Console.WriteLine("sending email: no longer is time to buy " + quoteHandler.stocks[i].name);
                            mailHandler.InformDefaultPrice(quoteHandler.stocks[i].name, quoteHandler.stocks[i].current, Decision.Buy);
                            alreadyInformed[MakePair(quoteHandler.stocks[i].name, Decision.Buy)] = false;
                        }
                        if (alreadyInformed.GetValueOrDefault(MakePair(quoteHandler.stocks[i].name, Decision.Sell)))
                        {
                            Console.WriteLine("sending email: no longer is time to sell " + quoteHandler.stocks[i].name);
                            mailHandler.InformDefaultPrice(quoteHandler.stocks[i].name, quoteHandler.stocks[i].current, Decision.Sell);
                            alreadyInformed[MakePair(quoteHandler.stocks[i].name, Decision.Sell)] = false;
                        }
                    }
                }
                Console.WriteLine("Next check at " + localDate.Add(timeSpam).ToString(culture));
                Thread.Sleep(checkDelay);
            }
        }
    }
}
