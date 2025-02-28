using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=localhost\\MSSQLSERVER01;Database=master;Trusted_Connection=True;TrustServerCertificate=True;"));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll"); // Apply CORS policy

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//var superUsers = new List<SuperUser> {
//    new SuperUser {Email = "matei@popescu.net", Password = "welcome1"},
//    new SuperUser {Email = "diana@popescu.net", Password = "welcome2"},
//    new SuperUser {Email = "pitagora@cat.net", Password = "welcome3"}
//};

//Database Context
var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
dbContext.Database.EnsureCreated(); //Create Database if not exists


////////////////////////////////////////////////////////////////////
///////////////////////////////SUPERUSER////////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/superuser", async(AppDbContext db) =>
    await db.SuperUser.ToListAsync());

app.MapGet("/superuser/{email}", async(AppDbContext db, string email) =>
    await db.SuperUser.FindAsync(email) is SuperUser user ? Results.Ok(user) : Results.NotFound());

app.MapPost("/superuser", async(AppDbContext db, SuperUser superUser) =>
{
    db.SuperUser.Add(superUser);
    await db.SaveChangesAsync();
    return Results.Created($"/superuser/{superUser.Email}", superUser);
});

app.MapPut("/superuser/{email}", async(AppDbContext db, string email, SuperUser updatedSuperUser) =>
{
    var existingUser = await db.SuperUser.FindAsync(email);
    if ( existingUser == null) 
    {
        return Results.NotFound();
    }
    existingUser.Password= updatedSuperUser.Password;
    await db.SaveChangesAsync();
    return Results.Ok(existingUser);
});

app.MapDelete("/superuser/{email}", async(AppDbContext db, string email) =>
{
    var userToRemove = await db.SuperUser.FindAsync(email);
    if (userToRemove == null)
    {
        return Results.NotFound();
    }
    db.SuperUser.Remove(userToRemove);
    await db.SaveChangesAsync();
    return Results.Ok(userToRemove);
});

////////////////////////////////////////////////////////////////////
////////////////////////////////USER////////////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/user", async (AppDbContext db) =>
    await db.User.ToListAsync());

app.MapGet("/user/{id}", async (AppDbContext db, int id) =>
    await db.User.FindAsync(id) is User user ? Results.Ok(user) : Results.NotFound());

app.MapPost("/user", async (AppDbContext db, User user) => { 
    db.User.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/user/{user.Id}", user);
});

app.MapPut("/user/{id}", async (AppDbContext db, int id, User updatedUser) =>
{
    var existingUser = await db.User.FindAsync(id);
    if (existingUser == null)
    {
        return Results.NotFound();
    }

    existingUser.Email = updatedUser.Email;
    existingUser.Full_Name = updatedUser.Full_Name;
    existingUser.Password = updatedUser.Password;
    existingUser.Company_Name = updatedUser.Company_Name;

    await db.SaveChangesAsync();
    return Results.Ok(existingUser);
});

app.MapDelete("/user/{id}", async (AppDbContext db, int id) =>
{
    var userToRemove = await db.User.FindAsync(id);
    if (userToRemove == null)
    {
        return Results.NotFound();
    }
    db.User.Remove(userToRemove);
    await db.SaveChangesAsync();
    return Results.Ok(userToRemove);
});
////////////////////////////////////////////////////////////////////
//////////////////////////////COMPANY///////////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/cpmpany", async (AppDbContext db) =>
    await db.Company.ToListAsync());

app.MapGet("/company/{company_name}", async (AppDbContext db, string company_name) =>
    await db.Company.FindAsync(company_name) is Company company ? Results.Ok(company) : Results.NotFound());

app.MapPost("/company", async (AppDbContext db, Company company) =>
{
    db.Company.Add(company);
    await db.SaveChangesAsync();
    return Results.Created($"/company/{company.Company_Name}", company);
});

app.MapPut("/company/{company_name}", async (AppDbContext db, string company_name, Company updatedCompany) =>
{
    var existingCompany = await db.Company.FindAsync(company_name);
    if (existingCompany == null)
    {
        return Results.NotFound();
    }
    existingCompany.Email = updatedCompany.Email;
    existingCompany.Password = updatedCompany.Password;
    existingCompany.Nr_of_accounts = updatedCompany.Nr_of_accounts;
    existingCompany.SuperUserEmail = updatedCompany.SuperUserEmail;
    await db.SaveChangesAsync();
    return Results.Ok(existingCompany);
});

app.MapDelete("/company/{companyName}", async (AppDbContext db, string companyName) =>
{
    var companyToRemove = await db.Company.FindAsync(companyName);
    if (companyToRemove == null) return Results.NotFound();

    db.Company.Remove(companyToRemove);
    await db.SaveChangesAsync();
    return Results.Ok(companyToRemove);
});


app.Run("http://0.0.0.0:7287");

class AppDbContext : DbContext
{
    public DbSet<SuperUser> SuperUser { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<Company> Company { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SuperUser>()
            .HasKey(s => s.Email);
        modelBuilder.Entity<SuperUser>()
            .ToTable("SuperUser");

        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
        modelBuilder.Entity<User>()
            .ToTable("User");

        modelBuilder.Entity<Company>()
            .HasKey(c => c.Company_Name);
        modelBuilder.Entity<Company>()
            .ToTable("Company");
    }
}


class SuperUser
{
    [Key]
    public string Email { get; set; }
    public string Password { get; set; }
}

class User
{
    [Key]
    public int Id { get; set; }
    public string Full_Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Company_Name { get; set; }
}

class Company
{
    [Key]
    public string Company_Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int Nr_of_accounts { get; set; }
    public string SuperUserEmail { get; set; }
}