using JellyInspector.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JellyInspector.Infrastructure.Data;

public class JellyInspectorDbContext : DbContext
{
    public JellyInspectorDbContext(
        DbContextOptions<JellyInspectorDbContext> options)
        : base(options)
    {
    }

    public DbSet<SeriesEntity> Series =>
        Set<SeriesEntity>();

    public DbSet<SeasonEntity> Seasons =>
        Set<SeasonEntity>();

    public DbSet<EpisodeEntity> Episodes =>
        Set<EpisodeEntity>();

    public DbSet<ScanIssueEntity> ScanIssues =>
        Set<ScanIssueEntity>();

    public DbSet<ScanSessionEntity> ScanSessions =>
        Set<ScanSessionEntity>();

    public DbSet<AppSettings> AppSettings =>
        Set<AppSettings>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SeriesEntity>(entity =>
        {
            entity.ToTable("Series");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.JellyfinId)
                .IsRequired();

            entity.Property(x => x.Name)
                .IsRequired();

            entity.HasIndex(x => x.JellyfinId)
                .IsUnique();

            entity.HasMany(x => x.Seasons)
                .WithOne(x => x.Series)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.ScanIssues)
                .WithOne(x => x.Series)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SeasonEntity>(entity =>
        {
            entity.ToTable("Seasons");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.JellyfinId)
                .IsRequired();

            entity.Property(x => x.Name)
                .IsRequired();

            entity.HasIndex(x => x.JellyfinId)
                .IsUnique();

            entity.HasMany(x => x.Episodes)
                .WithOne(x => x.Season)
                .HasForeignKey(x => x.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EpisodeEntity>(entity =>
        {
            entity.ToTable("Episodes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.JellyfinId)
                .IsRequired();

            entity.Property(x => x.Name)
                .IsRequired();

            entity.Property(x => x.Resolution)
                .IsRequired();

            entity.Property(x => x.VideoCodec)
                .IsRequired();

            entity.Property(x => x.AudioCodec)
                .IsRequired();

            entity.HasIndex(x => x.JellyfinId)
                .IsUnique();
        });

        modelBuilder.Entity<ScanIssueEntity>(entity =>
        {
            entity.ToTable("ScanIssues");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .IsRequired();

            entity.Property(x => x.Message)
                .HasMaxLength(1000);

            entity.Property(x => x.CreatedUtc)
                .IsRequired();

            entity.HasOne(x => x.Series)
                .WithMany(x => x.ScanIssues)
                .HasForeignKey(x => x.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ScanSession)
                .WithMany(x => x.Issues)
                .HasForeignKey(x => x.ScanSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScanSessionEntity>(entity =>
        {
            entity.ToTable("ScanSessions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.StartedUtc)
                .IsRequired();

            entity.Property(x => x.FinishedUtc)
                .IsRequired();

            entity.Property(x => x.Duration)
                .IsRequired();

            entity.HasMany(x => x.Issues)
                .WithOne(x => x.ScanSession)
                .HasForeignKey(x => x.ScanSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.ToTable("AppSettings");

            entity.HasKey(x => x.Id);
        });
    }
}