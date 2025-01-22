using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sql_exercise_scoring.Models
{
    public class Coder
    {
        public int CoderId { get; set; } // Primary Key
        public string CoderName { get; set; } // Not NULL
        public string CoderEmail { get; set; } // Not NULL
        public string? CoderAvatar { get; set; } // Nullable
        public string? DescriptionCoder { get; set; } // Nullable
        public string PwdMd5 { get; set; } // Not NULL
        public string? SaltMd5 { get; set; } // Not NULL
        public bool AdminCoder { get; set; } // Not NULL
        public bool ContestSetter { get; set; } // Not NULL
        public DateTime RegisterDate { get; set; } // Not NULL
        public string? PwdResetCode { get; set; } // Nullable
        public DateTime? PwdResetDate { get; set; } // Nullable
        public bool? ReceiveEmail { get; set; } // Not NULL
        public int? LastCompilerId { get; set; } // Nullable
        public string? Gender { get; set; } // Nullable
        // Quan hệ
        public ICollection<Blog> Blogs { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Problem> Problems { get; set; }
        public ICollection<Participation> Participations { get; set; }
        public ICollection<Favourite> Favourites { get; set; }
        public ICollection<Solved> Solveds { get; set; }
        public ICollection<Submission> Submissions { get; set; }
        public ICollection<Contest> Contests { get; set; }
    }
}
