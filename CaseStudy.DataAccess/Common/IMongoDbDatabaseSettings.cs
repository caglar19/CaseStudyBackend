namespace CaseStudy.DataAccess.Common;

public interface IMongoDbDatabaseSettings
{
    string ConnectionString { get; set; }
    string Database { get; set; }
}