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
    public DbSet<StaticContent> StaticContents => Set<StaticContent>();
    public DbSet<Consent> Consents => Set<Consent>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<BotConversation> BotConversations => Set<BotConversation>();
    public DbSet<BotChatTurn> BotChatTurns => Set<BotChatTurn>();
    public DbSet<BillingTokenLedger> BillingTokenLedgers => Set<BillingTokenLedger>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Doctor>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasMany(x => x.Patients)
                .WithOne(x => x.Doctor)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.LastSelectedPatient)
                .WithMany()
                .HasForeignKey(x => x.LastSelectedPatientId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.SpecializationCodes).HasColumnType("text[]");
            b.Property(x => x.SpecializationTitles).HasColumnType("text[]");
            b.Property(x => x.TelegramUserId);
            b.Property(x => x.TokenBalance).HasDefaultValue(0);
            b.HasIndex(x => x.TelegramUserId).IsUnique();
            b.OwnsOne(x => x.Registration, reg =>
            {
                reg.Property(r => r.SpecializationCodes).HasColumnType("text[]");
                reg.Property(r => r.SpecializationTitles).HasColumnType("text[]");
                reg.Property(r => r.Nickname).HasMaxLength(64);
            });
        });

        modelBuilder.Entity<Patient>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Nickname).HasMaxLength(64);
            b.Property(x => x.Tags).HasMaxLength(256);
        });

        modelBuilder.Entity<StaticContent>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Code).IsRequired().HasMaxLength(64);
            b.Property(x => x.Title).HasMaxLength(128);
            b.Property(x => x.Value).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Consent>(b =>
        {
            b.HasKey(x => x.Id);
        });

        modelBuilder.Entity<SystemSetting>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Key).IsRequired().HasMaxLength(128);
            b.Property(x => x.ValueJson).IsRequired().HasColumnType("text");
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.Key).IsUnique();
        });

        modelBuilder.Entity<BotConversation>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.TelegramUserId).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt).IsRequired();
            b.HasIndex(x => x.TelegramUserId);
            b.HasMany(x => x.Turns)
                .WithOne(x => x.Conversation)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BotChatTurn>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.RequestId).IsRequired();
            b.Property(x => x.UserText).IsRequired().HasColumnType("text");
            b.Property(x => x.AssistantText).IsRequired().HasColumnType("text");
            b.Property(x => x.Provider).HasMaxLength(64);
            b.Property(x => x.Model).HasMaxLength(128);
            b.Property(x => x.ProviderRequestId).HasMaxLength(128);
            b.Property(x => x.CreatedAt).IsRequired();
            b.HasIndex(x => x.ConversationId);
            b.HasIndex(x => x.RequestId).IsUnique();
        });

        modelBuilder.Entity<BillingTokenLedger>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.TelegramUserId).IsRequired();
            b.Property(x => x.Delta).IsRequired();
            b.Property(x => x.BalanceAfter).IsRequired();
            b.Property(x => x.Reason).IsRequired().HasMaxLength(64);
            b.Property(x => x.CreatedAt).IsRequired();
            b.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.DoctorId);
            b.HasIndex(x => x.TelegramUserId);
            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => x.RequestId).IsUnique();
        });
    }
}
