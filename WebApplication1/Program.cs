using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using WebApplication1.Data;
using WebApplication1.Infrastructure.Repositories;
using WebApplication1.Infrastructure.UnitOfWork;

// =========================================================================
// Sistema Integral de Asistencias Académicas mediante QR
// Backend API (PDF §3.1 "Subsistema Servidor").
// =========================================================================
var builder = WebApplication.CreateBuilder(args);

// -------- Servicios MVC + Swagger --------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------- EF Core (PostgreSQL vía Supabase) --------
// Nota: el PDF menciona SQL Server 2022, el proyecto real usa PostgreSQL/Supabase.
// La arquitectura es la misma; EF Core abstrae el proveedor.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------- Repository + Unit of Work (PDF §3.2) --------
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// -------- JWT (PDF §6) --------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// -------- CORS (PDF §6) --------
// El cliente MAUI puede correr en cualquier host/IP durante desarrollo; en prod
// se restringe a dominios conocidos.
const string MauiClientPolicy = "MauiClientPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MauiClientPolicy, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// -------- Rate Limiting (PDF §6) --------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Política global: 100 req/minuto por IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Política estricta para login (protección contra brute force)
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});

var app = builder.Build();

// =========================================================================
// Pipeline HTTP
// =========================================================================
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(MauiClientPolicy);
app.UseRateLimiter();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Urls.Add("http://0.0.0.0:10000");
app.Run();
