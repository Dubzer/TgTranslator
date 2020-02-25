namespace TgTranslator.Models
{
    public class GroupDatabaseSettings : IGroupDatabaseSettings
    {
        #region IGroupDatabaseSettings Members

        public string GroupsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }

        #endregion
    }

    public interface IGroupDatabaseSettings
    {
        string GroupsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}