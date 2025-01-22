using System;
using System.Collections.Generic;

namespace sql_exercise_scoring.DTOs
{
    public class CoderDTO
    {
        public int CoderId { get; set; }
        public string CoderName { get; set; }
        public string CoderEmail { get; set; }
        public string PwdMd5Coder { get; set; }
        public bool AdminCoder { get; set; }
        public bool ContestSetter { get; set; }
        public DateTime RegisterDate { get; set; }
    }
}
