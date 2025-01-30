using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class DatabaseSchema
{
    public int DatabaseSchemaId { get; set; }

    public string SchemaName { get; set; } = null!;

    public string SchemaDefinition { get; set; } = null!;

    public string InitialData { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
