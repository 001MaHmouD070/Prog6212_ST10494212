using Microsoft.EntityFrameworkCore;
using RacedayAPI.Models;

namespace RacedayAPI.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
        {
        }

        // DbSets - Each one maps to a table in the database
        public DbSet<User> Users { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }


        // OnModelCreating - Configure rules and constraints
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Users
            modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

            modelBuilder.Entity<User>()
            .ToTable(t => t.HasCheckConstraint(
            "CK_Users_Role",
            "[Role] IN ('Organiser', 'Participant')"));

            //Organisers
            modelBuilder.Entity<Organiser>()
            .HasIndex(o => o.UserID)
            .IsUnique();

            modelBuilder.Entity<Organiser>()
            .HasOne(o => o.User)
            .WithOne()
            .HasForeignKey<Organiser>(o => o.UserID)
            .OnDelete(DeleteBehavior.Cascade);

            //Participants
            modelBuilder.Entity<Participant>()
            .HasIndex(p => p.UserID)
            .IsUnique();

            modelBuilder.Entity<Participant>()
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<Participant>(p => p.UserID)
            .OnDelete(DeleteBehavior.Cascade);

            //Events
            modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganiserID)
            .OnDelete(DeleteBehavior.Cascade);

            //Categories
            modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventID)
            .OnDelete(DeleteBehavior.Cascade);

            //Enrolments
            modelBuilder.Entity<Enrolment>()
            .HasIndex(e => new { e.ParticipantID, e.EventID })
            .IsUnique();

            modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Participant)
            .WithMany(p => p.Enrolments)
            .HasForeignKey(e => e.ParticipantID)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Event)
            .WithMany(ev => ev.Enrolments)
            .HasForeignKey(e => e.EventID)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(e => e.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);

            //Results
            modelBuilder.Entity<Result>()
            .HasIndex(r => r.EnrolmentID)
            .IsUnique();

            modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrolmentID)
            .OnDelete(DeleteBehavior.Cascade);


            // DECIMAL PRECISION
            modelBuilder.Entity<Event>()
                .Property(e => e.Distance)
                .HasPrecision(6, 2);

            modelBuilder.Entity<Category>()
                .Property(c => c.EntryFee)
                .HasPrecision(10, 2);
        }

        }
    }