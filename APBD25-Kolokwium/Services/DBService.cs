using System.Data.Common;
using APBD25_CW9.Exceptions;
using APBD25_Kolokwium.Models;
using APBD25_Kolokwium.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NuGet.Packaging.Signing;

namespace APBD25_Kolokwium.Services;

public class DBService : IDBService
{
    private readonly IConfiguration _configuration;

    public DBService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Booking> GetBooking(int id, CancellationToken cancellationToken)
    {
        if(id <= 0)
            throw new BadRequestException("Invalid id, Id should be greater than 0");
        
        Booking booking = new Booking();
        
        string testcommand = "SELECT count(*) from Booking where booking_id = @id;";
        string getBookingcomand = @"SELECT date, G.first_name, G.last_name,G.date_of_birth, E.first_name, E.last_name, E.employee_number 
                                    from Booking 
                                    join dbo.Employee E on E.employee_id = Booking.employee_id 
                                    join dbo.Guest G on G.guest_id = Booking.guest_id 
                                    where booking_id = @id";
        
        string getAtractions = "SELECT A.name , A.price, amount from  Booking_Attraction join dbo.Attraction A on A.attraction_id = Booking_Attraction.attraction_id where booking_id = @id";
        
        await using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("LocalDB")) )
        await using( SqlCommand cmd = new SqlCommand(testcommand, conn))
        {
            await conn.OpenAsync(cancellationToken);
            cmd.Parameters.AddWithValue("@id", id);
            
            var check = (int)(await cmd.ExecuteScalarAsync(cancellationToken));
            if (check == 0)
                throw new NotFoundException("Booking not found");
            
            cmd.Parameters.Clear();
            
            cmd.CommandText = getBookingcomand;
            
            cmd.Parameters.AddWithValue("@id", id);

            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    booking.date = reader.GetDateTime(0);
                    booking.guest = new Guest()
                    {
                        firstName = reader.GetString(1),
                        lastName = reader.GetString(2),
                        dateOfBirth = reader.GetDateTime(3),
                    };
                    booking.employee = new Employee()
                    {
                        FirstName = reader.GetString(4),
                        LastName = reader.GetString(5),
                        EmployeeNumber = reader.GetString(6),
                    };
                }
            }
            cmd.Parameters.Clear();
            
            cmd.CommandText = getAtractions;
            cmd.Parameters.AddWithValue("@id", id);
            booking.attractions = new List<Attraction>();
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                while (await reader.ReadAsync(cancellationToken))
                {
                    booking.attractions.Add(new Attraction()
                    {
                        Name = reader.GetString(0),
                        price = reader.GetDecimal(1),
                        amount = reader.GetInt32(2),
                    });
                }
            return booking;
        }
    }

    public async Task<int> Post(BookingDto bookingDto, CancellationToken cancellationToken)
    {
        string checkBookingCommnad = "SELECT COUNT(*) from  Booking where booking_id = @booking_id";
        string checkGuestCommnad = "SELECT COUNT(*) from Guest where guest_id = @guest_id;";
        string checkEmployyCommand = "SELECT employee_id from Employee where employee_number = @employee_number";
        string checkAttractionsCommand = "SELECT attraction_id from Attraction where name = @Attraction_name;";
        
        
        int employeeId = 0;
        
        await using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("LocalDB")) )
        await using (SqlCommand cmd = new SqlCommand(checkBookingCommnad, conn))
        {
            await conn.OpenAsync(cancellationToken);
            
            cmd.Parameters.AddWithValue("@booking_id", bookingDto.bookingId);
            var checkB = (int)(await cmd.ExecuteScalarAsync(cancellationToken));
            if (checkB == 1)
                throw new ConflictException("Booking already exists");
            
            cmd.Parameters.Clear();
            cmd.CommandText = checkGuestCommnad;
            
            cmd.Parameters.AddWithValue("@guest_id", bookingDto.guestId);
            
            var checkG = (int)(await cmd.ExecuteScalarAsync(cancellationToken));
            if (checkG == 0)
                throw new NotFoundException("Guest not found");
            
            cmd.Parameters.Clear();
            cmd.CommandText = checkEmployyCommand;
            cmd.Parameters.AddWithValue("@employee_number", bookingDto.employeeNumber);

            try
            {
                employeeId = (int)(await cmd.ExecuteScalarAsync(cancellationToken));
            }
            catch (Exception e)
            {   
                throw new NotFoundException("Employee not found"); 
            }
            
            var attractionsMap = new Dictionary<int,int>();
            
            foreach (var attraction in bookingDto.attractions)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = checkAttractionsCommand;
                cmd.Parameters.AddWithValue("@Attraction_name", attraction.name);
                try
                {
                    var id = (int)(await cmd.ExecuteScalarAsync(cancellationToken));
                    attractionsMap[id] = attraction.amount;
                }
                catch (Exception e)
                {
                    throw new NotFoundException($"Attraction with name: {attraction.name} not found");
                }
            }
            
            DbTransaction transaction = await conn.BeginTransactionAsync(cancellationToken);
            cmd.Transaction = transaction as SqlTransaction;

            var instertBookingCommand = "INSERT INTO Booking (booking_id, guest_id, employee_id, date) VALUES (@booking_id, @guest_id, @employee_id, @date);";
            try
            {
                cmd.Parameters.Clear();
                
                cmd.CommandText = instertBookingCommand;
                
                cmd.Parameters.AddWithValue("@booking_id", bookingDto.bookingId);
                cmd.Parameters.AddWithValue("@guest_id", bookingDto.guestId);
                cmd.Parameters.AddWithValue("@employee_id", employeeId);
                DateTime date = DateTime.Today;
                cmd.Parameters.AddWithValue("@date", date);
                
                
                await cmd.ExecuteNonQueryAsync(cancellationToken);

                string insterAttractionsCommand = "INSERT INTO Booking_Attraction (booking_id, attraction_id, amount) VALUES (@booking_id, @attraction_id, @amount);";
                
                foreach (var attraction in attractionsMap)
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = insterAttractionsCommand;
                    
                    cmd.Parameters.AddWithValue("@booking_id", bookingDto.bookingId);
                    cmd.Parameters.AddWithValue("@attraction_id", attraction.Key);
                    cmd.Parameters.AddWithValue("@amount", attraction.Value);
                    
                  await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                
                await transaction.RollbackAsync();
                throw new ConflictException("Conflict during transaction");
            }

            return 1;
        }
        
    }
}