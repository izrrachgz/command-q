using Microsoft.EntityFrameworkCore;
using IdentityServer.Modules.Accounts;
using Time;
using IdentityServer.Persistence.Converters;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace IdentityServer.Persistence
{
    public abstract class IdentityServerDbContext(DbContextOptions options)
        : IdentityDbContext<IdentityUser, IdentityRole, Guid>(options)
    {
        public DbSet<IdentitySecurityEvent> SecurityEvents => Set<IdentitySecurityEvent>();
        internal DbSet<IdentityAccountChallenge> AccountChallenges => Set<IdentityAccountChallenge>();
        internal DbSet<IdentityAccountEmailDispatch> AccountEmailDispatches => Set<IdentityAccountEmailDispatch>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
            configurationBuilder.Properties<DateTimeOffset>().HaveConversion<UtcDateTimeOffsetConverter>();
            configurationBuilder.Properties<DateTime>().HavePrecision(UtcTime.PersistencePrecisionDigits);
            configurationBuilder.Properties<DateTimeOffset>().HavePrecision(UtcTime.PersistencePrecisionDigits);
        }

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
                entity.HasIndex(value => value.OccurredUtc);
            });

            builder.Entity<IdentityAccountChallenge>(entity =>
            {
                entity.ToTable("IdentityAccountChallenge");
                entity.HasKey(value => value.Id);
                entity.Property(value => value.Purpose).HasMaxLength(32).IsRequired();
                entity.Property(value => value.LinkTokenHash).HasMaxLength(64).IsFixedLength().IsRequired();
                entity.Property(value => value.CodeHash).HasMaxLength(64).IsFixedLength().IsRequired();
                entity.Property(value => value.ConcurrencyStamp).HasMaxLength(32).IsFixedLength().IsConcurrencyToken().IsRequired();
                entity.HasIndex(value => value.LinkTokenHash).IsUnique();
                entity.HasIndex(value => new { value.UserId, value.Purpose, value.ConsumedUtc });
                entity.HasIndex(value => new { value.UserId, value.Purpose, value.CreatedUtc });
                entity.HasIndex(value => value.ExpiresUtc);
                entity.HasOne(value => value.User)
                    .WithMany()
                    .HasForeignKey(value => value.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<IdentityAccountEmailDispatch>(entity =>
            {
                entity.ToTable("IdentityAccountEmailDispatch");
                entity.HasKey(value => value.Id);
                entity.Property(value => value.ConcurrencyStamp).HasMaxLength(32).IsFixedLength().IsConcurrencyToken().IsRequired();
                entity.HasIndex(value => value.ChallengeId).IsUnique();
                entity.HasIndex(value => new { value.QueuedUtc, value.AbandonedUtc, value.NextAttemptUtc });
                entity.HasOne(value => value.Challenge)
                    .WithOne()
                    .HasForeignKey<IdentityAccountEmailDispatch>(value => value.ChallengeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
