using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TgTranslator.Models
{
    public class Group
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("GroupId")]
        public long groupGroupId;
        
        [BsonElement("Language")]
        public string Language;

        public Group(long groupId, string language = "en")
        {
            groupGroupId = groupId;
            Language = language;
        }

        public Group(string objectId, long groupId, string language = "en")
        {
            _id = objectId;
            groupGroupId = groupId;
            Language = language;
        }
    }
}