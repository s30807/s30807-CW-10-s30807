namespace CW10.Controllers;

public class GetAllTripsDTO
{
    public int PageNum { get; set; }
    public int PageSize { get; set; }
    public int AllPages { get; set; }
    public ICollection<GetTripDTO> Trips { get; set; } = new List<GetTripDTO>();
}