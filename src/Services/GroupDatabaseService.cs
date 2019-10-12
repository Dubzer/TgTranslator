using System.Threading.Tasks;
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
        
        public async Task<Group> Get(long chatId) => await 
            _groups.FindAsync(group => group.groupGroupId == chatId)
                .Result
                .FirstOrDefaultAsync();

        public async Task<Group> Create(Group group)
        {
            await _groups.InsertOneAsync(group);
            return group;
        }

        public Task UpdateLanguage(Group groupIn, string language) =>
            _groups.ReplaceOneAsync(group => group._id == groupIn._id, new Group(groupIn._id, groupIn.groupGroupId, language));
    }
}