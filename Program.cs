using System.Text;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using DotNetEnv;
using echart_dentnu_api.Services;
using echart_dentnu_api.Database;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Grpc.Core;

// Load .env file only in Development environment
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    Console.WriteLine("--- Loading .env file (Development) ---");
    Env.Load();
    Console.WriteLine("--- .env file loaded successfully ---");
}

// --- Environment & Configuration ---

var builder = WebApplication.CreateBuilder(args);
var environmentName = builder.Environment.EnvironmentName;

Console.WriteLine($"Current Environment: {environmentName}");
Console.WriteLine($"JWT_SECRET: {builder.Configuration["JWT_SECRET"]}");
Console.WriteLine($"JWT_ISSUER: {builder.Configuration["JWT_ISSUER"]}");
Console.WriteLine($"JWT_AUDIENCE: {builder.Configuration["JWT_AUDIENCE"]}");

// --- Service Collection ---

// 1. Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? builder.Configuration.GetValue<string>("DB_CONNECTION_STRING");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string ('DefaultConnection' or 'DB_CONNECTION_STRING') is missing.");
    }
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// 2. Authentication & JWT
var jwtSecret = builder.Configuration["JWT_SECRET"];
var jwtIssuer = builder.Configuration["JWT_ISSUER"];
var jwtAudience = builder.Configuration["JWT_AUDIENCE"];

if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration is incomplete. Ensure JWT_SECRET, JWT_ISSUER, and JWT_AUDIENCE are set.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };

    Console.WriteLine($"JWT Secret Length: {jwtSecret.Length}");

    var jwtSecretDebug = builder.Configuration["JWT_SECRET"];
    Console.WriteLine($"JWT_SECRET (Raw): '{jwtSecretDebug}'");
    Console.WriteLine($"JWT_SECRET Length (Raw): {jwtSecretDebug?.Length ?? 0}");
    if (!string.IsNullOrEmpty(jwtSecretDebug))
    {
        var bytes = Encoding.UTF8.GetBytes(jwtSecretDebug);
        Console.WriteLine($"JWT_SECRET Bytes Length: {bytes.Length}");
        // Optional: Print some portion of the bytes if you suspect issues
        // Console.WriteLine($"JWT_SECRET First 10 Bytes: {BitConverter.ToString(bytes.Take(10).ToArray())}");
    }

    // Optional: Add events for debugging JWT authentication
    // แทนที่ JWT Bearer Events ใน Program.cs
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"[JWT] Raw Authorization Header: '{token}'");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roles = string.Join(", ", identity.FindAll(ClaimTypes.Role).Select(c => c.Value));
                Console.WriteLine($"[JWT Auth] Success: User '{userId}' with Roles '{roles}' validated.");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[JWT Auth] Failed: {context.Exception.Message}");
            Console.WriteLine($"[JWT Auth] Exception Type: {context.Exception.GetType().Name}");

            // ถ้าเป็น SecurityTokenMalformedException แสดงว่า token format ผิด
            if (context.Exception is SecurityTokenMalformedException)
            {
                Console.WriteLine("[JWT Auth] Token format is malformed - check if it has 3 parts separated by dots");
            }

            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"[JWT Auth] Challenge: {context.Error} - {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

// 3. Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("MedicalStaff", policy => policy.RequireRole("อาจารย์", "ปริญญาตรี", "ปริญญาโท", "หัวหน้าผู้ช่วยทันตแพทย์", "ผู้ช่วยทันตแพทย์"));
    // Add other policies as needed...
});

// 4. API Services & Swagger
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "e-Chart Dentistry API", Version = "v1" });

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token in the text input below."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOrigin", corsBuilder =>
    {
        var allowedOrigins = builder.Configuration["CORS_ALLOWED_ORIGINS"]?.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            Console.WriteLine($"CORS Allowed Origins: {string.Join(", ", allowedOrigins)}"); // Add this line for debugging
            corsBuilder.WithOrigins(allowedOrigins)
                       .AllowAnyHeader()
                       .AllowAnyMethod();
        }
        else // Fallback for development
        {
            corsBuilder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod();
        }
    });
});

// 6. Health Checks
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

// 7. Custom Application Services
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// 8. API limiter
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    // 1. for GET
    rateLimiterOptions.AddFixedWindowLimiter("readLimiter", options =>
    {
        options.PermitLimit = 50; // ยิง request ได้ 50 ครั้ง
        options.Window = TimeSpan.FromSeconds(30); // ใน 30 วินาที
        options.QueueLimit = 100; // ถ้ายิงเกิน 50 ครั้ง ก็จะให้รอในคิวได้อีก 100 ครั้ง
        /*  เช่น มี 150 คนยิง request พร้อมกันในช่วง 30 วินาที
            - รับ 50 request แรก -> ผ่านเลย
            - 100 request ถัดไป -> รออยู่ในคิว
            - เกิน 150 request -> ปฏิเสธทันที (429 Too Many Requests) */
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // คิวจะจัดลำดับแบบมาก่อน-ออกก่อน
    });

    // 2. for POST,PATCH,DELETE
    rateLimiterOptions.AddFixedWindowLimiter("writeLimiter", options =>
    {
        options.PermitLimit = 3; 
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueLimit = 2;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // 3. for login
    rateLimiterOptions.AddFixedWindowLimiter("loginLimiter", options =>
    {
        options.PermitLimit = 5; // ยิง request ได้ 5 ครั้ง
        options.Window = TimeSpan.FromMinutes(1); // ใน 1 นาที
        options.QueueLimit = 0; // ถ้ายิงเกิน 5 ครั้ง จะปฏิเสธทันที
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});


// --- HTTP Request Pipeline ---

var app = builder.Build();

// Verify database connection on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await dbContext.Database.CanConnectAsync();
        Console.WriteLine("✅ Database connection successful!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database connection failed: {ex.Message}");
    }
}

// Configure pipeline for Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "e-Chart API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRateLimiter(); // API limiter

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    await next();
});

// The order of middleware is critical
app.UseCors("AllowFrontendOrigin");

app.UseAuthentication(); // Checks for valid token
app.Use(async (context, next) =>
{
    // Debug: ดู Authorization header
    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        Console.WriteLine($"[DEBUG] Authorization Header: '{authHeader}'");

        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader.Substring(7); // Remove "Bearer "
            Console.WriteLine($"[DEBUG] Token: '{token}'");
            Console.WriteLine($"[DEBUG] Token Length: {token.Length}");
            Console.WriteLine($"[DEBUG] Token Dots Count: {token.Count(c => c == '.')}");
        }
    }
    else
    {
        Console.WriteLine("[DEBUG] No Authorization header found");
    }

    await next();
});

app.UseAuthorization();  // Checks if the user has the required role/policy

app.MapControllers();
app.MapHealthChecks("/health");

Console.WriteLine("🚀 Application starting...");
app.Run();