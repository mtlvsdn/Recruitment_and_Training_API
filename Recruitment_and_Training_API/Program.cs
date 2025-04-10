using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Mvc;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=localhost\\MSSQLSERVER01;Database=RecruitmentAndTraining;Trusted_Connection=True;TrustServerCertificate=True;"));

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

//Database Context
var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

try
{
    // Remove all table creation commands
    // await dbContext.Database.ExecuteSqlRawAsync(@"
    //     -- Create SuperUser table
    //     IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SuperUser')
    //     BEGIN
    //         CREATE TABLE SuperUser (
    //             SuperUseremail VARCHAR(35) PRIMARY KEY,
    //             password VARCHAR(64) NOT NULL
    //         )
    //     END
    // ... existing code ...
}
catch (Exception ex)
{
    Console.WriteLine($"Error initializing database: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }
}

////////////////////////////////////////////////////////////////////
///////////////////////////////SUPERUSER////////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/superuser", async (AppDbContext db) =>
    await db.SuperUser.ToListAsync());

app.MapGet("/superuser/{SuperUseremail}", async (AppDbContext db, string SuperUseremail) =>
    await db.SuperUser.FindAsync(SuperUseremail) is SuperUser user ? Results.Ok(user) : Results.NotFound());

app.MapPost("/superuser", async (AppDbContext db, SuperUser superUser) =>
{
    db.SuperUser.Add(superUser);
    await db.SaveChangesAsync();
    return Results.Created($"/superuser/{superUser.SuperUseremail}", superUser);
});

app.MapPut("/superuser/{SuperUseremail}", async (AppDbContext db, string SuperUseremail, SuperUser updatedSuperUser) =>
{
    var existingUser = await db.SuperUser.FindAsync(SuperUseremail);
    if (existingUser == null)
    {
        return Results.NotFound();
    }
    existingUser.Password = updatedSuperUser.Password;
    await db.SaveChangesAsync();
    return Results.Ok(existingUser);
});

app.MapDelete("/superuser/{SuperUseremail}", async (AppDbContext db, string SuperUseremail) =>
{
    var userToRemove = await db.SuperUser.FindAsync(SuperUseremail);
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

app.MapGet("/user/byemail/{email}", async (AppDbContext db, string email) =>
    await db.User.FirstOrDefaultAsync(u => u.Email == email) is User user
        ? Results.Ok(user)
        : Results.NotFound());

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
app.MapGet("/company", async (AppDbContext db) =>
    await db.Company.ToListAsync());

app.MapGet("/company/byemail/{email}", async (AppDbContext db, string email) =>
    await db.Company.FirstOrDefaultAsync(c => c.Email == email) is Company company
        ? Results.Ok(company)
        : Results.NotFound());

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
    existingCompany.SuperUseremail = updatedCompany.SuperUseremail;
    await db.SaveChangesAsync();
    return Results.Ok(existingCompany);
});

app.MapDelete("/company/{companyName}", async (AppDbContext db, string companyName) =>
{
    var companyToRemove = await db.Company.FindAsync(companyName);
    if (companyToRemove == null) return Results.NotFound();

    var relatedUsers = await db.User.Where(u => u.Company_Name == companyName).ToListAsync();
    if (relatedUsers.Any())
    {
        db.User.RemoveRange(relatedUsers);
    }

    db.Company.Remove(companyToRemove);
    await db.SaveChangesAsync();
    return Results.Ok(companyToRemove);
});
////////////////////////////////////////////////////////////////////
/////////////////////////AUTHENTICATION DEVS////////////////////////
////////////////////////////////////////////////////////////////////
app.MapPost("/authenticate", async (AppDbContext db, LoginRequest loginRequest) =>
{
    var user = await db.SuperUser.FindAsync(loginRequest.SuperUseremail);

    if (user == null)
        return Results.NotFound("User not found");

    if (user.Password != loginRequest.Password)
        return Results.Unauthorized();

    var token = Guid.NewGuid().ToString();

    return Results.Ok(new
    {
        Token = token,
        Email = user.SuperUseremail
    });
});

