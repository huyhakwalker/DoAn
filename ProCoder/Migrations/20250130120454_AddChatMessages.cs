using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProCoder.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coder",
                columns: table => new
                {
                    CoderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoderName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CoderEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CoderAvatar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ReceiveEmail = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    Gender = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    CreatedBy = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: true),
                    AdminCoder = table.Column<bool>(type: "bit", nullable: false),
                    ContestSetter = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Coder__C3ECFFBE3E74B862", x => x.CoderId);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseSchema",
                columns: table => new
                {
                    DatabaseSchemaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchemaName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SchemaDefinition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InitialData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Database__DF438D0AF9742449", x => x.DatabaseSchemaId);
                });

            migrationBuilder.CreateTable(
                name: "Theme",
                columns: table => new
                {
                    ThemeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThemeName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ThemeOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Theme__FBB3E4D9DE965D53", x => x.ThemeId);
                });

            migrationBuilder.CreateTable(
                name: "Blog",
                columns: table => new
                {
                    BlogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BlogContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BlogDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    PinHome = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Blog__54379E30F987878E", x => x.BlogId);
                    table.ForeignKey(
                        name: "FK_Blog_Coder",
                        column: x => x.CoderId,
                        principalTable: "Coder",
                        principalColumn: "CoderId");
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    ChatMessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.ChatMessageId);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Coder_CoderId",
                        column: x => x.CoderId,
                        principalTable: "Coder",
                        principalColumn: "CoderId");
                });

            migrationBuilder.CreateTable(
                name: "Contest",
                columns: table => new
                {
                    ContestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    ContestName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContestDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FailedPenalty = table.Column<int>(type: "int", nullable: false),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    StatusContest = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    RankingFinished = table.Column<bool>(type: "bit", nullable: false),
                    FrozenTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Contest__87DE0B1AADA4BC82", x => x.ContestId);
                    table.ForeignKey(
                        name: "FK_Contest_Coder",
                        column: x => x.CoderId,
                        principalTable: "Coder",
                        principalColumn: "CoderId");
                });

            migrationBuilder.CreateTable(
                name: "Problem",
                columns: table => new
                {
                    ProblemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProblemCode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProblemName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProblemDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProblemExplanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Published = table.Column<bool>(type: "bit", nullable: false),
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    DatabaseSchemaId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Problem__5CED528A1430AA49", x => x.ProblemId);
                    table.ForeignKey(
                        name: "FK_Problem_Coder",
                        column: x => x.CoderId,
                        principalTable: "Coder",
                        principalColumn: "CoderId");
                    table.ForeignKey(
                        name: "FK_Problem_DatabaseSchema",
                        column: x => x.DatabaseSchemaId,
                        principalTable: "DatabaseSchema",
                        principalColumn: "DatabaseSchemaId");
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogId = table.Column<int>(type: "int", nullable: false),
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    CommentDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    CommentContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Comments__C3B4DFCA05C8F9DA", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_Blog",
                        column: x => x.BlogId,
                        principalTable: "Blog",
                        principalColumn: "BlogId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Coder",
                        column: x => x.CoderId,
                        principalTable: "Coder",
                        principalColumn: "CoderId");
                });

            migrationBuilder.CreateTable(
                name: "Announcement",
                columns: table => new
                {
                    AnnouncementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContestId = table.Column<int>(type: "int", nullable: false),
                    AnnounceTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    AnnounceContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Announce__9DE44574913CF726", x => x.AnnouncementId);
                    table.ForeignKey(
                        name: "FK_Announcement_Contest",
                        column: x => x.ContestId,
                        principalTable: "Contest",
                        principalColumn: "ContestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Participation",
                columns: table => new
                {
                    ParticipationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    ContestId = table.Column<int>(type: "int", nullable: false),
                    RegisterTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    PointScore = table.Column<int>(type: "int", nullable: false),
                    TimeScore = table.Column<int>(type: "int", nullable: false),
                    Ranking = table.Column<int>(type: "int", nullable: false),
                    SolvedCount = table.Column<int>(type: "int", nullable: false),
                    RegisterMac = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Particip__4EA270E0858E2680", x => x.ParticipationId);
                    table.ForeignKey(
                        name: "FK_Participation_Coder",
                        column: x => x.CoderId,
                        principalTable: "Coder",
                        principalColumn: "CoderId");
                    table.ForeignKey(
                        name: "FK_Participation_Contest",
                        column: x => x.ContestId,
                        principalTable: "Contest",
                        principalColumn: "ContestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Favourite",
                columns: table => new
                {
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Favourit__76222A965C0D1F41", x => new { x.CoderId, x.ProblemId });
                    table.ForeignKey(
                        name: "FK_Favourite_Coder",
                        column: x => x.CoderId,
                        principalTable: "Coder",
                        principalColumn: "CoderId");
                    table.ForeignKey(
                        name: "FK_Favourite_Problem",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "ProblemId");
                });

            migrationBuilder.CreateTable(
                name: "HasProblem",
                columns: table => new
                {
                    HasProblemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContestId = table.Column<int>(type: "int", nullable: false),
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    ProblemOrder = table.Column<int>(type: "int", nullable: false),
                    PointProblem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HasProbl__CDB55A12FCBB9A57", x => x.HasProblemId);
                    table.ForeignKey(
                        name: "FK_HasProblem_Contest",
                        column: x => x.ContestId,
                        principalTable: "Contest",
                        principalColumn: "ContestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HasProblem_Problem",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "ProblemId");
                });

            migrationBuilder.CreateTable(
                name: "ProblemTheme",
                columns: table => new
                {
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    ThemeId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ProblemT__D3566CC7EDE3C2DF", x => new { x.ProblemId, x.ThemeId });
                    table.ForeignKey(
                        name: "FK_ProblemTheme_Problem",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "ProblemId");
                    table.ForeignKey(
                        name: "FK_ProblemTheme_Theme",
                        column: x => x.ThemeId,
                        principalTable: "Theme",
                        principalColumn: "ThemeId");
                });

            migrationBuilder.CreateTable(
                name: "Solved",
                columns: table => new
                {
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    ProblemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Solved__76222A96580CAFF0", x => new { x.CoderId, x.ProblemId });
                    table.ForeignKey(
                        name: "FK_Solved_Coder",
                        column: x => x.CoderId,
                        principalTable: "Coder",
                        principalColumn: "CoderId");
                    table.ForeignKey(
                        name: "FK_Solved_Problem",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "ProblemId");
                });

            migrationBuilder.CreateTable(
                name: "TestCase",
                columns: table => new
                {
                    TestCaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    TestQuery = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedResult = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false),
                    OrderNumber = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TestCase__D2074A94C148A30A", x => x.TestCaseId);
                    table.ForeignKey(
                        name: "FK_TestCase_Problem",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "ProblemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TakePart",
                columns: table => new
                {
                    TakePartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipationId = table.Column<int>(type: "int", nullable: false),
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    TimeSolved = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PointWon = table.Column<int>(type: "int", nullable: false),
                    MaxPoint = table.Column<int>(type: "int", nullable: false),
                    SubmissionCount = table.Column<int>(type: "int", nullable: false),
                    SubmitMac = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TakePart__1A8DC8FCECF4A125", x => x.TakePartId);
                    table.ForeignKey(
                        name: "FK_TakePart_Participation",
                        column: x => x.ParticipationId,
                        principalTable: "Participation",
                        principalColumn: "ParticipationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TakePart_Problem",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "ProblemId");
                });

            migrationBuilder.CreateTable(
                name: "Submission",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProblemId = table.Column<int>(type: "int", nullable: false),
                    CoderId = table.Column<int>(type: "int", nullable: false),
                    TakePartId = table.Column<int>(type: "int", nullable: true),
                    SubmitTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    SubmitCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmissionStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    ExecutionTime = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Submissi__449EE125A94E5CCE", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_Submission_Coder",
                        column: x => x.CoderId,
                        principalTable: "Coder",
                        principalColumn: "CoderId");
                    table.ForeignKey(
                        name: "FK_Submission_Problem",
                        column: x => x.ProblemId,
                        principalTable: "Problem",
                        principalColumn: "ProblemId");
                    table.ForeignKey(
                        name: "FK_Submission_TakePart",
                        column: x => x.TakePartId,
                        principalTable: "TakePart",
                        principalColumn: "TakePartId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TestRun",
                columns: table => new
                {
                    TestRunId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    TestCaseId = table.Column<int>(type: "int", nullable: false),
                    ActualOutput = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    ExecutionTime = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TestRun__BF2F960E0BC6190E", x => x.TestRunId);
                    table.ForeignKey(
                        name: "FK_TestRun_Submission",
                        column: x => x.SubmissionId,
                        principalTable: "Submission",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestRun_TestCase",
                        column: x => x.TestCaseId,
                        principalTable: "TestCase",
                        principalColumn: "TestCaseId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcement_ContestId",
                table: "Announcement",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_Blog_CoderId",
                table: "Blog",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_CoderId",
                table: "ChatMessages",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "UQ__Coder__132DE69E507A9F15",
                table: "Coder",
                column: "CoderEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_BlogId",
                table: "Comments",
                column: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CoderId",
                table: "Comments",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "IX_Contest_CoderId",
                table: "Contest",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "UQ__Database__AAFC14FE1BAC5E1E",
                table: "DatabaseSchema",
                column: "SchemaName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favourite_ProblemId",
                table: "Favourite",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_HasProblem_ContestId",
                table: "HasProblem",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_HasProblem_ProblemId",
                table: "HasProblem",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Participation_CoderId",
                table: "Participation",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "IX_Participation_ContestId",
                table: "Participation",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_Problem_CoderId",
                table: "Problem",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "IX_Problem_DatabaseSchemaId",
                table: "Problem",
                column: "DatabaseSchemaId");

            migrationBuilder.CreateIndex(
                name: "UQ__Problem__DB85FA6139A94322",
                table: "Problem",
                column: "ProblemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProblemTheme_ThemeId",
                table: "ProblemTheme",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Solved_ProblemId",
                table: "Solved",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_CoderId",
                table: "Submission",
                column: "CoderId");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_ProblemId",
                table: "Submission",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_Submission_TakePartId",
                table: "Submission",
                column: "TakePartId");

            migrationBuilder.CreateIndex(
                name: "IX_TakePart_ParticipationId",
                table: "TakePart",
                column: "ParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_TakePart_ProblemId",
                table: "TakePart",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCase_ProblemId",
                table: "TestCase",
                column: "ProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_TestRun_SubmissionId",
                table: "TestRun",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestRun_TestCaseId",
                table: "TestRun",
                column: "TestCaseId");

            migrationBuilder.CreateIndex(
                name: "UQ__Theme__4E60E6D044DEEBA2",
                table: "Theme",
                column: "ThemeName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcement");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Favourite");

            migrationBuilder.DropTable(
                name: "HasProblem");

            migrationBuilder.DropTable(
                name: "ProblemTheme");

            migrationBuilder.DropTable(
                name: "Solved");

            migrationBuilder.DropTable(
                name: "TestRun");

            migrationBuilder.DropTable(
                name: "Blog");

            migrationBuilder.DropTable(
                name: "Theme");

            migrationBuilder.DropTable(
                name: "Submission");

            migrationBuilder.DropTable(
                name: "TestCase");

            migrationBuilder.DropTable(
                name: "TakePart");

            migrationBuilder.DropTable(
                name: "Participation");

            migrationBuilder.DropTable(
                name: "Problem");

            migrationBuilder.DropTable(
                name: "Contest");

            migrationBuilder.DropTable(
                name: "DatabaseSchema");

            migrationBuilder.DropTable(
                name: "Coder");
        }
    }
}
