using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProCoder.Models;

public partial class SqlExerciseScoringContext : DbContext
{
    public SqlExerciseScoringContext()
    {
    }

    public SqlExerciseScoringContext(DbContextOptions<SqlExerciseScoringContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<Coder> Coders { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Contest> Contests { get; set; }

    public virtual DbSet<DatabaseSchema> DatabaseSchemas { get; set; }

    public virtual DbSet<Favourite> Favourites { get; set; }

    public virtual DbSet<HasProblem> HasProblems { get; set; }

    public virtual DbSet<InitData> InitData { get; set; }

    public virtual DbSet<Participation> Participations { get; set; }

    public virtual DbSet<Problem> Problems { get; set; }

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
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__Blog__54379E30331EC428");

            entity.ToTable("Blog");

            entity.Property(e => e.BlogDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.BlogTitle).HasMaxLength(255);

            entity.HasOne(d => d.Coder).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Blog_Coder");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.ChatMessageId).HasName("PK__ChatMess__9AB6103543E4F839");

            entity.Property(e => e.SentAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Coder).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessages_Coder");
        });

        modelBuilder.Entity<Coder>(entity =>
        {
            entity.HasKey(e => e.CoderId).HasName("PK__Coder__C3ECFFBED66353FD");

            entity.ToTable("Coder");

            entity.HasIndex(e => e.CoderEmail, "UQ__Coder__132DE69E8C3B22B4").IsUnique();

            entity.Property(e => e.CoderAvatar).HasMaxLength(255);
            entity.Property(e => e.CoderEmail).HasMaxLength(255);
            entity.Property(e => e.CoderName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(36)
                .IsUnicode(false);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.ReceiveEmail).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(36)
                .IsUnicode(false);

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
                        j.HasKey("CoderId", "ProblemId").HasName("PK__Solved__76222A96175875BA");
                        j.ToTable("Solved");
                    });
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFCA7E68D8A5");

