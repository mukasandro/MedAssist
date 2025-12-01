using MedAssist.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedAssist.Infrastructure.Data;

public class MedAssistDbContext : DbContext
{
    public MedAssistDbContext(DbContextOptions<MedAssistDbContext> options) : base(options)
    {
    }

    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Dialog> Dialogs => Set<Dialog>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Consent> Consents => Set<Consent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Doctor>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.DisplayName).IsRequired();
            b.Property(x => x.SpecializationCode).HasMaxLength(100);
            b.Property(x => x.SpecializationTitle).HasMaxLength(256);
            b.Property(x => x.TelegramUsername).IsRequired().HasMaxLength(64);
            b.Property(x => x.Languages).HasMaxLength(128);
            b.Property(x => x.FocusAreas).HasMaxLength(512);
            b.OwnsOne(x => x.Registration);
        });

        modelBuilder.Entity<Patient>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.FullName).IsRequired();
            b.Property(x => x.Tags).HasMaxLength(256);
        });

        modelBuilder.Entity<Dialog>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Topic).HasMaxLength(256);
        });

        modelBuilder.Entity<Message>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Content).IsRequired();
        });

        modelBuilder.Entity<Consent>(b =>
        {
            b.HasKey(x => x.Id);
        });
    }
}
