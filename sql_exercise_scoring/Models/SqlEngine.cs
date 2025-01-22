using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace sql_exercise_scoring.Models
{
    public class SqlEngine
    {
        [Key] // Đảm bảo khóa chính được định nghĩa
        [Column("engine_id")] // Ánh xạ đến cột trong cơ sở dữ liệu
        public int EngineId { get; set; }

        [Column("engine_name")]
        public string EngineName { get; set; }

        [Column("version")]
        public string Version { get; set; }

        [Column("engine_path")]
        public string EnginePath { get; set; }

        [Column("engine_option")]
        public string? EngineOption { get; set; }

        public ICollection<Problem> Problems { get; set; }
        public ICollection<Submission> Submissions { get; set; }
    }
}
