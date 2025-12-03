// Note: repository implementation removed for workshop exercise (TODOs in project files)
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CampusWorkshops.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using CampusWorkshops.Api.Infrastructure.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using CampusWorkshops.Api.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);


// JWT
#region JWT

var jwt = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
      options.TokenValidationParameters = new()
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwt["Issuer"],
          ValidAudience = jwt["Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
          ClockSkew = TimeSpan.FromMinutes(1) // previsível para testes
      };
  });

builder.Services.AddAuthorization();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanWriteWorkshops", p => p.RequireRole("Instructor","Admin"));
    options.AddPolicy("CanDeleteWorkshops", p => p.RequireRole("Admin"));
    options.AddPolicy("CanViewAnalytics",  p => p.RequireRole("Admin"));
});

#endregion

// Add services
builder.Services.AddDbContext<WorkshopsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("WorkshopsDb")));

builder.Services.AddMemoryCache();
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));


// Primeiro registramos a implementação EF concreta
builder.Services.AddScoped<EfWorkshopRepository>();

// Depois expomos IWorkshopRepository como o "EF envelopado por cache"
builder.Services.AddScoped<IWorkshopRepository>(sp =>
    new CachedWorkshopRepository(
        sp.GetRequiredService<EfWorkshopRepository>(),
        sp.GetRequiredService<IMemoryCache>(),
        sp.GetRequiredService<IOptionsSnapshot<CacheSettings>>()));

// Lifetimes de exemplo
builder.Services.AddTransient<IRequestIdTransient, RequestIdTransient>();
builder.Services.AddScoped<IRequestIdScoped, RequestIdScoped>();
builder.Services.AddSingleton<IRequestIdSingleton, RequestIdSingleton>();

// Exemplo de "captive dependency"
builder.Services.AddTransient<IPerRequestClock, PerRequestClock>(); // Transient
builder.Services.AddSingleton<ReportingSingleton>();


// Removendo para ativar o cached repository
// builder.Services.AddScoped<IWorkshopRepository, EfWorkshopRepository>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CampusWorkshops API",
        Version = "v1",
        Description = "API para gestão de workshops do campus."
    });
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Cole apenas o JWT (sem 'Bearer ').",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,   // <- IMPORTANTE
        Scheme = "bearer",                // <- minúsculo
        BearerFormat = "JWT",
        Reference = new OpenApiReference   // <- garante que o requirement aponte para esta definição
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    o.AddSecurityDefinition("Bearer", bearerScheme);

    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// DI


#region CORS: Adiciona o Serviço com a Política Nomeada

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// 1. Adicionar o serviço CORS e definir a política
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          // Use WithOrigins para especificar as URLs permitidas.
                          // Inclua o esquema (http ou https) e a porta, se houver.
                          policy.WithOrigins("http://localhost:5173") //Meus clientes confiáveis entram aqui.
                                .AllowAnyHeader() // Permite qualquer cabeçalho na requisição
                                .AllowAnyMethod(); // Permite todos os métodos HTTP (GET, POST, etc.)
                          
                          // Se estiver usando cookies ou autenticação baseada em credenciais, 
                          // substitua AllowAnyOrigin por WithOrigins e adicione:
                          // .AllowCredentials(); 
                          // *Nota: Não é permitido usar .AllowAnyOrigin() junto com .AllowCredentials().

                          //Breve explicação:
                          
                          // _myAllowSpecificOrigins: É o nome da sua política. Use um nome descritivo.

                          // WithOrigins(...): Este método é crucial. Você lista as URLs exatas (origens) que terão permissão para acessar sua API. 
                          // Evite usar AllowAnyOrigin() (que permite todas as origens) para manter a segurança.

                          // AllowAnyHeader() e AllowAnyMethod(): Permitem que os clientes usem qualquer cabeçalho e método HTTP. 
                          // Você pode restringi-los ainda mais usando WithHeaders(...) e WithMethods(...) se necessário.  

                      });
});

#endregion

var app = builder.Build();

// Exception handler that returns RFC7807 ProblemDetails for unhandled errors
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var ex = feature?.Error;

        var pd = new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = app.Environment.IsDevelopment() ? ex?.Message : null
        };

        context.Response.StatusCode = pd.Status.Value;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(pd);
    });
});

app.UseHttpsRedirection();

app.UseAuthentication();   // <-- antes

#region CORS: Usar o Middleware aqui
// 2. Usar o Middleware CORS com o nome da política
app.UseCors(MyAllowSpecificOrigins);

/* app.UseCors(MyAllowSpecificOrigins): Aplica a política CORS que você definiu na etapa 1.

💡 Dicas Adicionais

    Ambientes de Desenvolvimento/Produção: É comum definir políticas separadas para o ambiente de Desenvolvimento (ex: http://localhost:4200) e 
    Produção, usando a configuração do ASP.NET Core (IConfiguration) para ler as origens permitidas.

    Ordem do Middleware: app.UseCors() deve ser chamado antes de app.UseAuthorization() e app.MapControllers() (ou app.UseMvc()). 
    A ordem é importante no pipeline de processamento de requisições do ASP.NET Core.

*/

#endregion

app.UseAuthorization();    // <-- depois

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CampusWorkshops API v1");
    c.RoutePrefix = "swagger"; // serve at /swagger
});

app.MapControllers();

app.Run();
