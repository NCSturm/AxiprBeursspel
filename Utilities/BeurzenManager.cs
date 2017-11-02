using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Beursspel.Data;
using Beursspel.Models.Beurzen;
using Microsoft.EntityFrameworkCore;

namespace Beursspel.Utilities
{
    public static class BeurzenManager
    {
        private static ConcurrentDictionary<int, Beurs> _cached;

        private static async Task CreateCache()
        {
            using (var db = new ApplicationDbContext())
            {
                var beurzen = (await db.Beurzen.Include(x => x.Waardes)
                    .ToDictionaryAsync(x => x.BeursId, x => x));
                _cached = new ConcurrentDictionary<int, Beurs>(beurzen);
            }
            foreach (var cachedKey in _cached.Keys)
            {
                if (_cached.TryGetValue(cachedKey, out var beurs))
                {
                    beurs.Waardes.Sort((x, y) => DateTime.Compare(x.Tijd, y.Tijd));
                }
            }
        }

        public static async Task ClearCache()
        {
            _cached = null;
        }

        public static async Task SetCache(IEnumerable<Beurs> beurzen)
        {
            _cached = new ConcurrentDictionary<int, Beurs>(beurzen.ToDictionary(x => x.BeursId, x => x));
            foreach (var cachedKey in _cached.Keys)
            {
                if (_cached.TryGetValue(cachedKey, out var beurs))
                {
                    beurs.Waardes.Sort((x, y) => DateTime.Compare(x.Tijd, y.Tijd));
                }
            }
        }

        public static async Task AddBeurs(Beurs beurs)
        {
            if (_cached == null)
                await CreateCache();
            Debug.Assert(_cached != null, nameof(_cached) + " != null");
            _cached.TryAdd(beurs.BeursId, beurs);
        }

        public static async Task ModifyBeurs(Beurs beurs)
        {
            if (_cached == null)
                await CreateCache();
            Debug.Assert(_cached != null, nameof(_cached) + " != null");
            _cached[beurs.BeursId] = beurs;
        }

        public static void DeleteBeurs(int id)
        {
            if (_cached == null)
                return;
            _cached.TryRemove(id, out _);
        }

        public static async Task<List<Beurs>> GetBeurzenAsync()
        {
            if (_cached == null)
            {
                await CreateCache();
            }
            Debug.Assert(_cached != null, nameof(_cached) + " != null");
            return _cached.Values.ToList();
        }

        public static async Task<Beurs> GetBeursAsync(int id)
        {
            if (_cached == null)
            {
                await CreateCache();
            }
            Debug.Assert(_cached != null, nameof(_cached) + " != null");
            return _cached.TryGetValue(id, out var beurs) ? beurs : null;
        }

        public static async Task<Beurs> GetBeursMetNaamAsync(string naam)
        {
            if (_cached == null)
            {
                await CreateCache();
            }
            Debug.Assert(_cached != null, nameof(_cached) + " != null");
            return _cached
                .FirstOrDefault(x => string.Equals(x.Value.Naam, naam, StringComparison.CurrentCultureIgnoreCase))
                .Value;
        }
    }
}