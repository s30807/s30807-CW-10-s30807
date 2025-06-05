using CW10.Models;

namespace CW10.Controllers;

public class GetTripDTO
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public ICollection<GetCountryDTO> Countries { get; set; } = new List<GetCountryDTO>();
    public ICollection<GetClientDTO> Clients { get; set; } = new List<GetClientDTO>();
    
}

public class GetCountryDTO
{
    public string Name { get; set; }
}

public class GetClientDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}