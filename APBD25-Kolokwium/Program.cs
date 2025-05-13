using APBD25_Kolokwium.Services;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IDBService, DBService>();

builder.Services.AddSwaggerGen();


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.DocExpansion(DocExpansion.List);
        c.DefaultModelExpandDepth(0);
        c.DisplayRequestDuration();
        c.EnableFilter();
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();