using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class DatabaseSchema
{
    public int DatabaseSchemaId { get; set; }

    public string SchemaName { get; set; } = null!;

    public string SchemaDefinitionPath { get; set; } = null!;

    public string? Description { get; set; }

    public int CoderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Coder Coder { get; set; } = null!;

    public virtual ICollection<InitData> InitData { get; set; } = new List<InitData>();

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
