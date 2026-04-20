using SQLite;

namespace SharedActivityManager.Models
{
    [Table("Categories")]
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentCategoryId { get; set; }

        public int DisplayOrder { get; set; }

        public string Icon { get; set; }

        // Proprietate pentru UI
        [Ignore]
        public string DisplayName => Name;

        [Ignore]
        public bool HasChildren { get; set; }

        [Ignore]
        public int Level { get; set; }
    }
}