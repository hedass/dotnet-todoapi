using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using onboarding.bll.Interfaces;
using onboarding.bll.Services;
using onboarding.dal;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var connection = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddEnvironmentVariables().AddJsonFile("appsettings.Development.json");
    connection = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
}

builder.Services.AddDbContext<ToDoItemDbContext>(options => options.UseSqlServer(connection));

// Add services to the container.
builder.Services.AddHostedService<SchedulerService>();

builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<ToDoRepository>();
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddScoped<IToDoService, ToDoService>();

builder.Services.AddControllers().ConfigureApiBehaviorOptions(x => { x.SuppressMapClientErrors = true; });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API",
        Description = "An ASP.NET Core Web API for managing ToDo items",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });

    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("AZURE_CACHE_CONNECTIONSTRING");
    options.InstanceName = "todosean";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
