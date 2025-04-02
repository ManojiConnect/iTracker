using Domain.Entities.Common;
using System.Collections.Generic;

namespace Domain.Entities;

public class LookupType : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public virtual ICollection<Lookup> Lookups { get; set; } = new List<Lookup>();
}
