using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TgTranslator.Menu;

namespace TgTranslator.Models
{
    public class Group
    {
        [BsonId] [BsonRepresentation(BsonType.ObjectId)]
        public string objectId;

        [BsonElement("Id")]
        public long Id;

        [BsonElement("Language")]
        public string Language;
        
        [BsonElement("Mode")]
        public TranslationMode Mode;
        
        public Group(long chatId, string language = "en", TranslationMode mode = TranslationMode.Auto)
        {
            Id = chatId;
            Language = language;
            Mode = mode;
        }
        
        public Group(string objectId, long chatId, string language = "en", TranslationMode mode = TranslationMode.Auto)
        {
            this.objectId = objectId;
            Id = chatId;
            Language = language;
            Mode = mode;
        }

    }
}