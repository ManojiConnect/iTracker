using System.Collections.Generic;

namespace Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<Investment> Investments { get; set; } = new List<Investment>();
} 