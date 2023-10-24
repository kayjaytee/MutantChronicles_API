using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MutantChroniclesAPI.Model.WeaponModel;
using MutantChroniclesAPI.Services.Data;

namespace MutantChroniclesAPI.Services;

public class WeaponService
{

    private readonly IMongoCollection<Weapon> _weaponsCollection;

    public WeaponService(IOptions<MongoDBSettings> mongoDBSettings)
    {
        var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _weaponsCollection = mongoDatabase.GetCollection<Weapon>(mongoDBSettings.Value.CollectionName);
    }


    public async Task<List<Weapon>> GetAsync()
    {
        var projection = Builders<Weapon>.Projection.Exclude("_id");

        return await _weaponsCollection.Find(new BsonDocument())
                                       .Project<Weapon>(projection)
                                       .ToListAsync();
    }


    public async Task<Weapon> GetByNameAsync(string name)
    {
        var filter = Builders<Weapon>.Filter.Eq("Name", name);
        var projection = Builders<Weapon>.Projection.Exclude("_id");

        var exactMatchWeapon = await _weaponsCollection.Find(filter)
                                                       .Project<Weapon>(projection)
                                                       .FirstOrDefaultAsync();

        if (exactMatchWeapon is not null)
        {
            return exactMatchWeapon;
        }
        
        return null;
    }

    private async Task SuggestSimilarWeaponsAsync(string name)
    {
        var suggestedWeapons = await SearchAsync(name);

        if (suggestedWeapons.Count > 0)
        {
            string suggestedNames = string.Join(", ", suggestedWeapons.Select(w => $"{w.Name} ({w.Description})"));
            throw new Exception($"Did not find the specified weapon. Did you mean {suggestedNames}?");
        }
    }

    public async Task<List<Weapon>> SearchAsync(string query)
    {
        var regexPattern = new BsonRegularExpression(query, "i"); // "i" for case-insensitive search
        var filter = Builders<Weapon>.Filter.Or(
            Builders<Weapon>.Filter.Regex("Name", regexPattern),
            Builders<Weapon>.Filter.Regex("Description", regexPattern)
        );

        var projection = Builders<Weapon>.Projection.Exclude("_id");

        return await _weaponsCollection.Find(filter)
                                       .Project<Weapon>(projection)
                                       .ToListAsync();
    }

    //public async Task CreateAsync(Weapon weapon) =>
    //    await _weaponsCollection.InsertOneAsync(weapon);

    //public async Task UpdateAsync(string id, Weapon weapon) =>
    //    await _weaponsCollection.ReplaceOneAsync(x => x.Id == id, weapon);

    //public async Task RemoveAsync(string id) =>
    //    await _weaponsCollection.DeleteOneAsync(x => x.Id == id);
}

