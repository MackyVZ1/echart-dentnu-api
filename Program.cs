using Microsoft.EntityFrameworkCore;
using DotNetEnv; // Make sure you have this package installed: dotnet add package DotNetEnv
using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using echart_dentnu_api.Services; // Assuming this namespace is correct for your IJwtTokenGenerator and JwtTokenGenerator

var currentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (string.Equals(currentEnvironment, "Development", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("--- Loading .env file (Early Load) ---");
    Env.Load(); // Loads environment variables from .env file
    Console.WriteLine("--- .env file loaded for Development environment. ---");
}
else
{
    Console.WriteLine($"Current ASPNETCORE_ENVIRONMENT (Early Check): {currentEnvironment ?? "Not Set"}");
    Console.WriteLine("--- Not loading .env file as environment is not Development (Early Load). ---");
}

var builder = WebApplication.CreateBuilder(args);
// --- START: Database Configuration ---
builder.Services.AddDbContext<Database>(options =>
{
    // Prioritize Environment Variable (for Docker/Render)
    // Fallback to GetConnectionString("DefaultConnection") from appsettings.json if not found
    var connectionString = builder.Configuration.GetValue<string>("DB_CONNECTION_STRING")
                           ?? builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine("❌ Error: DB_CONNECTION_STRING or DefaultConnection in appsettings.json is not set.");
        // Consider throwing an exception or exiting the application if DB connection is critical
        // throw new InvalidOperationException("Database connection string is not configured.");
    }
    else
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        Console.WriteLine("✅ Database context configured.");
    }
});
// --- END: Database Configuration ---


// --- START: JWT Configuration ---
// Access JWT settings using builder.Configuration, which reads from environment variables
// (loaded by Env.Load() in dev, or directly from Render dashboard in production)
var jwtSecret = builder.Configuration["JWT_SECRET"];
var jwtIssuer = builder.Configuration["JWT_ISSUER"];
var jwtAudience = builder.Configuration["JWT_AUDIENCE"];
// Use int.TryParse for safer conversion, with a default value if not found or invalid
var jwtExpireMinutes = int.TryParse(builder.Configuration["JWT_TOKEN_EXPIRE_MINUTES"], out int minutes) ? minutes : 180;

if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT_SECRET, JWT_ISSUER, and JWT_AUDIENCE must be set as environment variables or in appsettings.json.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
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
        ClockSkew = TimeSpan.Zero // เพื่อไม่ให้ token หมดอายุก่อนเวลาจริง
    };
});

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
// --- END: JWT Configuration ---


// --- START: Authorization Policies ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim(ClaimTypes.Role, "Administrator"));
    options.AddPolicy("AppointmentOnly", policy => policy.RequireClaim(ClaimTypes.Role, "ระบบนัดหมาย"));
    options.AddPolicy("FinancialOnly", policy => policy.RequireClaim(ClaimTypes.Role, "การเงิน"));
    options.AddPolicy("MedicalrecordOnly", policy => policy.RequireClaim(ClaimTypes.Role, "เวชระเบียน"));
    options.AddPolicy("TeacherOnly", policy => policy.RequireClaim(ClaimTypes.Role, "อาจารย์"));
    options.AddPolicy("BachelorOnly", policy => policy.RequireClaim(ClaimTypes.Role, "ปริญญาตรี"));
    options.AddPolicy("DrugOnly", policy => policy.RequireClaim(ClaimTypes.Role, "ระบบยา"));
    options.AddPolicy("GeneralOnly", policy => policy.RequireClaim(ClaimTypes.Role, "ผู้ใช้งานทั่วไป"));
    options.AddPolicy("MasterOnly", policy => policy.RequireClaim(ClaimTypes.Role, "ปริญญาโท"));
    options.AddPolicy("RequirementDiagOnly", policy => policy.RequireClaim(ClaimTypes.Role, "RequirementDiag"));
    options.AddPolicy("HeadAssistantDentistOnly", policy => policy.RequireClaim(ClaimTypes.Role, "หัวหน้าผู้ช่วยทันตแพทย์"));
    options.AddPolicy("AssistantDentistOnly", policy => policy.RequireClaim(ClaimTypes.Role, "ผู้ช่วยทันตแพทย์"));
});
// --- END: Authorization Policies ---


// --- START: CORS Policy ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOrigin",
        corsBuilder =>
        {
            // Get CORS origins from configuration (e.g., appsettings.json or environment variable)
            // Example: "CORS_ALLOWED_ORIGINS": "http://localhost:5173,https://your-frontend-on-render.com"
            var allowedOrigins = builder.Configuration["CORS_ALLOWED_ORIGINS"]?.Split(';', StringSplitOptions.RemoveEmptyEntries);

            if (allowedOrigins != null && allowedOrigins.Length > 0)
            {
                corsBuilder.WithOrigins(allowedOrigins)
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
                Console.WriteLine($"✅ CORS policy configured for origins: {string.Join(", ", allowedOrigins)}");
            }
            else
            {
                // Fallback for development or if no specific origins are set
                // Be cautious with AllowAnyOrigin() in production
                corsBuilder.AllowAnyOrigin() // This allows all origins, use with caution in production
                       .AllowAnyHeader()
                       .AllowAnyMethod();
                Console.WriteLine("⚠️ CORS policy configured to allow any origin. Please set CORS_ALLOWED_ORIGINS in production.");
            }
        });
});
// --- END: CORS Policy ---


// --- START: Other Services ---
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "e-Chart's Dentistry Hospital",
        Version = "v1",
        Description = "API สำหรับโปรเจค e-Chart ระบบทันตกรรม",
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    else
    {
        Console.WriteLine($"⚠️ Swagger XML Comments file not found at: {xmlPath}");
    }

    // Add JWT Authentication to Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {securityScheme, new string[] { }}
    });
});
// --- END: Other Services ---


var app = builder.Build();

// --- START: Database Connection Check (Optional but Recommended) ---
// This block ensures the database is created/migrated and logs connection status
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Database>();
    try
    {
        // For migrations, you might use context.Database.MigrateAsync();
        // EnsureCreatedAsync() is good for development or simple scenarios.
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("✅ เชื่อมต่อกับฐานข้อมูล MySQL สำเร็จ!");
        Console.WriteLine($"📊 ฐานข้อมูล: {context.Database.GetDbConnection().Database}");
        Console.WriteLine($"🔗 Server: {context.Database.GetDbConnection().DataSource}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ ไม่สามารถเชื่อมต่อฐานข้อมูลได้:");
        Console.WriteLine($"🔴 Error: {ex.Message}");
        Console.WriteLine($"🔴 Inner Exception: {ex.InnerException?.Message}"); // Log inner exception for more details
        Console.WriteLine("🔧 กรุณาตรวจสอบ connection string และการตั้งค่า MySQL");
        // Optionally re-throw or exit if DB connection is critical for app startup
        // throw;
    }
}
// --- END: Database Connection Check ---


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use the CORS policy you defined
app.UseCors("AllowFrontendOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();