using System;
using System.Collections.Generic;

namespace sql_exercise_scoring.Models;

public partial class DatabaseSchema
{
    public int DatabaseSchemaId { get; set; }

    public int ProblemId { get; set; }

    public string SchemaDefinition { get; set; } = null!;

    public string InitialData { get; set; } = null!;

    public virtual Problem Problem { get; set; } = null!;
}
