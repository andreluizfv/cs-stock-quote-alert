using System.Text.Json;
using System.Net.Mail;
using System.Net;

namespace stock_quote_alert
{
    public class SmtpSettings
    {
        public string SmtpServer { get; set; }
        public string SenderMail { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool UseDefaultCredentials { get; set; }

    }
    internal class MailHandler
    {
        private SmtpSettings _smtpSettings;
        public SmtpClient Smtp { get; set; } = new SmtpClient();
        public MailMessage Message { get; set; } = new MailMessage();

        public MailHandler(StreamReader email_file, StreamReader settings_file)
        {
            this._smtpSettings = JsonSerializer.Deserialize<SmtpSettings>(settings_file.ReadToEnd());
            string newmail;
            newmail = email_file.ReadLine();
            while (newmail != null)
            {
                this.Message.To.Add(new MailAddress(newmail));
                newmail = email_file.ReadLine();
            }
            this.Message.From = new MailAddress(this._smtpSettings.SenderMail);
            this.Message.IsBodyHtml = true;
            this.Smtp.Port = _smtpSettings.Port;
            this.Smtp.Host = _smtpSettings.SmtpServer;
            this.Smtp.EnableSsl = _smtpSettings.EnableSsl;
            this.Smtp.UseDefaultCredentials = _smtpSettings.UseDefaultCredentials;
            this.Smtp.Credentials = new NetworkCredential(_smtpSettings.SenderMail, _smtpSettings.Password);
            this.Smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

            email_file.Close();
            settings_file.Close();
        }
        public void InformAlert(string name, float value, float lim)
        {
            string str_curr_v = value.ToString("n2");
            string lim_string = lim.ToString("n2");
            if (value < lim)
            {
                this.Message.Subject = ("Stock market Warning: time to buy " + name);
                this.Message.Body = name + " reached " + str_curr_v +
                    "R$, less than the " + lim_string + "R$ limit, so it is a good time to buy it.";
            }
            else
            {
                this.Message.Subject = "Stock market Warning: time to sell " + name;
                this.Message.Body = name + " reached " + str_curr_v +
                    "R$, more than the " + lim_string + "R$ limit, so it is a good time to sell it.";
            }
            try
            {
                this.Smtp.Send(this.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("There was a problem on sending: " + e.ToString());
            }
        }
        public void InformDefaultPrice(string name, float current, Decision decision)
        {
            if (decision == Decision.Buy)
            {
                this.Message.Subject = "An warning Expired: no more is time to buy " + name;
                this.Message.Body = name + " raised to " + current.ToString("n2") +
                    "R$, thus no more is a good time to buy it.";
            }
            else
            {
                this.Message.Subject = "An warning Expired: no more is time to sell " + name;
                this.Message.Body = name + " raised to " + current.ToString("n2") +
                    "R$, thus no more is a good time to sell it.";
            }
            try
            {
                this.Smtp.Send(this.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("There was a problem on sending: " + e.ToString());
            }
        }
    }
}
