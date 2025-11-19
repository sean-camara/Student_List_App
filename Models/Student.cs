using SQLite;
using Microsoft.Maui.Graphics;

namespace MAUIApp7.Models
{
    [Table("section")]
    public class Student
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(250)]
        public string Name { get; set; }

        // UI-only properties (not persisted)
        [Ignore]
        public Color CardColor { get; set; }

        [Ignore]
        public string Initial { get; set; }

        [Ignore]
        public string Subtitle { get; set; }
    }
}