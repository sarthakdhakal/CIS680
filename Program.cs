using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewDotnet.Authentication;
using NewDotnet.Code;  // Ensure namespace matches your project structure
using NewDotnet.Context;
using NewDotnet.Middleware;
using NewDotnet.Models;
using System.Security.Claims; // Ensure namespace matches your project structure
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<OODBModelContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("OODBModel")));


builder.Services.AddControllers(options =>
{
    // Global exception filter can be added here if custom
    options.Filters.Add(new GlobalExceptionFilterAttribute());
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressMapClientErrors = true; // Customize client error response
    options.SuppressModelStateInvalidFilter = true; // Take control over model state validation
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    c => c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First()));
builder.Services.AddMemoryCache();
// Correctly bind AppSettings to a strongly typed configuration object
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NewDotnet", Version = "v1" });
});

var app = builder.Build();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    app.Use(async (context, next) =>
    {
        // Check if the user is authenticated, otherwise set default claims
        if (!context.User.Identity.IsAuthenticated)
        {
            var claims = new List<Claim>
        {
            new Claim("starId", "ad0000mi"),  // Replace with a suitable default value
            new Claim("expiry", DateTime.Now.AddDays(30).ToString()), // Set a default expiry 30 days from now
            new Claim("guestId", "f9dbfbf209db37a0b0234677d5684ee4d3f93764"),  // Replace with a suitable default value
            new Claim("keyType", ""),  // Replace with a suitable default value
            new Claim("playlist", "1")  // Set a default playlist ID
            // ... other claims
        };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            context.User = claimsPrincipal;
        }

        // Continue processing the request
        await next.Invoke();
    });


app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();
app.UseExceptionLogger();
app.Run();
