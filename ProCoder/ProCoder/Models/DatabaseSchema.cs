using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ProCoder.Models;

public partial class DatabaseSchema
{
    public int DatabaseSchemaId { get; set; }

    [Required(ErrorMessage = "Tên Schema không được để trống")]
    [StringLength(255, ErrorMessage = "Tên Schema không được vượt quá 255 ký tự")]
    public string SchemaName { get; set; } = null!;

    [Required(ErrorMessage = "Đường dẫn file Schema không được để trống")]
    public string SchemaDefinitionPath { get; set; } = null!;

    public string? Description { get; set; }

    public int CoderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Coder Coder { get; set; } = null!;

    public virtual ICollection<InitData> InitData { get; set; } = new List<InitData>();

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
