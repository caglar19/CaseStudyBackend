namespace CaseStudy.DataAccess.Common.Impl;

public class MongoDbDatabaseSettings : IMongoDbDatabaseSettings
{
    public required string ConnectionString { get; set; }
    public required string Database { get; set; }
}