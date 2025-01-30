using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProCoder.Models;

public partial class Blog
{
    public int BlogId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
    [StringLength(255, MinimumLength = 5, ErrorMessage = "Tiêu đề phải từ 5-255 ký tự")]
    public string BlogTitle { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập nội dung")]
    [MinLength(10, ErrorMessage = "Nội dung phải có ít nhất 10 ký tự")]
    public string BlogContent { get; set; } = null!;

    public DateTime BlogDate { get; set; }

    public bool Published { get; set; }

    [Required]
    public int CoderId { get; set; }

    public bool PinHome { get; set; }

    public virtual Coder? Coder { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
