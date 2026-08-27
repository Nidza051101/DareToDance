using Microsoft.EntityFrameworkCore;
using NotificationRecordEntity = NotificationService.Domain.NotificationRecord.NotificationRecord;

namespace NotificationService.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<NotificationRecordEntity> NotificationRecords => Set<NotificationRecordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationRecordEntity>(builder =>
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Recipient).IsRequired().HasMaxLength(320);
            builder.Property(n => n.Template).IsRequired().HasMaxLength(200);
            builder.Property(n => n.Channel).HasConversion<string>().HasMaxLength(20);
            builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20);

            // Bez ove linije, EF Core-ovo prepoznavanje konstruktora puca sa
            // "Cannot bind 'createdAtUtc'" — otkriveno pravim gRPC pozivom
            // (grpcurl) protiv running kontejnera. Svako polje koje konstruktor
            // prima mora biti EKSPLICITNO konfigurisano ovde, ne samo po
            // konvenciji, ili binding za njega ne uspe.
            builder.Property(n => n.CreatedAtUtc).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}

// NAPOMENA (2026-08-26): EF Core InMemory provajder je ovde ODLUKA, ne
// privremena zakrpa dok se nešto ne stigne rešiti — admin je odlučio da baza
// za testiranje ostane privremena (podaci ne prežive restart procesa), pa
// se pitanje MySQL provajdera trenutno ne postavlja.
//
// Uzgredno probano pri kreiranju ovog skeleta: Pomelo.EntityFrameworkCore.MySql
// još nema verziju kompatibilnu sa EF Core 10 (najnovija, 9.0.0, traži
// Microsoft.EntityFrameworkCore.Relational <= 9.0.999, sudara se sa 10.0.11
// koji koristi ostatak ove solucije) — ovo više nije razlog za InMemory,
// samo dodatna informacija ako se odluka ikad promeni. Zamena provajdera je
// tad jedna linija u DependencyInjection.cs (AddDbContext poziv) — Domain,
// Handler-i i Worker ne zavise od toga koji provajder je ispod.
