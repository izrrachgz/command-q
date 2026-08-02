using Microsoft.EntityFrameworkCore;
using IdentityServer.Modules.Accounts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IdentityServer.Persistence
{
    public abstract class IdentityServerDbContext(DbContextOptions options)
        : IdentityDbContext<IdentityUser, IdentityRole, Guid>(options)
    {
        public DbSet<IdentitySecurityEvent> SecurityEvents => Set<IdentitySecurityEvent>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentitySecurityEvent>(entity =>
            {
                entity.ToTable("IdentitySecurityEvent");
                entity.HasKey(value => value.Id);
                entity.Property(value => value.Action).HasMaxLength(128).IsRequired();
                entity.Property(value => value.ClientId).HasMaxLength(128);
                entity.Property(value => value.IpAddress).HasMaxLength(64);
                entity.Property(value => value.UserAgent).HasMaxLength(512);
                entity.Property(value => value.DetailsJson).HasMaxLength(4000);
                entity.HasIndex(value => new { value.UserId, value.OccurredUtc });
                entity.HasIndex(value => new { value.Action, value.OccurredUtc });
            });
        }
    }
}
