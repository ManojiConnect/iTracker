using System;

namespace Application.Features.Lookups.Responses;
public class LookupResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Boolean IsActive { get; set; }
    public string Type { get; set; }
}
