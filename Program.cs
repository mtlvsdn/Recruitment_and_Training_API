using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.IO;

public class CvPdf
{
    [Key]
    public int cv_id { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(255)")]
    public string file_name { get; set; }

    [Required]
    [Column(TypeName = "bigint")]
    public long file_size { get; set; }

    [Required]
    [Column(TypeName = "varbinary(max)")]
    public byte[] file_data { get; set; }
}

// Add this DTO for Swagger/JSON requests
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JSON options to handle Base64 strings
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

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

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/cv-pdf/{userId}", async (AppDbContext db, int userId) =>
    await db.CvPdf.FindAsync(userId) is CvPdf cv ? Results.Ok(cv) : Results.NotFound());

// Form data upload endpoint
app.MapPost("/cv-pdf/upload", async (HttpContext context, AppDbContext db) =>
{
    try
    {
        // Check content type
        var contentType = context.Request.ContentType;
        Console.WriteLine($"Received request with Content-Type: {contentType}");
        
        if (string.IsNullOrEmpty(contentType) || !contentType.Contains("multipart/form-data"))
        {
            Console.WriteLine("Invalid content type for form data upload");
            return Results.BadRequest($"Invalid Content-Type: {contentType}. Must be multipart/form-data");
        }

        var form = await context.Request.ReadFormAsync();
        Console.WriteLine($"Form data contains {form.Files?.Count ?? 0} files and {form.Count} form fields");
        
        var file = form.Files.FirstOrDefault();
        if (file == null)
        {
            Console.WriteLine("No file received in request");
            return Results.BadRequest("No file uploaded. Make sure to include a file in the 'file' field");
        }
        
        var userIdString = form["userId"].ToString();
        Console.WriteLine($"UserId from form: '{userIdString}'");

        if (string.IsNullOrEmpty(userIdString))
        {
            Console.WriteLine("No userId received in request");
            return Results.BadRequest("User ID is required. Make sure to include 'userId' in the form data");
        }

        if (!int.TryParse(userIdString, out int userId))
        {
            Console.WriteLine($"Invalid userId format: {userIdString}");
            return Results.BadRequest($"Invalid user ID format: {userIdString}. Must be a number");
        }

        Console.WriteLine($"Uploading file '{file.FileName}' ({file.Length} bytes) for user {userId}");

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
            Console.WriteLine($"Updated existing CV for user {userId}");
        }
        else
        {
            db.CvPdf.Add(cvPdf);
            Console.WriteLine($"Added new CV for user {userId}");
        }

        await db.SaveChangesAsync();
        Console.WriteLine("CV saved successfully");
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

// JSON upload endpoint
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
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem($"Error uploading file: {ex.Message}");
    }
})
.WithName("UploadCvJson")
.WithOpenApi();

// Simple upload endpoint - supports both JSON and form data
app.MapPost("/cv-pdf/simple-upload", async (HttpContext context, AppDbContext db) =>
{
    try
    {
        int userId = 0;
        string fileName = "";
        byte[] fileData = null;
        
        // Log all request info for debugging
        Console.WriteLine($"Request method: {context.Request.Method}");
        Console.WriteLine($"Content-Type: {context.Request.ContentType}");
        Console.WriteLine($"Content-Length: {context.Request.ContentLength}");
        
        foreach (var header in context.Request.Headers)
        {
            Console.WriteLine($"Header: {header.Key} = {header.Value}");
        }
        
        // Handle form data
        if (context.Request.HasFormContentType)
        {
            var form = await context.Request.ReadFormAsync();
            Console.WriteLine($"Form data received with {form.Files?.Count ?? 0} files and {form.Count} fields");
            
            // Get userId from form
            if (form.ContainsKey("userId") && int.TryParse(form["userId"], out userId))
            {
                Console.WriteLine($"UserId from form: {userId}");
            }
            else
            {
                Console.WriteLine("No valid userId in form data, using default");
                // For testing, use user ID 12 if not provided
                userId = 12;
            }
            
            // Get file
            var file = form.Files.FirstOrDefault();
            if (file != null)
            {
                fileName = file.FileName;
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                fileData = ms.ToArray();
                Console.WriteLine($"File received: {fileName}, {fileData.Length} bytes");
            }
            else
            {
                return Results.BadRequest("No file in form data");
            }
        }
        // Handle JSON
        else if (context.Request.ContentType?.Contains("application/json") == true)
        {
            var request = await context.Request.ReadFromJsonAsync<CvPdfUploadRequest>();
            if (request == null)
            {
                return Results.BadRequest("Invalid JSON format");
            }
            
            userId = request.cv_id;
            fileName = request.file_name;
            
            try 
            {
                fileData = Convert.FromBase64String(request.file_data);
                Console.WriteLine($"Base64 data decoded: {fileData.Length} bytes");
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Invalid Base64 data: {ex.Message}");
            }
        }
        // Handle any other content type
        else
        {
            // For testing, use a default user ID
            userId = 12;
            fileName = "test.pdf";
            
            // Read raw request body
            using var ms = new MemoryStream();
            await context.Request.Body.CopyToAsync(ms);
            fileData = ms.ToArray();
            Console.WriteLine($"Raw data received: {fileData.Length} bytes");
        }
        
        // Validate we have what we need
        if (userId <= 0)
        {
            return Results.BadRequest("Valid user ID is required");
        }
        
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = $"upload_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        }
        
        if (fileData == null || fileData.Length == 0)
        {
            return Results.BadRequest("No file data received");
        }
        
        // Save to database
        var cvPdf = new CvPdf
        {
            cv_id = userId,
            file_name = fileName,
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
            Console.WriteLine($"Updated existing CV for user {userId}");
        }
        else
        {
            db.CvPdf.Add(cvPdf);
            Console.WriteLine($"Added new CV for user {userId}");
        }
        
        await db.SaveChangesAsync();
        Console.WriteLine("CV saved successfully");
        return Results.Ok(new { message = "File uploaded successfully", userId, fileName, fileSize = fileData.Length });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in simple upload: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        return Results.Problem($"Upload error: {ex.Message}");
    }
})
.DisableAntiforgery()
.WithName("SimpleUpload")
.WithOpenApi();

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

// Add a simple test endpoint
app.MapGet("/test", () => "API is running");

app.Run("http://0.0.0.0:7287");