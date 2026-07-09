namespace AgroConnect.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconClass { get; set; } // e.g. "bi-bug" for Arıçılıq
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
