using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sql_exercise_scoring.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coders",
                columns: table => new
                {
                    CoderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoderEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoderAvatar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionCoder = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PwdMd5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaltMd5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdminCoder = table.Column<bool>(type: "bit", nullable: false),
                    ContestSetter = table.Column<bool>(type: "bit", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PwdResetCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PwdResetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceiveEmail = table.Column<bool>(type: "bit", nullable: false),
                    LastCompilerId = table.Column<int>(type: "int", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coders", x => x.CoderId);
                });

            migrationBuilder.CreateTable(
                name: "sql_engine",
                columns: table => new
                {
                    engine_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    engine_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    engine_path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    engine_option = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sql_engine", x => x.engine_id);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    ThemeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThemeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.ThemeId);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    BlogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BlogContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BlogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    PinHome = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.BlogId);
                    table.ForeignKey(
                        name: "FK_Blogs_Coders_CoderId",
                        column: x => x.CoderId,
                        principalTable: "Coders",
                        principalColumn: "CoderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contests",
                columns: table => new
                {
                    ContestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoderId = table.Column<int>(type: "int", nullable: true),
                    ContestName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContestDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FailedPenalty = table.Column<int>(type: "int", nullable: false),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    StatusContest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    RankingFinished = table.Column<bool>(type: "bit", nullable: false),
                    FrozenTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contests", x => x.ContestId);
                    table.ForeignKey(
                        name: "FK_Contests_Coders_CoderId",
                        column: x => x.CoderId,
                        principalTable: "Coders",
                        principalColumn: "CoderId");
                });

            migrationBuilder.CreateTable(
                name: "Problems",
                columns: table => new
                {
                    ProblemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProblemCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProblemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProblemContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProblemExplanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TestCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TestProgCompilations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    CoderId = table.Column<int>(type: "int", nullable: true),
                    EngineId = table.Column<int>(type: "int", nullable: true),
                    ReviewCoderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problems", x => x.ProblemId);
                    table.ForeignKey(
                        name: "FK_Problems_Coders_CoderId",
                        column: x => x.CoderId,
                        principalTable: "Coders",
                        principalColumn: "CoderId");
                    table.ForeignKey(
                        name: "FK_Problems_sql_engine_EngineId",
                        column: x => x.EngineId,
                        principalTable: "sql_engine",
                        principalColumn: "engine_id");
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogId = table.Column<int>(type: "int", nullable: false),
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    CommentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CommentContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "BlogId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Coders_CoderId",
                        column: x => x.CoderId,
                        principalTable: "Coders",
                        principalColumn: "CoderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    AnnouncementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContestId = table.Column<int>(type: "int", nullable: true),
                    AnnounceTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnnounceContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.AnnouncementId);
                    table.ForeignKey(
                        name: "FK_Announcements_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "ContestId");
                });

            migrationBuilder.CreateTable(
                name: "Participations",
                columns: table => new
                {
                    ParticipationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoderId = table.Column<int>(type: "int", nullable: true),
                    ContestId = table.Column<int>(type: "int", nullable: true),
                    RegisterTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PointScore = table.Column<int>(type: "int", nullable: false),
                    TimeScore = table.Column<int>(type: "int", nullable: false),
                    Ranking = table.Column<int>(type: "int", nullable: false),
                    SolvedCount = table.Column<int>(type: "int", nullable: false),
                    RegisterMac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubRank = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participations", x => x.ParticipationId);
                    table.ForeignKey(
                        name: "FK_Participations_Coders_CoderId",
                        column: x => x.CoderId,
                        principalTable: "Coders",
                        principalColumn: "CoderId");
                    table.ForeignKey(
                        name: "FK_Participations_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "ContestId");
                });

            migrationBuilder.CreateTable(
                name: "DatabaseSchemas",
                columns: table => new
                {
                    DatabaseSchemaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProblemId = table.Column<int>(type: "int", nullable: true),
                    SchemaDefinition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InitialData = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseSchemas", x => x.DatabaseSchemaId);
                    table.ForeignKey(
                        name: "FK_DatabaseSchemas_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId");
                });

            migrationBuilder.CreateTable(
                name: "Favourites",
                columns: table => new
                {
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favourites", x => new { x.CoderId, x.ProblemId });
                    table.ForeignKey(
                        name: "FK_Favourites_Coders_CoderId",
                        column: x => x.CoderId,
                        principalTable: "Coders",
                        principalColumn: "CoderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favourites_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HasProblems",
                columns: table => new
                {
                    HasProblemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContestId = table.Column<int>(type: "int", nullable: true),
                    ProblemId = table.Column<int>(type: "int", nullable: true),
                    ProblemOrder = table.Column<int>(type: "int", nullable: false),
                    PointProblem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HasProblems", x => x.HasProblemId);
                    table.ForeignKey(
                        name: "FK_HasProblems_Contests_ContestId",
                        column: x => x.ContestId,
                        principalTable: "Contests",
                        principalColumn: "ContestId");
                    table.ForeignKey(
                        name: "FK_HasProblems_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId");
                });

            migrationBuilder.CreateTable(
                name: "ProblemThemes",
                columns: table => new
                {
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    ThemeId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemThemes", x => new { x.ProblemId, x.ThemeId });
                    table.ForeignKey(
                        name: "FK_ProblemThemes_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProblemThemes_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "ThemeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Solveds",
                columns: table => new
                {
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    ProblemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solveds", x => new { x.CoderId, x.ProblemId });
                    table.ForeignKey(
                        name: "FK_Solveds_Coders_CoderId",
                        column: x => x.CoderId,
                        principalTable: "Coders",
                        principalColumn: "CoderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Solveds_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestCases",
                columns: table => new
                {
                    TestCaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProblemId = table.Column<int>(type: "int", nullable: true),
                    SampleTest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreTest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedResult = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckerLogic = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCases", x => x.TestCaseId);
                    table.ForeignKey(
                        name: "FK_TestCases_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId");
                });

            migrationBuilder.CreateTable(
                name: "TakeParts",
                columns: table => new
                {
                    TakePartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipationId = table.Column<int>(type: "int", nullable: true),
                    ProblemId = table.Column<int>(type: "int", nullable: true),
                    TimeSolved = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PointWon = table.Column<int>(type: "int", nullable: false),
                    MaxPoint = table.Column<int>(type: "int", nullable: false),
                    SubmissionCount = table.Column<int>(type: "int", nullable: false),
                    SubmitMac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrozenTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TakeParts", x => x.TakePartId);
                    table.ForeignKey(
                        name: "FK_TakeParts_Participations_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "Participations",
                        principalColumn: "ParticipationId");
                    table.ForeignKey(
                        name: "FK_TakeParts_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId");
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EngineId = table.Column<int>(type: "int", nullable: true),
                    TakePartId = table.Column<int>(type: "int", nullable: true),
                    ProblemId = table.Column<int>(type: "int", nullable: true),
                    CoderId = table.Column<int>(type: "int", nullable: true),
                    SubmitTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmitCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmissionStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmitLineCount = table.Column<int>(type: "int", nullable: false),
                    TestRunCount = table.Column<int>(type: "int", nullable: false),
                    TestResult = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxMemorySize = table.Column<int>(type: "int", nullable: false),
                    MaxTimeDuration = table.Column<int>(type: "int", nullable: false),
                    SubmitMinute = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_Submissions_Coders_CoderId",
                        column: x => x.CoderId,
                        principalTable: "Coders",
                        principalColumn: "CoderId");
                    table.ForeignKey(
                        name: "FK_Submissions_Problems_ProblemId",
                        column: x => x.ProblemId,
                        principalTable: "Problems",
                        principalColumn: "ProblemId");
                    table.ForeignKey(
                        name: "FK_Submissions_TakeParts_TakePartId",
                        column: x => x.TakePartId,
                        principalTable: "TakeParts",
                        principalColumn: "TakePartId");
                    table.ForeignKey(
                        name: "FK_Submissions_sql_engine_EngineId",
                        column: x => x.EngineId,
                        principalTable: "sql_engine",
                        principalColumn: "engine_id");
                });

            migrationBuilder.CreateTable(
                name: "TestRuns",
                columns: table => new
                {
                    TestRunId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: true),
                    TestCaseId = table.Column<int>(type: "int", nullable: true),
                    TimeDuration = table.Column<int>(type: "int", nullable: false),
                    MemorySize = table.Column<int>(type: "int", nullable: false),
                    TestOutput = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckerLog = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRuns", x => x.TestRunId);
                    table.ForeignKey(
                        name: "FK_TestRuns_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "SubmissionId");
                    table.ForeignKey(
                        name: "FK_TestRuns_TestCases_TestCaseId",
                        column: x => x.TestCaseId,
                        principalTable: "TestCases",
                        principalColumn: "TestCaseId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_ContestId",
                table: "Announcements",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_CoderId",
                table: "Blogs",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_BlogId",
                table: "Comments",
                column: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CoderId",
                table: "Comments",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "IX_Contests_CoderId",
                table: "Contests",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "IX_DatabaseSchemas_ProblemId",
                table: "DatabaseSchemas",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Favourites_ProblemId",
                table: "Favourites",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_HasProblems_ContestId",
                table: "HasProblems",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_HasProblems_ProblemId",
                table: "HasProblems",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Participations_CoderId",
                table: "Participations",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "IX_Participations_ContestId",
                table: "Participations",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_Problems_CoderId",
                table: "Problems",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "IX_Problems_EngineId",
                table: "Problems",
                column: "EngineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemThemes_ThemeId",
                table: "ProblemThemes",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Solveds_ProblemId",
                table: "Solveds",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_CoderId",
                table: "Submissions",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_EngineId",
                table: "Submissions",
                column: "EngineId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ProblemId",
                table: "Submissions",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_TakePartId",
                table: "Submissions",
                column: "TakePartId");

            migrationBuilder.CreateIndex(
                name: "IX_TakeParts_ParticipationId",
                table: "TakeParts",
                column: "ParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_TakeParts_ProblemId",
                table: "TakeParts",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_ProblemId",
                table: "TestCases",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_SubmissionId",
                table: "TestRuns",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_TestCaseId",
                table: "TestRuns",
                column: "TestCaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "DatabaseSchemas");

            migrationBuilder.DropTable(
                name: "Favourites");

            migrationBuilder.DropTable(
                name: "HasProblems");

            migrationBuilder.DropTable(
                name: "ProblemThemes");

            migrationBuilder.DropTable(
                name: "Solveds");

            migrationBuilder.DropTable(
                name: "TestRuns");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "TestCases");

            migrationBuilder.DropTable(
                name: "TakeParts");

            migrationBuilder.DropTable(
                name: "Participations");

            migrationBuilder.DropTable(
                name: "Problems");

            migrationBuilder.DropTable(
                name: "Contests");

            migrationBuilder.DropTable(
                name: "sql_engine");

            migrationBuilder.DropTable(
                name: "Coders");
        }
    }
}
