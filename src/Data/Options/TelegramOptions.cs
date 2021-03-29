using System.Collections.Generic;

namespace TgTranslator.Data.Options
{
    public class TelegramOptions
    {
        public string BotToken { get; set; }
        public bool Webhooks { get; set; }
        public string WebhooksDomain{ get; set; }
        public string CustomIpHeader { get; set; }
        public IEnumerable<string> TelegramIpWhitelist { get; set; }
    }
}