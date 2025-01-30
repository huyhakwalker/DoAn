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

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<TakePart> TakeParts { get; set; }

    public virtual DbSet<TestCase> TestCases { get; set; }

    public virtual DbSet<TestRun> TestRuns { get; set; }

    public virtual DbSet<Theme> Themes { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-QRJ84D1\\SQLEXPRESS;Initial Catalog=sql_exercise_scoring;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("PK__Announce__9DE44574913CF726");

            entity.ToTable("Announcement");

            entity.Property(e => e.AnnounceTime).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Contest).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.ContestId)
                .HasConstraintName("FK_Announcement_Contest");
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__Blog__54379E30F987878E");

            entity.ToTable("Blog");

            entity.Property(e => e.BlogDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.BlogTitle).HasMaxLength(255);

            entity.HasOne(d => d.Coder).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Blog_Coder");
        });

        modelBuilder.Entity<Coder>(entity =>
        {
            entity.HasKey(e => e.CoderId).HasName("PK__Coder__C3ECFFBE3E74B862");

            entity.ToTable("Coder");

            entity.HasIndex(e => e.CoderEmail, "UQ__Coder__132DE69E507A9F15").IsUnique();

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
                        j.HasKey("CoderId", "ProblemId").HasName("PK__Solved__76222A96580CAFF0");
                        j.ToTable("Solved");
                    });
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFCA05C8F9DA");

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
            entity.HasKey(e => e.ContestId).HasName("PK__Contest__87DE0B1AADA4BC82");

            entity.ToTable("Contest");

            entity.Property(e => e.ContestName).HasMaxLength(255);
            entity.Property(e => e.RuleType).HasMaxLength(255);
            entity.Property(e => e.StatusContest).HasMaxLength(50);

            entity.HasOne(d => d.Coder).WithMany(p => p.Contests)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contest_Coder");
        });

        modelBuilder.Entity<DatabaseSchema>(entity =>
        {
            entity.HasKey(e => e.DatabaseSchemaId).HasName("PK__Database__DF438D0AF9742449");

            entity.ToTable("DatabaseSchema");

            entity.HasIndex(e => e.SchemaName, "UQ__Database__AAFC14FE1BAC5E1E").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.SchemaName).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Favourite>(entity =>
        {
            entity.HasKey(e => new { e.CoderId, e.ProblemId }).HasName("PK__Favourit__76222A965C0D1F41");

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
            entity.HasKey(e => e.HasProblemId).HasName("PK__HasProbl__CDB55A12FCBB9A57");

            entity.ToTable("HasProblem");

            entity.HasOne(d => d.Contest).WithMany(p => p.HasProblems)
                .HasForeignKey(d => d.ContestId)
                .HasConstraintName("FK_HasProblem_Contest");

            entity.HasOne(d => d.Problem).WithMany(p => p.HasProblems)
                .HasForeignKey(d => d.ProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HasProblem_Problem");
        });

        modelBuilder.Entity<Participation>(entity =>
        {
            entity.HasKey(e => e.ParticipationId).HasName("PK__Particip__4EA270E0858E2680");

            entity.ToTable("Participation");

            entity.Property(e => e.RegisterMac).HasMaxLength(255);
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
            entity.HasKey(e => e.ProblemId).HasName("PK__Problem__5CED528A1430AA49");

            entity.ToTable("Problem");

            entity.HasIndex(e => e.ProblemCode, "UQ__Problem__DB85FA6139A94322").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ProblemCode).HasMaxLength(255);
            entity.Property(e => e.ProblemName).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Coder).WithMany(p => p.Problems)
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Problem_Coder");

            entity.HasOne(d => d.DatabaseSchema).WithMany(p => p.Problems)
                .HasForeignKey(d => d.DatabaseSchemaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Problem_DatabaseSchema");
        });

        modelBuilder.Entity<ProblemTheme>(entity =>
        {
            entity.HasKey(e => new { e.ProblemId, e.ThemeId }).HasName("PK__ProblemT__D3566CC7EDE3C2DF");

            entity.ToTable("ProblemTheme");

            entity.HasOne(d => d.Problem).WithMany(p => p.ProblemThemes)
                .HasForeignKey(d => d.ProblemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProblemTheme_Problem");

            entity.HasOne(d => d.Theme).WithMany(p => p.ProblemThemes)
                .HasForeignKey(d => d.ThemeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProblemTheme_Theme");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId).HasName("PK__Submissi__449EE125A94E5CCE");

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
            entity.HasKey(e => e.TakePartId).HasName("PK__TakePart__1A8DC8FCECF4A125");

            entity.ToTable("TakePart");

            entity.Property(e => e.SubmitMac).HasMaxLength(255);

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
            entity.HasKey(e => e.TestCaseId).HasName("PK__TestCase__D2074A94C148A30A");

            entity.ToTable("TestCase");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Problem).WithMany(p => p.TestCases)
                .HasForeignKey(d => d.ProblemId)
                .HasConstraintName("FK_TestCase_Problem");
        });

        modelBuilder.Entity<TestRun>(entity =>
        {
            entity.HasKey(e => e.TestRunId).HasName("PK__TestRun__BF2F960E0BC6190E");

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
            entity.HasKey(e => e.ThemeId).HasName("PK__Theme__FBB3E4D9DE965D53");

            entity.ToTable("Theme");

            entity.HasIndex(e => e.ThemeName, "UQ__Theme__4E60E6D044DEEBA2").IsUnique();

            entity.Property(e => e.ThemeName).HasMaxLength(255);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.ChatMessageId);
            
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.SentAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Coder)
                .WithMany()
                .HasForeignKey(d => d.CoderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessages_Coder");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
