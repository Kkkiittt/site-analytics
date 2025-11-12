using System.Text;

using Analite.Api.Services;
using Analite.Application.Implementations;
using Analite.Application.Interfaces;
using Analite.Domain.Entities;
using Analite.Infrastructure.EFCore;

using BCrypt.Net;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("AnalyticsConnection")));


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
	opt.SwaggerDoc("v1", new OpenApiInfo { Title = "UserJourney-Analytics", Version = "v1" });
	opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Description = "Введите токен JWT: Bearer {ваш_токен}",
		Name = "Authorization",
		Type = SecuritySchemeType.ApiKey
	});
	opt.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
			new string[] {}
		}
	});
});



var tokenSettings = configuration.GetSection("Jwt");
var keyValue = tokenSettings["Key"];
if(string.IsNullOrWhiteSpace(keyValue))
	throw new InvalidOperationException("Missing Jwt:Key in configuration");

var key = Encoding.UTF8.GetBytes(keyValue);

builder.Services.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = tokenSettings["Issuer"],
			ValidAudience = tokenSettings["Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(key)
		};
	});


builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IFlowService, FlowService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IBlockService, BlockService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddStackExchangeRedisCache(opt =>
{
	opt.Configuration = "localhost:6379";
});



var app = builder.Build();

if(app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

using(var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetService<AppDbContext>() ?? throw new Exception("Fatal db error");
	await db.Database.MigrateAsync();
	if(!await db.Customers.AnyAsync(c => c.Email == "admin@mail.com"))
	{
		Customer admin = new Customer()
		{
			SecurityStamp = Guid.NewGuid().ToString(),
			Surname = "Admin",
			CreatedAt = DateTime.UtcNow,
			Email = "admin@mail.com",
			Id = Guid.NewGuid(),
			IsActive = true,
			IsApproved = true,
			Name = "Admin",
			PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
			PublicKey = Guid.NewGuid().ToString(),
			Role = Roles.SuperAdmin,
			UpdatedAt = DateTime.UtcNow,
		};
		db.Customers.Add(admin);
		await db.SaveChangesAsync();
	}
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
