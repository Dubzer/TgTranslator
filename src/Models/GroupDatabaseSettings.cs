namespace TgTranslator.Models
{
    public class GroupDatabaseSettings : IGroupDatabaseSettings
    {
        public string GroupsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IGroupDatabaseSettings
    {
        string GroupsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}