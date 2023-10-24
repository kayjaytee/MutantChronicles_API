namespace MutantChroniclesAPI.Services.Data;

public class MongoDBSettings : IMongoDBSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string CollectionName { get; set; } = null!;
}
