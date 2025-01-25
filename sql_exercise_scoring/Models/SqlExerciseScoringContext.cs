using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace sql_exercise_scoring.Models;

public partial class SqlExerciseScoringContext : DbContext
{
    public SqlExerciseScoringContext()
    {
    }

    public SqlExerciseScoringContext(DbContextOptions<SqlExerciseScoringContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<Coder> Coders { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Contest> Contests { get; set; }

    public virtual DbSet<DatabaseSchema> DatabaseSchemas { get; set; }

    public virtual DbSet<Favourite> Favourites { get; set; }

    public virtual DbSet<HasProblem> HasProblems { get; set; }

    public virtual DbSet<Participation> Participations { get; set; }

    public virtual DbSet<Problem> Problems { get; set; }

    public virtual DbSet<ProblemTheme> ProblemThemes { get; set; }

    public virtual DbSet<SqlEngine> SqlEngines { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<TakePart> TakeParts { get; set; }

    public virtual DbSet<TestCase> TestCases { get; set; }

    public virtual DbSet<TestRun> TestRuns { get; set; }

    public virtual DbSet<Theme> Themes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-QRJ84D1\\SQLEXPRESS;Initial Catalog=sql_exercise_scoring;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("PK__Announce__9DE44574CBA8F75E");

            entity.ToTable("Announcement");

            entity.Property(e => e.AnnounceContent).HasColumnType("ntext");
            entity.Property(e => e.AnnounceTime).HasColumnType("datetime");

            entity.HasOne(d => d.Contest).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.ContestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Announcement_Contest");
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__Blog__54379E3075A74CD7");

            entity.ToTable("Blog");

            entity.Property(e => e.BlogContent).HasColumnType("ntext");
            entity.Property(e => e.BlogDate).HasColumnType("datetime");
            entity.Property(e => e.BlogTitle).HasMaxLength(255);

            entity.HasOne(d => d.Coder).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Blog_Coder");
        });

        modelBuilder.Entity<Coder>(entity =>
        {
            entity.HasKey(e => e.CoderId).HasName("PK__Coder__C3ECFFBEF7443F95");

            entity.ToTable("Coder");

            entity.Property(e => e.CoderAvatar).HasMaxLength(255);
            entity.Property(e => e.CoderEmail).HasMaxLength(255);
            entity.Property(e => e.CoderName).HasMaxLength(255);
            entity.Property(e => e.DescriptionCoder).HasColumnType("ntext");
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.PwdMd5).HasMaxLength(255);
            entity.Property(e => e.PwdResetCode).HasMaxLength(255);
            entity.Property(e => e.PwdResetDate).HasColumnType("datetime");
            entity.Property(e => e.RegisterDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SaltMd5).HasMaxLength(255);

            entity.HasMany(d => d.ProblemsNavigation).WithMany(p => p.Coders)
                .UsingEntity<Dictionary<string, object>>(
                    "Solved",
                    r => r.HasOne<Problem>().WithMany()
                        .HasForeignKey("ProblemId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Solved_Problem"),
                    l => l.HasOne<Coder>().WithMany()
                        .HasForeignKey("CoderId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Solved_Coder"),
                    j =>
                    {
                        j.HasKey("CoderId", "ProblemId").HasName("PK__Solved__76222A96BAF8E135");
                        j.ToTable("Solved");
                    });
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFCA7C0DE955");

            entity.Property(e => e.CommentContent).HasColumnType("ntext");
            entity.Property(e => e.CommentDate).HasColumnType("datetime");

            entity.HasOne(d => d.Blog).WithMany(p => p.Comments)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comments_Blog");

            entity.HasOne(d => d.Coder).WithMany(p => p.Comments)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comments_Coder");
        });

