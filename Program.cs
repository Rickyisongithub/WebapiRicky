using AutoMapper;
using KairosWebAPI.AutoMapperProfile;
using KairosWebAPI.DatabaseContext;
using KairosWebAPI.Extensions;
using KairosWebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    //options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve; // Use Preserve for handling cycles
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          policy
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .SetIsOriginAllowed(_ => true)
                          .AllowCredentials();

                      });
});

// Auto Mapper Configurations
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDependencyInjection();
builder.Services.ConfigureServices(builder.Configuration);
//builder.Services.AddSignalR();
builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true;
    o.MaximumReceiveMessageSize =  1024 * 1024; //1 megabytes
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Kairos FMS API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s: builder.Configuration["JWT:Key"] ?? ""))

    };
});

var app = builder.Build();

try
{
    using var serviceScope = app.Services.GetService<IServiceScopeFactory>()!.CreateScope();
    var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
});
app.UseStaticFiles();
//if (app.Environment.IsDevelopment())
//{
//code here only for development mode
//}
app.UseCors(myAllowSpecificOrigins);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<JourneyHub>("/journeyHub");

app.Run();
