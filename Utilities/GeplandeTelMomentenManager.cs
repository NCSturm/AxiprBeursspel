using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Beursspel.Data;
using Beursspel.Models;
using Microsoft.EntityFrameworkCore;

namespace Beursspel.Utilities
{
    public static class GeplandeTelMomentenManager
    {
        private static List<GeplandTelMoment> _cached;

        private static async Task CreateCache()
        {
            using (var db = new ApplicationDbContext())
            {
                _cached = await db.GeplandeTelMomenten.OrderByDescending(x => x.Tijd).ToListAsync();
            }
        }

        public static async Task AddTelMoment(GeplandTelMoment moment)
        {
            if (_cached == null)
                await CreateCache();
            Debug.Assert(_cached != null, nameof(_cached) + " != null");
            using (var db = new ApplicationDbContext())
            {
                await db.GeplandeTelMomenten.AddAsync(moment);
                await db.SaveChangesAsync();
            }
            _cached.Add(moment);
        }

        public static async Task<List<GeplandTelMoment>> GetGeplandeMomenten()
        {
            if (_cached == null)
                await CreateCache();
            Debug.Assert(_cached != null, nameof(_cached) + " != null");
            return _cached;
        }

        public static async Task DeleteMoment(int id)
        {
            var model = _cached.SingleOrDefault(x => x.GeplandTelMomentId == id);
            if (model == null)
            {
                throw new NullReferenceException("Er bestaat geen gepland moment met id: " + id);
            }
            _cached.Remove(model);
            using (var db = new ApplicationDbContext())
            {
                db.GeplandeTelMomenten.Remove(model);
                await db.SaveChangesAsync();
            }
        }
    }
}