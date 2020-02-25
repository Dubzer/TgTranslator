using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TgTranslator.Menu;
using TgTranslator.Models;

namespace TgTranslator.Services
{
    public class GroupDatabaseService
    {
        private readonly IMongoCollection<Group> _groups;

        public GroupDatabaseService(IGroupDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            IMongoDatabase database = client.GetDatabase(settings.DatabaseName);

            _groups = database.GetCollection<Group>(settings.GroupsCollectionName);
        }

        public async Task<Group> Get(long chatId)
        {
            Group group = await _groups.AsQueryable().Where(grp => grp.Id == chatId)
                .FirstOrDefaultAsync() ?? await Create(new Group(chatId));

            return group;
        }


        public async Task<Group> Create(Group group)
        {
            await _groups.InsertOneAsync(group);
            return group;
        }

        public Task UpdateLanguage(Group groupIn, string language) =>
            _groups.ReplaceOneAsync(group => group.Id == groupIn.Id, new Group(groupIn.objectId, groupIn.Id, language));

        public Task UpdateMode(Group groupIn, TranslationMode mode) =>
            _groups.ReplaceOneAsync(group => group.Id == groupIn.Id, new Group(groupIn.objectId, groupIn.Id, groupIn.Language, mode));
    }
}