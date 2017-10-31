using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beursspel.Data;
using Beursspel.Models.Beurzen;
using Beursspel.Models.TelMomentModels;
using Beursspel.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Beursspel.Berekeningen
{
    public static class Aanwezigheid
    {
        public static async Task BerekenBeurzen(TelMomentHouder telMoment)
        {
            using (var db = new ApplicationDbContext())
            {
                //Haal de beurzen uit de database, itereer over hen
                var beurzen = await db.Beurzen.Include(x => x.Waardes).ToListAsync();
                foreach (var beurs in beurzen)
                {
                    //We moeten zeker zijn dat de waarde geordend zijn op tijd
                    beurs.Waardes = beurs.Waardes.OrderBy(x => x.Tijd).ToList();
                    //als er al een waarde is die uitgerekend is door aanwezigheid, en dezelfde tijd als deze heeft
                    //zijn we deze aan het bewerken
                    if (beurs.Waardes.Any(x => x.Type == BeursWaardes.WaardeType.Aanwezigheid &&
                                               x.Tijd == telMoment.Tijd))
                    {
                        //We pakken de beurs waar het telmoment naar verwijst aan de hand van het beurs id
                        var telBeurs = telMoment.Beurzen.FirstOrDefault(x => x.BeursId == beurs.BeursId);
                        if (telBeurs == null)
                        {
                            throw new NullReferenceException("Beurs is null.");
                        }
                        //pak de telmomenten voor de huidige beurs die niet het huidige moment zijn, en die
                        //voor dit telmoment hebben plaatsgevonden. We nemen hier de laatste drie van.
                        var telMomenten = await db.TelMomentModel.
                            Where(x => x.BeursId == beurs.BeursId && x.TelMomentModelId != telBeurs.TelMomentModelId
                                       && x.Tijd < telMoment.Tijd)
                            .OrderByDescending(x => x.Tijd)
                            .Take(3)
                            .ToListAsync();
                        //bereken de beurswaarde
                        var waarde = BerekenBeurs(beurs, telMomenten, telBeurs);
                        //verwijder de oude waarde
                        beurs.Waardes.Remove(beurs.Waardes
                            .FirstOrDefault(x => x.Type == BeursWaardes.WaardeType.Aanwezigheid &&
                                                 x.Tijd == telMoment.Tijd));
                        //voeg de geupdate waarde toe
                        beurs.Waardes.Add(waarde);
                    }
                    else
                    {
                        //We pakken de beurs waar het telmoment naar verwijst aan de hand van het beurs id
                        var telBeurs = telMoment.Beurzen.FirstOrDefault(x => x.BeursId == beurs.BeursId);
                        if (telBeurs == null)
                        {
                            throw new NullReferenceException("Beurs is null.");
                        }
                        //pak de telmomenten voor de huidige beurs die niet het huidige moment zijn,
                        // We nemen hier de laatste drie van.
                        var telMomenten = await db.TelMomentModel.
                            Where(x => x.BeursId == beurs.BeursId && x.TelMomentModelId != telBeurs.TelMomentModelId)
                            .OrderByDescending(x => x.Tijd)
                            .Take(3)
                            .ToListAsync();
                        //bereken de beurswaarde
                        var waarde = BerekenBeurs(beurs, telMomenten, telBeurs);
                        //voeg de geupdate waarde toe
                        beurs.Waardes.Add(waarde);
                        //update de beurs
                        db.Update(beurs);
                    }
                }
                //verwijder de locale cache, en update deze met de nieuwe beruzen
                await BeurzenManager.SetCache(beurzen);
                //sla de veranderingen in de database op
                await db.SaveChangesAsync();
            }
        }

        private const float InitieleMaatstaaf = 0.3f;

        private static BeursWaardes BerekenBeurs(Beurs beurs,
            List<TelMomentModel> telMomenten, TelMomentModel telMoment)
        {
            var aanwezigheid = (float)telMoment.Aantal / beurs.AantalLeden;
            var huidigeWaarde = beurs.Waardes.Where(x => x.Tijd < telMoment.Tijd).OrderBy(x => x.Tijd).Last()
                .Waarde;
            double nieuweWaarde;

            //als er nog geen telmomenten zijn
            if (!telMomenten.Any())
            {
                //als meer dan 30 procent aanwezig is, laten we de prijs stijgen
                if (aanwezigheid > InitieleMaatstaaf)
                {
                    nieuweWaarde = huidigeWaarde + (huidigeWaarde * aanwezigheid * 0.25d);
                }
                //anders laten we hem dalen
                else
                {
                    nieuweWaarde = huidigeWaarde - (huidigeWaarde * (InitieleMaatstaaf - aanwezigheid) * 0.25d);
                }
            }
            else
            {
                var vorigeTelMomenten = telMomenten;
                var laatste = vorigeTelMomenten.Last();

                //we pakken het gemiddelde van de laatste 3 telmomenten
                var laatsteDrieGemiddelde = vorigeTelMomenten.Average(x => x.Aantal);
                //We vergelijken de huidige aantal met het gemiddelde van de laatste 5 gemiddelden en de laatste.
                //op deze manier geven we het laatste aantal wel een hoge prioriteit, maar nemen we andere momenten
                //ook mee
                var vergelijkinsgAantal = Math.Round((laatsteDrieGemiddelde + laatste.Aantal) / 2f);
                var verschil = telMoment.Aantal - vergelijkinsgAantal;
                //We kijken proportioneel naar het verschil in leden
                var verschilProportie = (verschil / beurs.AantalLeden);
                //Als er alleen een klein verschil is, en de opkomst is hoog, geven we een bonus om
                // de beurzen in beweging te houden
                if (Math.Abs(verschilProportie) < 0.1f && aanwezigheid > 0.4f)
                {
                    verschilProportie += 0.2d;
                }
                nieuweWaarde = huidigeWaarde + (huidigeWaarde * verschilProportie * 0.2d);
            }
            return new BeursWaardes
            {
                Beurs = beurs,
                BeursId = beurs.BeursId,
                Tijd =telMoment.Tijd,
                Type = BeursWaardes.WaardeType.Aanwezigheid,
                Waarde = nieuweWaarde
            };
        }
    }
}