using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SportsEvents.Mvc.Entities;

namespace SportsEvents.Mvc.Data;

public partial class SportsEventsDbContext : DbContext
{
    public SportsEventsDbContext(DbContextOptions<SportsEventsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Event");

            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.StartAt).HasPrecision(0);
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.ToTable("Participant");

            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasDefaultValue("");
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.ToTable("Registration");

            entity.HasIndex(e => new { e.EventId, e.ParticipantId }, "UQ_Registration_Event_Participant").IsUnique();

            entity.Property(e => e.RegisteredAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Registration_RegisteredAt");

            entity.HasOne(d => d.Event).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Registration_Event");

            entity.HasOne(d => d.Participant).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.ParticipantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Registration_Participant");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
