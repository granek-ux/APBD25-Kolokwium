using APBD25_Kolokwium.Models;
using Microsoft.AspNetCore.Mvc;

namespace APBD25_Kolokwium.Services;

public interface IDBService
{
    
    public Task<IEnumerable<Customer>> Get(int id,CancellationToken cancellationToken);
    
}