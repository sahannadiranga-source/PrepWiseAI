using Microsoft.EntityFrameworkCore;
using PrepWise.API.Models;
using System.Collections.Generic;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<InterviewSession> InterviewSessions { get; set; }
    public DbSet<Question> Questions { get; set; }
}