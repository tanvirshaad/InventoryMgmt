using InventoryMgmt.DAL.Data;
using Microsoft.EntityFrameworkCore;
using InventoryMgmt.DAL.Repos;
using InventoryMgmt.DAL.Interfaces;
using InventoryMgmt.BLL.Services;
using InventoryMgmt.DAL.EF.TableModels;
using InventoryMgmt.BLL.Profiles;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using InventoryMgmt.MVC.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add SignalR for real-time chat
builder.Services.AddSignalR();

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("InventoryMgmt")));

// Add JWT Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// Add Authorization
builder.Services.AddAuthorization();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Register repositories
builder.Services.AddScoped(typeof(IRepo<>), typeof(Repo<>));
builder.Services.AddScoped<IInventoryRepo, InventoryRepo>();

// Register services
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Initialize database and create admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var jwtService = services.GetRequiredService<IJwtService>();
        
        // Ensure database is created
        context.Database.Migrate();
        
        // Create admin user if it doesn't exist
        if (!context.Users.Any())
        {
            var adminUser = new User
            {
                UserName = "admin",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                Role = "Admin",
                PasswordHash = jwtService.HashPassword("Admin123!"),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            context.SaveChanges();
            Console.WriteLine("Default admin user created successfully");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        Console.WriteLine("Error initializing database: " + ex.Message);
        if (ex.InnerException != null)
        {
            Console.WriteLine("Inner exception: " + ex.InnerException.Message);
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map SignalR hub
app.MapHub<CommentHub>("/commentHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();