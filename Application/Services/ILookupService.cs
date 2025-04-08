using Ardalis.Result;
using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Application.Abstractions.Services;
using System.Threading;

namespace Application.Services;
public interface ILookupService
{
    Task InitializeAsync();
    bool IsValid(string typeName, int id);
    Task<int> GetLookupIdAsync(string lookupName);
    ValidationError ValidateLookup(string typeName, int id);
    Task<int> GetTypeIdAsync(string typeName);
    Task<int> GetLookupIdByTypeAndNameAsync(int typeId, string lookupName);
    Task<string> GetFirstLookupNameByTypeAsync(int typeId);
    Task<List<Lookup>> GetAllLookupsByType(string type);
    Task<Lookup> GetLookupById(int id);
    Task<Lookup> GetLookupByName(string name);
    Task<Lookup> GetLookupByTypeAndName(string type, string name);
    Task<bool> IsLookupExists(string type, string name);
    Task<Lookup> CreateLookup(Lookup lookup);
    Task<Lookup> UpdateLookup(Lookup lookup);
    Task<bool> DeleteLookup(int id);
}

public class LookupService : ILookupService
{
    private readonly IContext _context;
    private Dictionary<string, HashSet<int>> _lookupCache;
    private Dictionary<string, int> _lookupNameIdCache;

    public LookupService(IContext context)
    {
        _context = context;
        _lookupCache = new Dictionary<string, HashSet<int>>();
        _lookupNameIdCache = new Dictionary<string, int>();
    }

    public async Task InitializeAsync()
    {
        var lookupTypes = await _context.LookupTypes
            .Include(lt => lt.Lookups)
            .ToListAsync();

        foreach (var lookupType in lookupTypes)
        {
            var normalizedTypeName = NormalizeString(lookupType.Name);
            _lookupCache[normalizedTypeName] = new HashSet<int>();

            foreach (var lookup in lookupType.Lookups)
            {
                _lookupCache[normalizedTypeName].Add(lookup.Id);
                _lookupNameIdCache[NormalizeString(lookup.Name)] = lookup.Id;
            }
        }
    }

    public bool IsValid(string typeName, int id)
    {
        var normalizedTypeName = NormalizeString(typeName);
        return _lookupCache.ContainsKey(normalizedTypeName) && _lookupCache[normalizedTypeName].Contains(id);
    }

    public async Task<int> GetLookupIdAsync(string lookupName)
    {
        var normalizedLookupName = NormalizeString(lookupName);
        if (_lookupNameIdCache.TryGetValue(normalizedLookupName, out int id))
        {
            return id;
        }

        // If not found in cache, try to fetch from database
        var lookup = await _context.Lookups
            .FirstOrDefaultAsync(l => l.Name.Replace(" ", "").ToLower() == normalizedLookupName);

        if (lookup != null)
        {
            _lookupNameIdCache[normalizedLookupName] = lookup.Id;
            return lookup.Id;
        }

        throw new KeyNotFoundException($"Lookup with name '{lookupName}' not found.");
    }

    public ValidationError ValidateLookup(string typeName, int id)
    {
        if (!IsValid(typeName, id))
        {
            return new ValidationError
            {
                Identifier = "Unable to Create",
                ErrorMessage = $"Invalid {typeName}"
            };
        }

        return null;
    }
    public async Task<int> GetTypeIdAsync(string typeName)
    {
        var normalizedTypeName = NormalizeString(typeName);
        var lookupType = await _context.LookupTypes
            .FirstOrDefaultAsync(t => t.Name.Replace(" ", "").ToLower() == normalizedTypeName);

        if (lookupType == null)
        {
            throw new KeyNotFoundException($"Type '{typeName}' not found.");
        }

        return lookupType.Id;
    }

    public async Task<int> GetLookupIdByTypeAndNameAsync(int typeId, string lookupName)
    {
        var normalizedLookupName = NormalizeString(lookupName);
        var lookup = await _context.Lookups
            .FirstOrDefaultAsync(l => l.TypeId == typeId &&
                                    l.Name.Replace(" ", "").ToLower() == normalizedLookupName);

        if (lookup == null)
        {
            throw new KeyNotFoundException($"Lookup '{lookupName}' not found for type ID {typeId}.");
        }

        return lookup.Id;
    }
    public async Task<string> GetFirstLookupNameByTypeAsync(int typeId)
    {
        var firstLookup = await _context.Lookups
            .Where(l => l.TypeId == typeId)
            .OrderBy(l => l.Id)  // Ensures consistent results by ordering by Id
            .Select(l => l.Name)
            .FirstOrDefaultAsync();

        if (firstLookup == null)
        {
            throw new KeyNotFoundException($"No lookups found for type ID {typeId}.");
        }

        return firstLookup;
    }
    private string NormalizeString(string input)
    {
        return input.Replace(" ", "").ToLower();
    }

    public async Task<List<Lookup>> GetAllLookupsByType(string type)
    {
        return await _context.Lookups
            .Include(x => x.Type)
            .Where(x => x.Type.Name == type)
            .ToListAsync();
    }

    public async Task<Lookup> GetLookupById(int id)
    {
        return await _context.Lookups
            .Include(x => x.Type)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Lookup> GetLookupByName(string name)
    {
        return await _context.Lookups
            .Include(x => x.Type)
            .FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<Lookup> GetLookupByTypeAndName(string type, string name)
    {
        return await _context.Lookups
            .Include(x => x.Type)
            .FirstOrDefaultAsync(x => x.Type.Name == type && x.Name == name);
    }

    public async Task<bool> IsLookupExists(string type, string name)
    {
        return await _context.Lookups
            .Include(x => x.Type)
            .AnyAsync(x => x.Type.Name == type && x.Name == name);
    }

    public async Task<Lookup> CreateLookup(Lookup lookup)
    {
        _context.Lookups.Add(lookup);
        await _context.SaveChangesAsync();
        return lookup;
    }

    public async Task<Lookup> UpdateLookup(Lookup lookup)
    {
        _context.Lookups.Update(lookup);
        await _context.SaveChangesAsync();
        return lookup;
    }

    public async Task<bool> DeleteLookup(int id)
    {
        var lookup = await GetLookupById(id);
        if (lookup == null)
            return false;

        _context.Lookups.Remove(lookup);
        await _context.SaveChangesAsync();
        return true;
    }
}