////////////////////////////////////////////////////////////////////
/////////////////////////AUTHENTICATION USERS///////////////////////
////////////////////////////////////////////////////////////////////
app.MapPost("/authenticate-user", async (AppDbContext db, LoginRequestUser loginRequestUser) =>
{
    try
    {
        // Find user by email instead of trying to use FindAsync with email as primary key
        var user = await db.User.FirstOrDefaultAsync(u => u.Email == loginRequestUser.Email);

        if (user == null)
            return Results.NotFound("User not found");

        if (user.Password != loginRequestUser.Password)
            return Results.Unauthorized();

        var token = Guid.NewGuid().ToString();

        return Results.Ok(new
        {
            Token = token,
            Email = user.Email
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Authentication error: {ex.Message}");
    }
});

////////////////////////////////////////////////////////////////////
/////////////////////////AUTHENTICATION COMPANY/////////////////////
////////////////////////////////////////////////////////////////////
app.MapPost("/authenticate-company", async (AppDbContext db, LoginRequestUser loginRequestCompany) =>
{
    var user = await db.Company.FirstOrDefaultAsync(c => c.Email == loginRequestCompany.Email);

    if (user == null)
        return Results.NotFound("Company not found");

    if (user.Password != loginRequestCompany.Password)
        return Results.Unauthorized();

    var token = Guid.NewGuid().ToString();

    return Results.Ok(new
    {
        Token = token,
        Email = user.Email
    });
});

////////////////////////////////////////////////////////////////////
///////////////////////////HOMEPAGE CARDS///////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/company/count", async (AppDbContext db) =>
{
    var companyCount = await db.Company.CountAsync();
    return Results.Ok(companyCount);
});

app.MapGet("/user/count", async (AppDbContext db) =>
{
    var userCount = await db.User.CountAsync();
    return Results.Ok(userCount);
});

////////////////////////////////////////////////////////////////////
///////////////////////////////CV PDF///////////////////////////////
////////////////////////////////////////////////////////////////////
app.MapGet("/cv-pdf/{userId}", async (AppDbContext db, int userId) =>
    await db.CvPdf.FindAsync(userId) is CvPdf cv ? Results.Ok(cv) : Results.NotFound());

app.MapDelete("/cv-pdf/{userId}", async (AppDbContext db, int userId) =>
{
    try
    {
        await db.Database.ExecuteSqlRawAsync(
            "DELETE FROM CV_as_PDF WHERE cv_id = @p0", 
            userId);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting CV: {ex.Message}");
        return Results.Problem($"Error deleting CV: {ex.Message}");
    }
});

// Form data upload endpoint
app.MapPost("/cv-pdf/upload", async (HttpContext context, AppDbContext db) =>
{
    try
    {
        var form = await context.Request.ReadFormAsync();
        var file = form.Files.FirstOrDefault();
        var userIdString = form["userId"].ToString();

        if (file == null)
        {
            return Results.BadRequest("No file uploaded");
        }

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Results.BadRequest("Valid user ID is required");
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileData = ms.ToArray();

        var cvPdf = new CvPdf
        {
            cv_id = userId,
            file_name = file.FileName,
            file_size = fileData.Length,
            file_data = fileData
        };

        var existingCv = await db.CvPdf.FindAsync(userId);
        if (existingCv != null)
        {
            existingCv.file_name = cvPdf.file_name;
            existingCv.file_size = cvPdf.file_size;
            existingCv.file_data = cvPdf.file_data;
            db.CvPdf.Update(existingCv);
        }
        else
        {
            db.CvPdf.Add(cvPdf);
        }

        await db.SaveChangesAsync();
        return Results.Ok(new { message = "File uploaded successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing file upload: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem($"Error uploading file: {ex.Message}");
    }
})
.DisableAntiforgery()
.WithName("UploadCvForm")
.WithOpenApi();

// JSON upload endpoint (for Swagger testing)
app.MapPost("/cv-pdf/upload-json", async ([FromBody] CvPdfUploadRequest request, AppDbContext db) =>
{
    try
    {
        if (request == null)
        {
            return Results.BadRequest("Request data is required");
        }

        byte[] fileData;
        try
        {
            fileData = Convert.FromBase64String(request.file_data);
        }
        catch (FormatException)
        {
            return Results.BadRequest("Invalid file data format. File data must be Base64 encoded.");
        }

        var cvPdf = new CvPdf
        {
            cv_id = request.cv_id,
            file_name = request.file_name,
            file_size = fileData.Length,
            file_data = fileData
        };

        var existingCv = await db.CvPdf.FindAsync(request.cv_id);
        if (existingCv != null)
        {
            existingCv.file_name = cvPdf.file_name;
            existingCv.file_size = cvPdf.file_size;
            existingCv.file_data = cvPdf.file_data;
            db.CvPdf.Update(existingCv);
        }
        else
        {
            db.CvPdf.Add(cvPdf);
        }

        await db.SaveChangesAsync();
        return Results.Ok(new { message = "File uploaded successfully" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error uploading file: {ex.Message}");
        return Results.Problem($"Error uploading file: {ex.Message}");
    }
})
.WithName("UploadCvJson")
.WithOpenApi();

app.Run("http://0.0.0.0:7287");

class AppDbContext : DbContext
{
    public DbSet<SuperUser> SuperUser { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<Company> Company { get; set; }
    public DbSet<CvPdf> CvPdf { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SuperUser>()
            .HasKey(s => s.SuperUseremail);
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

        modelBuilder.Entity<CvPdf>()
            .HasKey(c => c.cv_id);
        modelBuilder.Entity<CvPdf>()
            .ToTable("CV_as_PDF");
        modelBuilder.Entity<CvPdf>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<CvPdf>(c => c.cv_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

class SuperUser
{
    [Key]
    public string SuperUseremail { get; set; }
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
    public string SuperUseremail { get; set; }
}

public class LoginRequest
{
    public string SuperUseremail { get; set; }
    public string Password { get; set; }
}


public class LoginRequestUser
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class CvPdf
{
    [Key]
    public int cv_id { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string file_name { get; set; }

    [Column(TypeName = "bigint")]
    public long file_size { get; set; }

    [Column(TypeName = "varbinary(max)")]
    public byte[] file_data { get; set; }
}

public class CvPdfUploadRequest
{
    [Required]
    public int cv_id { get; set; }

    [Required]
    [MaxLength(255)]
    public string file_name { get; set; }

    [Required]
    public string file_data { get; set; }
}