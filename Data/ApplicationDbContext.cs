using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Beursspel.Models;
using Beursspel.Models.Beurzen;
using Beursspel.Models.TelMomentModels;

namespace Beursspel.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private static DbContextOptions<ApplicationDbContext> Options;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        :base(options)
        {
            Options = options;
        }

        public ApplicationDbContext()
        :base(Options)
        {

        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Beurs> Beurzen { get; set; }
        public DbSet<AandeelHouder> Aandelen { get; set; }
        public DbSet<BeursWaardes> BeursWaardes { get; set; }
        public DbSet<TelMomentHouder> TelMomenten { get; set; }
        public DbSet<TelMomentModel> TelMomentModel { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Beurs>().ToTable("Beurzen");
            builder.Entity<AandeelHouder>().ToTable("AandeelHouder");
        }
    }
}
