using Microsoft.EntityFrameworkCore;
using SportsBuddyMatcher.Repository.Models;

namespace SportsBuddyMatcher.Repository;

public class SportsBuddyMatcherDbContext : DbContext
{
    public SportsBuddyMatcherDbContext(DbContextOptions<SportsBuddyMatcherDbContext> options)
        : base(options)
    {
    }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<PlayerSkill> PlayerSkills => Set<PlayerSkill>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<UserEvent> UserEvents => Set<UserEvent>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<UserConnection> Connections => Set<UserConnection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>().ToTable("Location");
        modelBuilder.Entity<User>().ToTable("User");
        modelBuilder.Entity<Skill>().ToTable("Skill");
        modelBuilder.Entity<PlayerSkill>().ToTable("PlayerSkill");
        modelBuilder.Entity<Event>().ToTable("Event");
        modelBuilder.Entity<UserEvent>().ToTable("UserEvent");
        modelBuilder.Entity<Availability>().ToTable("Availability");
        modelBuilder.Entity<User>().Property(u => u.PreferredRangeKm).HasPrecision(12, 2);
        modelBuilder.Entity<UserConnection>().ToTable("Connection", t => t.HasCheckConstraint("CK_Connection_Order", "[UserId] < [OtherUserId]"));
        modelBuilder.Entity<UserConnection>().HasIndex(c => new { c.UserId, c.OtherUserId }).IsUnique();
        modelBuilder.Entity<UserConnection>().HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<UserConnection>().HasOne(c => c.OtherUser).WithMany().HasForeignKey(c => c.OtherUserId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Event>().HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Skill>().HasIndex(s => s.Name).IsUnique();
        modelBuilder.Entity<PlayerSkill>().HasIndex(p => new { p.UserId, p.SkillId }).IsUnique();
        modelBuilder.Entity<UserEvent>().HasIndex(u => new { u.UserId, u.EventId }).IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.LocationNavigation).WithMany(l => l.Users).HasForeignKey(u => u.Location)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.LocationNavigation).WithMany(l => l.Events).HasForeignKey(e => e.Location)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Skill).WithMany(s => s.Events)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PlayerSkill>()
            .HasOne(p => p.User).WithMany(u => u.PlayerSkills)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PlayerSkill>()
            .HasOne(p => p.Skill).WithMany(s => s.PlayerSkills)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<UserEvent>()
            .HasOne(u => u.User).WithMany(u => u.UserEvents)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<UserEvent>()
            .HasOne(u => u.Event).WithMany(e => e.UserEvents)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
