using SystemSalesTicketsPresentation.Interfaces;
using SystemSalesTicketsData;
using SystemSalesTicketsInfrastructure;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;
using SystemSalesTickets.Api.Middleware;
using SystemSalesTicketsCore.Repository;
using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core;
using AutoMapper;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IEventService, EventService>();

builder.Services.AddScoped<IOrderService, OrderService>(); 

builder.Services.AddScoped<ISeatService, SeatService>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));


builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);




builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCheckIfBuyMiddleware();
app.UseCheckShabatMiddleware();

app.MapControllers();

app.Run();