        modelBuilder.Entity<Contest>(entity =>
        {
            entity.HasKey(e => e.ContestId).HasName("PK__Contest__87DE0B1A5DF6EB56");

            entity.ToTable("Contest");

            entity.Property(e => e.ContestDescription).HasColumnType("ntext");
            entity.Property(e => e.ContestName).HasMaxLength(255);
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.FrozenTime).HasColumnType("datetime");
            entity.Property(e => e.RuleType).HasMaxLength(255);
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.StatusContest).HasMaxLength(50);

            entity.HasOne(d => d.Coder).WithMany(p => p.Contests)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contest_Coder");
        });

        modelBuilder.Entity<DatabaseSchema>(entity =>
        {
            entity.HasKey(e => e.DatabaseSchemaId).HasName("PK__Database__DF438D0ACA70081D");

            entity.ToTable("DatabaseSchema");

            entity.Property(e => e.InitialData).HasColumnType("ntext");
            entity.Property(e => e.SchemaDefinition).HasColumnType("ntext");

            entity.HasOne(d => d.Problem).WithMany(p => p.DatabaseSchemas)
                .HasForeignKey(d => d.ProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatabaseSchema_Problem");
        });

        modelBuilder.Entity<Favourite>(entity =>
        {
            entity.HasKey(e => new { e.CoderId, e.ProblemId }).HasName("PK__Favourit__76222A96928BDE02");

            entity.ToTable("Favourite");

            entity.Property(e => e.Note).HasColumnType("ntext");

            entity.HasOne(d => d.Coder).WithMany(p => p.Favourites)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favourite_Coder");

            entity.HasOne(d => d.Problem).WithMany(p => p.Favourites)
                .HasForeignKey(d => d.ProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favourite_Problem");
        });

        modelBuilder.Entity<HasProblem>(entity =>
        {
            entity.HasKey(e => e.HasProblemId).HasName("PK__HasProbl__CDB55A120201854B");

            entity.ToTable("HasProblem");

            entity.HasOne(d => d.Contest).WithMany(p => p.HasProblems)
                .HasForeignKey(d => d.ContestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HasProblem_Contest");

            entity.HasOne(d => d.Problem).WithMany(p => p.HasProblems)
                .HasForeignKey(d => d.ProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HasProblem_Problem");
        });

        modelBuilder.Entity<Participation>(entity =>
        {
            entity.HasKey(e => e.ParticipationId).HasName("PK__Particip__4EA270E0F3823737");

            entity.ToTable("Participation");

            entity.Property(e => e.RegisterMac).HasMaxLength(255);
            entity.Property(e => e.RegisterTime).HasColumnType("datetime");

            entity.HasOne(d => d.Coder).WithMany(p => p.Participations)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Participation_Coder");

            entity.HasOne(d => d.Contest).WithMany(p => p.Participations)
                .HasForeignKey(d => d.ContestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Participation_Contest");
        });

        modelBuilder.Entity<Problem>(entity =>
        {
            entity.HasKey(e => e.ProblemId).HasName("PK__Problem__5CED528AF238ACE1");

            entity.ToTable("Problem");

            entity.Property(e => e.ProblemCode).HasMaxLength(255);
            entity.Property(e => e.ProblemContent).HasColumnType("ntext");
            entity.Property(e => e.ProblemExplanation).HasColumnType("ntext");
            entity.Property(e => e.ProblemName).HasMaxLength(255);
            entity.Property(e => e.TestCode).HasColumnType("ntext");
            entity.Property(e => e.TestProgCompilations).HasColumnType("ntext");
            entity.Property(e => e.TestType).HasMaxLength(50);

            entity.HasOne(d => d.Coder).WithMany(p => p.Problems)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Problem_Coder");

            entity.HasOne(d => d.Engine).WithMany(p => p.Problems)
                .HasForeignKey(d => d.EngineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Problem_Engine");
        });

        modelBuilder.Entity<ProblemTheme>(entity =>
        {
            entity.HasKey(e => new { e.ProblemId, e.ThemeId }).HasName("PK__ProblemT__D3566CC7603FA2CD");

            entity.ToTable("ProblemTheme");

            entity.Property(e => e.Note).HasColumnType("ntext");

            entity.HasOne(d => d.Problem).WithMany(p => p.ProblemThemes)
                .HasForeignKey(d => d.ProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProblemTheme_Problem");

            entity.HasOne(d => d.Theme).WithMany(p => p.ProblemThemes)
                .HasForeignKey(d => d.ThemeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProblemTheme_Theme");
        });

        modelBuilder.Entity<SqlEngine>(entity =>
        {
            entity.HasKey(e => e.EngineId).HasName("PK__SqlEngin__7BBCE9046B2724F8");

            entity.ToTable("SqlEngine");

            entity.Property(e => e.EngineName).HasMaxLength(50);
            entity.Property(e => e.EngineOption).HasColumnType("ntext");
            entity.Property(e => e.EnginePath).HasMaxLength(255);
            entity.Property(e => e.Version).HasMaxLength(20);
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId).HasName("PK__Submissi__449EE1251614EE24");

            entity.ToTable("Submission");

            entity.Property(e => e.SubmissionStatus).HasMaxLength(50);
            entity.Property(e => e.SubmitCode).HasColumnType("ntext");
            entity.Property(e => e.SubmitTime).HasColumnType("datetime");
            entity.Property(e => e.TestResult).HasMaxLength(255);

            entity.HasOne(d => d.Coder).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Submission_Coder");

            entity.HasOne(d => d.Engine).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.EngineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Submission_Engine");

            entity.HasOne(d => d.Problem).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.ProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Submission_Problem");

            entity.HasOne(d => d.TakePart).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.TakePartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Submission_TakePart");
        });

        modelBuilder.Entity<TakePart>(entity =>
        {
            entity.HasKey(e => e.TakePartId).HasName("PK__TakePart__1A8DC8FC5C66333B");

            entity.ToTable("TakePart");

            entity.Property(e => e.FrozenTime).HasColumnType("datetime");
            entity.Property(e => e.SubmitMac).HasMaxLength(255);
            entity.Property(e => e.TimeSolved).HasColumnType("datetime");

            entity.HasOne(d => d.Participation).WithMany(p => p.TakeParts)
                .HasForeignKey(d => d.ParticipationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TakePart_Participation");

            entity.HasOne(d => d.Problem).WithMany(p => p.TakeParts)
                .HasForeignKey(d => d.ProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TakePart_Problem");
        });

        modelBuilder.Entity<TestCase>(entity =>
        {
            entity.HasKey(e => e.TestCaseId).HasName("PK__TestCase__D2074A94E200D683");

            entity.ToTable("TestCase");

            entity.Property(e => e.CheckerLogic).HasColumnType("ntext");
            entity.Property(e => e.ExpectedResult).HasColumnType("ntext");
            entity.Property(e => e.PreTest).HasColumnType("ntext");
            entity.Property(e => e.SampleTest).HasColumnType("ntext");

            entity.HasOne(d => d.Problem).WithMany(p => p.TestCases)
                .HasForeignKey(d => d.ProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestCase_Problem");
        });

        modelBuilder.Entity<TestRun>(entity =>
        {
            entity.HasKey(e => e.TestRunId).HasName("PK__TestRun__BF2F960E347A060C");

            entity.ToTable("TestRun");

            entity.Property(e => e.CheckerLog).HasColumnType("ntext");
            entity.Property(e => e.Result).HasMaxLength(50);
            entity.Property(e => e.TestOutput).HasColumnType("ntext");

            entity.HasOne(d => d.Submission).WithMany(p => p.TestRuns)
                .HasForeignKey(d => d.SubmissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestRun_Submission");

            entity.HasOne(d => d.TestCase).WithMany(p => p.TestRuns)
                .HasForeignKey(d => d.TestCaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestRun_TestCase");
        });

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.ThemeId).HasName("PK__Theme__FBB3E4D950C170E4");

            entity.ToTable("Theme");

            entity.Property(e => e.ThemeName).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
