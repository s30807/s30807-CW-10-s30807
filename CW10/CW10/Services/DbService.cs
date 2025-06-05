using CW10.Controllers;
using CW10.Data;
using CW10.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CW10.Services;

public interface IDbService
{
    public Task<GetAllTripsDTO> GetAllTripsAsync(int page, int pageSize);
    public Task DeleteClientAsync(string idClient);
}

public class DbService(ApbdContext data) : IDbService
{
    public async Task<GetAllTripsDTO> GetAllTripsAsync(int page, int pageSize)
    {
        var totalTrips = await data.Trips.CountAsync();
        var totalPages = (int)Math.Ceiling(totalTrips / (double)pageSize);
        
        var trips = await data.Trips
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .Include(t => t.IdCountries)
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new GetTripDTO
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries.Select(c => new GetCountryDTO
                {
                    Name = c.Name
                }).ToList(),
                Clients = t.ClientTrips.Select(ct => new GetClientDTO
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                }).ToList()
            })
            .ToListAsync();

        return new GetAllTripsDTO
        {
            PageNum = page,
            PageSize = pageSize,
            AllPages = totalPages,
            Trips = trips
        };

    }
    
    public async Task DeleteClientAsync(string idClient)
    {
        var client = await data.Clients.FindAsync(idClient);
        if (client == null)
        {
            throw new NotFound($"Client with id {idClient} not found");
        }
        
        var trips = await data.ClientTrips.AnyAsync(ct => ct.IdClient ==  client.IdClient);
        if (trips) { throw new ClientWithTripsException($"Cannot delete client {idClient}, client has trips"); }
        
        //if no trips and client exists
        data.Clients.Remove(client);
        await data.SaveChangesAsync();
    }
}