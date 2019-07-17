using Newtonsoft.Json;

namespace TgTranslator.Types
{
    public class LanguageJson
    {
        [JsonProperty("language")]
        public Language Language { get; set; }
    }
    public class Language
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("code")]
        public string Code { get; set; }
            
        [JsonProperty("flag")]
        public string Flag { get; set; }
    }
}