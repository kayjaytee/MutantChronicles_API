using MongoDB.Bson;
using MongoDB.Driver;
using MutantChroniclesAPI.Model.WeaponModel;

namespace MutantChroniclesAPI.Interface;

public interface IWeaponService
{
    public Task<List<Weapon>> GetAsync();
    public Task<Weapon> GetByNameAsync(string name);
    public Task<List<Weapon>> SearchAsync(string query);
}
