Beursspel

# installatie
## dotnet
Installeer dotnet sdk (2.1, maar misschien kan hoger ook)

## postgresql
- installeer postgresql
- in de config bestand van postgres 
    - `/etc/postgresql/[versie]/main/postgresql.conf`
    - en dan onder *Connections and authentication* moet je even `listen_addresses` uncommenten
    - `sudo -u postgres psql` en zet of een wachtwoord voor de postgres user, of maak een nieuwe user met wachtwoord
    - maak een database `Beursspel`
- Maak een `appsettings.json` bestand aan (er staat al een example waar je alleen je eigen meuk in hoeft te vullen)
- Doe vervolgens `dotnet ef migrations add Initial` oid, en daarna `dotnet ef database update`


## verder
- Tijdens development gebruik dan `ASPNETCORE_ENVIRONMENT=Development dotnet run` zodat je in development komt, tijdens het draaien zelf gebruik hier `Production` ipv `Development`
- Ook tijdens het draaien gebruik `dotnet publish -c Release` en doe dit op de server, doe vervolgens een reverse proxy maken en de rest en zo
- Of via SQL maak een account en maak jezelf admin, of je kan ook in `CheckIfOpen.cs` even de redirect weghalen en in `AdminController.cs` even de `[Authorize]` meuk wegcommenten en dan via de site zelf een account maken en jezelf admin maken
- voeg alle beurzen toe
- succes
- Misschien handig om een systemctl service te maken en die te enablen, scheelt weer moeite
- vergeet op `Gesloten.cshtml` niet de datum aan te passen van 'we hopen je te zien op x'