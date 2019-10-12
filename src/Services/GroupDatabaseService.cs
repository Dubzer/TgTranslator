using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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

        public async Task<Group> Get(long chatId) => await _groups.AsQueryable()
                .Where(group => group.Id == chatId)
                .FirstOrDefaultAsync();

        public async Task<Group> Create(Group group)
        {
            await _groups.InsertOneAsync(group);
            return group;
        }

        public Task UpdateLanguage(Group groupIn, string language) =>
            _groups.ReplaceOneAsync(group => group.Id == groupIn.Id, new Group(groupIn.objectId, groupIn.Id, language));
    }
}