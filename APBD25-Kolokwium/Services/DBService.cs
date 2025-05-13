using APBD25_CW9.Exceptions;
using APBD25_Kolokwium.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace APBD25_Kolokwium.Services;

public class DBService : IDBService
{
    private readonly IConfiguration _configuration;

    public DBService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IEnumerable<Customer>> Get(int id, CancellationToken cancellationToken)
    {
        Console.WriteLine(_configuration.GetConnectionString("Default"));
        if(id <= 0)
            throw new BadRequestException("Invalid id");
        string testcommand = "Select Count(*) from Customer where customer_id = @id";
        string comand = "Select * from Customer where customer_id = @id";
        
        List<Customer> customers = new List<Customer>();
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")) )
        using (SqlCommand cmd = new SqlCommand(testcommand, conn))
        {
            await conn.OpenAsync(cancellationToken);
            cmd.Parameters.AddWithValue("@id", id);
            
            var check = (int)(await cmd.ExecuteScalarAsync(cancellationToken));
            if (check == 0)
                throw new NotFoundException("Customer not found");
            
            cmd.Parameters.Clear();
            
            cmd.CommandText = comand;
            
            cmd.Parameters.AddWithValue("@id", id);

            using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    customers.Add(new Customer()
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                    });
                }
            }
            return customers;
        }
    }
}