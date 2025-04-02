using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Common;

namespace Domain.Entities;

public class Lookup : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int TypeId { get; set; }
    public virtual LookupType Type { get; set; } = null!;
}
