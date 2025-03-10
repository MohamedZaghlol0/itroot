using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FluentValidation.AspNetCore;
using ItRoots.Business.Services;
using ItRoots.Data.Repositories;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// 1. Minimal JWT config (no issuer/audience)
var jwtKey = builder.Configuration["JwtSettings:Key"] ?? "DefaultDevKey";

// 2. Register DB connection
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddTransient<IDbConnection>(sp => new SqlConnection(connString));


// 3. Register Repositories & Services
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

// 4. JWT Setup: Only validating signing key & lifetime
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
        ValidateIssuer = false,       // Not validating issuer
        ValidateAudience = false,     // Not validating audience
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// 5. Authorization
builder.Services.AddAuthorization();

// 6. Add MVC + FluentValidation (optional)
builder.Services.AddControllersWithViews()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());

var app = builder.Build();

// Middleware pipeline
app.UseDeveloperExceptionPage(); // For debugging
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Register}/{id?}"
);



app.Run();
