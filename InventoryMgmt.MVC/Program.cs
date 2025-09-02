using InventoryMgmt.DAL.Data;
using Microsoft.EntityFrameworkCore;
using InventoryMgmt.DAL.Repos;
using InventoryMgmt.DAL.Interfaces;
using InventoryMgmt.BLL.Services;
using InventoryMgmt.BLL.Interfaces;
using InventoryMgmt.DAL.EF.TableModels;
using InventoryMgmt.BLL.Profiles;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;
using InventoryMgmt.MVC.Hubs;
using InventoryMgmt.DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// Register repositories
builder.Services.AddScoped(typeof(IRepo<>), typeof(Repo<>));

// Register specific repositories
builder.Services.AddScoped<IInventoryRepo, InventoryRepo>();
builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<ITagRepo, TagRepo>();
builder.Services.AddScoped<IItemRepo, ItemRepo>();
builder.Services.AddScoped<ICommentRepo, CommentRepo>();
builder.Services.AddScoped<IInventoryAccessRepo, InventoryAccessRepo>();
builder.Services.AddScoped<IInventoryTagRepo, InventoryTagRepo>();
builder.Services.AddScoped<IItemLikeRepo, ItemLikeRepo>();

// Register services
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<InventoryMgmt.BLL.Interfaces.IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<DataAccess>();
builder.Services.AddScoped<ICustomFieldService, CustomFieldService>();
builder.Services.AddScoped<ICustomFieldProcessor, CustomFieldProcessor>();

// Register the new specialized services created during refactoring
builder.Services.AddScoped<InventoryMgmt.BLL.Services.ICustomFieldService, InventoryMgmt.BLL.Services.CustomFieldService>();
builder.Services.AddScoped<InventoryMgmt.BLL.Services.ICustomIdService, InventoryMgmt.BLL.Services.CustomIdService>();
builder.Services.AddScoped<InventoryMgmt.BLL.Services.ITagService, InventoryMgmt.BLL.Services.TagService>();
builder.Services.AddScoped<InventoryMgmt.BLL.Services.IInventoryAccessService, InventoryMgmt.BLL.Services.InventoryAccessService>();

// Add SignalR
builder.Services.AddSignalR();

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
                UpdatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            context.SaveChanges();
            Console.WriteLine("Default admin user created successfully");
        }

        // Create default categories if they don't exist
        if (!context.Categories.Any())
        {
            var defaultCategories = new[]
            {
                new Category { Name = "Electronics", Description = "Electronic devices and gadgets", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Name = "Books", Description = "Books and publications", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Name = "Clothing", Description = "Apparel and accessories", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Name = "Home & Garden", Description = "Home improvement and garden items", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Name = "Sports", Description = "Sports equipment and gear", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Name = "Tools", Description = "Hand tools and power tools", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Name = "Office Supplies", Description = "Office and stationery items", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Category { Name = "Other", Description = "Miscellaneous items", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            context.Categories.AddRange(defaultCategories);
            context.SaveChanges();
            Console.WriteLine("Default categories created successfully");
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map SignalR hub
app.MapHub<CommentHub>("/commentHub");

app.Run();
