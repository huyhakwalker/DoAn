namespace sql_exercise_scoring.Models
{
    public class DatabaseSchema
    {
        public int DatabaseSchemaId { get; set; }
        public int ProblemId { get; set; }
        public string SchemaDefinition { get; set; } = string.Empty;
        public string InitialData { get; set; } = string.Empty;

        public Problem Problem { get; set; }
    }
}
