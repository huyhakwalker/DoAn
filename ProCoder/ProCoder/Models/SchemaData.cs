using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProCoder.Models;

public partial class SchemaData
{
    public int SchemaDataId { get; set; }

    public int DatabaseSchemaId { get; set; }

    [StringLength(255)]
    public string DataName { get; set; } = null!;

    [Column(TypeName = "nvarchar(max)")]
    public string DataContent { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual DatabaseSchema DatabaseSchema { get; set; } = null!;

    public virtual ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
} 