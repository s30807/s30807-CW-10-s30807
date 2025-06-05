using CW10.Controllers;
using CW10.Data;
using CW10.Exceptions;
using CW10.Models;
using Microsoft.EntityFrameworkCore;

namespace CW10.Services;

public interface IDbService
{
    public Task<GetAllTripsDTO> GetAllTripsAsync(int page, int pageSize);
    public Task DeleteClientAsync(string idClient);
    public Task<PostClientToTripDTO> AddClientToTripAsync(int idTrip, PostClientToTripDTO request);
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

    public async Task<PostClientToTripDTO> AddClientToTripAsync(int idTrip, PostClientToTripDTO request)
    {
        var trip = await data.Trips
            .Include(t => t.ClientTrips)
            .FirstOrDefaultAsync(t => t.IdTrip == idTrip);

        if (trip == null)
        {
            throw new NotFound($"Trip with id {idTrip} not found");
        }

        if (trip.DateFrom <= DateTime.Now)
        {
            throw new BadRequestException("Cannot register for a trip that has already started");
        }

        var existingClient = await data.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.Pesel == request.Pesel);

        if (existingClient != null)
        {
            var alreadyOnTrip = await data.ClientTrips
                .AnyAsync(ct => ct.IdClient == existingClient.IdClient && ct.IdTrip == idTrip);

            if (alreadyOnTrip)
            {
                throw new BadRequestException("Client is already registered for this trip");
            }
        }

        // Jeśli klient nie istnieje, utwórz nowego
        var client = new Client
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Telephone = request.Telephone,
            Pesel = request.Pesel
        };
        
        var clientTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = request.PaymentDate
        };

        await data.ClientTrips.AddAsync(clientTrip);
        await data.SaveChangesAsync();

        return request;
    }
}