            entity.Property(e => e.CommentDate).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Blog).WithMany(p => p.Comments)
                .HasForeignKey(d => d.BlogId)
                .HasConstraintName("FK_Comments_Blog");

            entity.HasOne(d => d.Coder).WithMany(p => p.Comments)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comments_Coder");
        });

        modelBuilder.Entity<Contest>(entity =>
        {
            entity.HasKey(e => e.ContestId).HasName("PK__Contest__87DE0B1A773A0677");

            entity.ToTable("Contest");

            entity.Property(e => e.ContestName).HasMaxLength(255);
            entity.Property(e => e.StatusContest).HasMaxLength(50);

            entity.HasOne(d => d.Coder).WithMany(p => p.Contests)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contest_Coder");
        });

        modelBuilder.Entity<DatabaseSchema>(entity =>
        {
            entity.HasKey(e => e.DatabaseSchemaId).HasName("PK__Database__DF438D0A54B8DE22");

            entity.ToTable("DatabaseSchema");

            entity.HasIndex(e => e.SchemaName, "UQ__Database__AAFC14FEBC4BA10C").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.SchemaName).HasMaxLength(255);
            entity.Property(e => e.SchemaDefinitionPath).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Coder).WithMany(p => p.DatabaseSchemas)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatabaseSchema_Coder");
        });

        modelBuilder.Entity<Favourite>(entity =>
        {
            entity.HasKey(e => new { e.CoderId, e.ProblemId }).HasName("PK__Favourit__76222A96A09293E0");

            entity.ToTable("Favourite");

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
            entity.HasKey(e => e.HasProblemId).HasName("PK__HasProbl__CDB55A120753B433");

            entity.ToTable("HasProblem");

            entity.HasOne(d => d.Contest).WithMany(p => p.HasProblems)
                .HasForeignKey(d => d.ContestId)
                .HasConstraintName("FK_HasProblem_Contest");

            entity.HasOne(d => d.Problem).WithMany(p => p.HasProblems)
                .HasForeignKey(d => d.ProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HasProblem_Problem");
        });

        modelBuilder.Entity<InitData>(entity =>
        {
            entity.HasKey(e => e.InitDataId).HasName("PK__InitData__9BF139016AB65994");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DataName).HasMaxLength(255);
            entity.Property(e => e.DataContentPath).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.DatabaseSchema).WithMany(p => p.InitData)
                .HasForeignKey(d => d.DatabaseSchemaId)
                .HasConstraintName("FK_InitData_DatabaseSchema");
        });

        modelBuilder.Entity<Participation>(entity =>
        {
            entity.HasKey(e => e.ParticipationId).HasName("PK__Particip__4EA270E005A5B723");

            entity.ToTable("Participation");

            entity.Property(e => e.RegisterTime).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Coder).WithMany(p => p.Participations)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Participation_Coder");

            entity.HasOne(d => d.Contest).WithMany(p => p.Participations)
                .HasForeignKey(d => d.ContestId)
                .HasConstraintName("FK_Participation_Contest");
        });

        modelBuilder.Entity<Problem>(entity =>
        {
            entity.HasKey(e => e.ProblemId).HasName("PK__Problem__5CED528AD5B48176");

            entity.ToTable("Problem");

            entity.HasIndex(e => e.ProblemCode, "UQ__Problem__DB85FA61069AC0A2").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ProblemCode).HasMaxLength(255);
            entity.Property(e => e.ProblemName).HasMaxLength(255);
            entity.Property(e => e.AnswerQueryPath).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Coder).WithMany(p => p.Problems)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Problem_Coder");

            entity.HasOne(d => d.DatabaseSchema).WithMany(p => p.Problems)
                .HasForeignKey(d => d.DatabaseSchemaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Problem_DatabaseSchema");

            entity.HasOne(d => d.Theme).WithMany(p => p.Problems)
                .HasForeignKey(d => d.ThemeId)
                .HasConstraintName("FK_Problem_Theme");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId).HasName("PK__Submissi__449EE125F4589F8F");

            entity.ToTable("Submission");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.SubmissionStatus).HasMaxLength(50);
            entity.Property(e => e.SubmitTime).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Coder).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Submission_Coder");

            entity.HasOne(d => d.Problem).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.ProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Submission_Problem");

            entity.HasOne(d => d.TakePart).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.TakePartId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Submission_TakePart");
        });

        modelBuilder.Entity<TakePart>(entity =>
        {
            entity.HasKey(e => e.TakePartId).HasName("PK__TakePart__1A8DC8FCB0C14D96");

            entity.ToTable("TakePart");

            entity.HasOne(d => d.Participation).WithMany(p => p.TakeParts)
                .HasForeignKey(d => d.ParticipationId)
                .HasConstraintName("FK_TakePart_Participation");

            entity.HasOne(d => d.Problem).WithMany(p => p.TakeParts)
                .HasForeignKey(d => d.ProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TakePart_Problem");
        });

        modelBuilder.Entity<TestCase>(entity =>
        {
            entity.HasKey(e => e.TestCaseId).HasName("PK__TestCase__D2074A94A61F3FCB");

            entity.ToTable("TestCase");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.AnswerResultPath).HasMaxLength(255);

            entity.HasOne(d => d.InitData).WithMany(p => p.TestCases)
                .HasForeignKey(d => d.InitDataId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_TestCase_InitData");

            entity.HasOne(d => d.Problem).WithMany(p => p.TestCases)
                .HasForeignKey(d => d.ProblemId)
                .HasConstraintName("FK_TestCase_Problem");
        });

        modelBuilder.Entity<TestRun>(entity =>
        {
            entity.HasKey(e => e.TestRunId).HasName("PK__TestRun__BF2F960E9EF9A162");

            entity.ToTable("TestRun");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Submission).WithMany(p => p.TestRuns)
                .HasForeignKey(d => d.SubmissionId)
                .HasConstraintName("FK_TestRun_Submission");

            entity.HasOne(d => d.TestCase).WithMany(p => p.TestRuns)
                .HasForeignKey(d => d.TestCaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestRun_TestCase");
        });

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.ThemeId).HasName("PK__Theme__FBB3E4D91CB7366C");

            entity.ToTable("Theme");

            entity.HasIndex(e => e.ThemeName, "UQ__Theme__4E60E6D0B1B3CEF9").IsUnique();

            entity.Property(e => e.ThemeName).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
