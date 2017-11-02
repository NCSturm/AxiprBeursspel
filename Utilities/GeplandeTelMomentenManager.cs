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
        public static bool IsMarktDicht { get; private set; }
        private static GeplandTelMoment _huidigTelMoment;

        private static async Task CreateCache()
        {
            using (var db = new ApplicationDbContext())
            {
                _cached = await db.GeplandeTelMomenten.OrderByDescending(x => x.Tijd).ToListAsync();
            }
        }

        public static async Task CheckMarktSluiting()
        {
            if (_cached == null)
                await CreateCache();
            Debug.Assert(_cached != null, nameof(_cached) + " != null");
            var datum = DateTime.Now.Date;
            foreach (var geplandTelMoment in _cached)
            {
                if (geplandTelMoment.Tijd.Date == datum && DateTime.Now.Hour >= 18)
                {
                    IsMarktDicht = true;
                    _huidigTelMoment = geplandTelMoment;
                    break;
                }
            }
        }

        public static async Task CheckMarktOpening()
        {
            if (_cached == null)
                await CreateCache();
            Debug.Assert(_cached != null, nameof(_cached) + " != null");
            if (!IsMarktDicht)
                return;
            if (_cached.All(x => x.GeplandTelMomentId != _huidigTelMoment.GeplandTelMomentId))
            {
                IsMarktDicht = false;
            }
            if (DateTime.Now.Date > _huidigTelMoment.Tijd.Date && DateTime.Now.Hour > 2)
            {
                IsMarktDicht = false;
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
            await CheckMarktSluiting();
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
            await CheckMarktOpening();
        }
    }
}