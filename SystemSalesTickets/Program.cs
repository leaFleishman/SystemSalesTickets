using SystemSalesTickets;
using SystemSalesTickets.Core.Repository;
using SystemSalesTickets.Service.Service;
using Microsoft.EntityFrameworkCore;
using SystemSalesTickets.Core;
using SystemSalesTickets.Core.Interfaces;
using SystemSalesTickets.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using NLog.Web;
using SystemSalesTickets.API.Middleware;
using Microsoft.AspNetCore.Identity;
using SystemSalesTickets.Core.Models;
using SystemSalesTickets.Service.Background;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientPolicy", policy =>
    {
        policy
            .WithOrigins("https://systemsalestickets-client.onrender.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseConnectionHealthCheck>("DatabaseConnection");
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHostedService<HealthMonitorService>();


builder.Services.AddScoped<IEventService, EventService>();

builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<ISeatService, SeatService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEventSeatService, EventSeatService>();
builder.Services.AddScoped<IEventSeatRepository, EventSeatRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));


builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Logging.ClearProviders();
builder.Host.UseNLog();



builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
        };
    });
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Apply EF migrations on startup when RunMigrations=true (set as an env var in the cloud).
// Off by default, so local runs never touch the database schema unexpectedly.
if (app.Configuration.GetValue<bool>("RunMigrations"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandlingMiddleware();
app.UsePerformanceMiddleware();
app.UseMiddleware<LoggingMiddleware>();
app.UseHttpsRedirection();

app.UseCors("ClientPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();
