using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TgTranslator.Models
{
    public class Group
    {
        [BsonId] [BsonRepresentation(BsonType.ObjectId)]
        public string objectId;

        [BsonElement("Id")]
        public long Id;

        [BsonElement("Language")]
        public readonly string Language;
        
        public Group(long chatId, string language = "en")
        {
            Id = chatId;
            Language = language;
        }
        
        public Group(string objectId, long chatId, string language = "en")
        {
            this.objectId = objectId;
            Id = chatId;
            Language = language;
        }

    }
}