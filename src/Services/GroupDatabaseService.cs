using MongoDB.Driver;
using TgTranslator.Models;

namespace TgTranslator.Services
{
    public class GroupDatabaseService
    { 
        private readonly IMongoCollection<Group> _groups;

        public GroupDatabaseService(IGroupDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _groups = database.GetCollection<Group>(settings.GroupsCollectionName);
        }
        
        public Group Get(long chatId) =>
            _groups.Find(group => group.groupGroupId == chatId).FirstOrDefault();

        public Group Create(Group group)
        {
            _groups.InsertOne(group);
            return group;
        }

        public void UpdateLanguage(Group groupIn, string language) =>
            _groups.ReplaceOne(group => group._id == groupIn._id, new Group(groupIn._id, groupIn.groupGroupId, language));
    }
}