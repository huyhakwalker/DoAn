using System.Reflection.Metadata;
using System;
using Microsoft.EntityFrameworkCore;
using sql_exercise_scoring.Models;

namespace sql_exercise_scoring.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSet for all tables
    public DbSet<Coder> Coder { get; set; }
    public DbSet<SqlEngine> SqlEngine { get; set; }
    public DbSet<Problem> Problem { get; set; }
    public DbSet<Blog> Blog { get; set; }
    public DbSet<Comment> Comment { get; set; }
    public DbSet<Favourite> Favourite { get; set; }
    public DbSet<Solved> Solved { get; set; }
    public DbSet<Submission> Submission { get; set; }
    public DbSet<Contest> Contest { get; set; }
    public DbSet<Participation> Participation { get; set; }
    public DbSet<TakePart> TakePart { get; set; }
    public DbSet<TestCase> TestCase { get; set; }
    public DbSet<TestRun> TestRun { get; set; }
    public DbSet<Announcement> Announcement { get; set; }
    public DbSet<HasProblem> HasProblem { get; set; }
    public DbSet<Theme> Theme { get; set; }
    public DbSet<ProblemTheme> ProblemTheme { get; set; }
    public DbSet<DatabaseSchema> DatabaseSchema { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite keys
        modelBuilder.Entity<Favourite>().HasKey(f => new { f.CoderId, f.ProblemId });
        modelBuilder.Entity<Solved>().HasKey(s => new { s.CoderId, s.ProblemId });
        modelBuilder.Entity<ProblemTheme>().HasKey(pt => new { pt.ProblemId, pt.ThemeId });

        // Foreign key relationships
        modelBuilder.Entity<Blog>()
            .HasOne(b => b.Coder)
            .WithMany(c => c.Blogs)
            .HasForeignKey(b => b.CoderId);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Blog)
            .WithMany(b => b.Comments)
            .HasForeignKey(c => c.BlogId);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Coder)
            .WithMany(c => c.Comments)
            .HasForeignKey(c => c.CoderId);

        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.Coder)
            .WithMany(c => c.Favourites)
            .HasForeignKey(f => f.CoderId);

        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.Problem)
            .WithMany(p => p.Favourites)
            .HasForeignKey(f => f.ProblemId);

        modelBuilder.Entity<Solved>()
            .HasOne(s => s.Coder)
            .WithMany(c => c.Solveds)
            .HasForeignKey(s => s.CoderId);

        modelBuilder.Entity<Solved>()
            .HasOne(s => s.Problem)
            .WithMany(p => p.Solveds)
            .HasForeignKey(s => s.ProblemId);

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Engine)
            .WithMany(e => e.Submissions)
            .HasForeignKey(s => s.EngineId);

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.TakePart)
            .WithMany(tp => tp.Submissions)
            .HasForeignKey(s => s.TakePartId);

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Coder)
            .WithMany(c => c.Submissions)
            .HasForeignKey(s => s.CoderId);

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Problem)
            .WithMany(p => p.Submissions)
            .HasForeignKey(s => s.ProblemId);

        modelBuilder.Entity<Contest>()
            .HasOne(c => c.Coder)
            .WithMany(c => c.Contests)
            .HasForeignKey(c => c.CoderId);

        modelBuilder.Entity<Participation>()
            .HasOne(p => p.Coder)
            .WithMany(c => c.Participations)
            .HasForeignKey(p => p.CoderId);

        modelBuilder.Entity<Participation>()
            .HasOne(p => p.Contest)
            .WithMany(c => c.Participations)
            .HasForeignKey(p => p.ContestId);

        modelBuilder.Entity<TakePart>()
            .HasOne(tp => tp.Participation)
            .WithMany(p => p.TakeParts)
            .HasForeignKey(tp => tp.ParticipationId);

        modelBuilder.Entity<TakePart>()
            .HasOne(tp => tp.Problem)
            .WithMany(p => p.TakeParts)
            .HasForeignKey(tp => tp.ProblemId);

        modelBuilder.Entity<Problem>()
            .HasOne(p => p.Coder)
            .WithMany(c => c.Problems)
            .HasForeignKey(p => p.CoderId);

        modelBuilder.Entity<Problem>()
            .HasOne(p => p.Engine)
            .WithMany(e => e.Problems)
            .HasForeignKey(p => p.EngineId);

        modelBuilder.Entity<TestCase>()
            .HasOne(tc => tc.Problem)
            .WithMany(p => p.TestCases)
            .HasForeignKey(tc => tc.ProblemId);

        modelBuilder.Entity<TestRun>()
            .HasOne(tr => tr.Submission)
            .WithMany(s => s.TestRuns)
            .HasForeignKey(tr => tr.SubmissionId);

        modelBuilder.Entity<TestRun>()
            .HasOne(tr => tr.TestCase)
            .WithMany(tc => tc.TestRuns)
            .HasForeignKey(tr => tr.TestCaseId);

        modelBuilder.Entity<Announcement>()
            .HasOne(a => a.Contest)
            .WithMany(c => c.Announcements)
            .HasForeignKey(a => a.ContestId);

        modelBuilder.Entity<HasProblem>()
            .HasOne(hp => hp.Contest)
            .WithMany(c => c.HasProblems)
            .HasForeignKey(hp => hp.ContestId);

        modelBuilder.Entity<HasProblem>()
            .HasOne(hp => hp.Problem)
            .WithMany(p => p.HasProblems)
            .HasForeignKey(hp => hp.ProblemId);

        modelBuilder.Entity<ProblemTheme>()
            .HasOne(pt => pt.Problem)
            .WithMany(p => p.ProblemThemes)
            .HasForeignKey(pt => pt.ProblemId);

        modelBuilder.Entity<ProblemTheme>()
            .HasOne(pt => pt.Theme)
            .WithMany(t => t.ProblemThemes)
            .HasForeignKey(pt => pt.ThemeId);

        modelBuilder.Entity<DatabaseSchema>()
            .HasOne(ds => ds.Problem)
            .WithMany(p => p.DatabaseSchemas)
            .HasForeignKey(ds => ds.ProblemId);
        modelBuilder.Entity<SqlEngine>()
            .ToTable("sql_engine")
            .HasKey(e => e.EngineId);
        modelBuilder.Entity<Coder>(entity =>
        {
            entity.Property(e => e.RegisterDate)
                  .HasDefaultValueSql("GETDATE()");
        });
        base.OnModelCreating(modelBuilder);
    }
}
