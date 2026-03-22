using Microsoft.EntityFrameworkCore;
using PrepWise.API.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<InterviewSession> InterviewSessions { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<UserSkill> UserSkills { get; set; }
    public DbSet<SessionAnalytics> SessionAnalytics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InterviewSession>()
            .HasOne(s => s.Analytics)
            .WithOne()
            .HasForeignKey<SessionAnalytics>(a => a.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InterviewSession>()
            .HasMany(s => s.Questions)
            .WithOne()
            .HasForeignKey(q => q.InterviewSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}