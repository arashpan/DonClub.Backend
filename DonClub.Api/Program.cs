//using System.Text;
//using Donclub.Infrastructure.DependencyInjection;
//using Donclub.Infrastructure.Persistence;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Donclub.Api.Middleware;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// DbContext
//builder.Services.AddDbContext<DonclubDbContext>(opt =>
//    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

//// Infrastructure services
//builder.Services.AddInfrastructureServices();

//// JWT
//var jwtSection = builder.Configuration.GetSection("Jwt");
//var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"]!);
//var signingKey = new SymmetricSecurityKey(keyBytes);

//builder.Services
//    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new()
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidIssuer = jwtSection["Issuer"],
//            ValidAudience = jwtSection["Audience"],
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = signingKey,
//            ValidateLifetime = true,
//            ClockSkew = TimeSpan.FromMinutes(2)
//        };
//    });

//builder.Services.AddAuthorization();

//var app = builder.Build();

//// 🔹 این باید خیلی زود در pipeline صدا زده بشه
//app.UseGlobalErrorHandling();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();

//app.Run();

using Donclub.Api.Middleware;
using Donclub.Infrastructure.DependencyInjection;
using Donclub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	const string schemeId = "bearer";

	options.AddSecurityDefinition(schemeId, new OpenApiSecurityScheme
	{
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Description = "JWT Authorization header using the Bearer scheme."
	});

	options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
	{
		[new OpenApiSecuritySchemeReference(schemeId, document)] = []
	});
});


// DbContext
builder.Services.AddDbContext<DonclubDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Infrastructure DI
builder.Services.AddInfrastructureServices();

// JWT Auth
var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// CORS (Config-driven)
builder.Services.AddCors(options =>
{
    var cors = builder.Configuration.GetSection("Cors");
    var origins = cors.GetSection("Origins").Get<string[]>() ?? Array.Empty<string>();
    var allowAnyOrigin = cors.GetValue<bool?>("AllowAnyOrigin") ?? false;
    var allowCredentials = cors.GetValue<bool?>("AllowCredentials") ?? false;
    var preflightSeconds = cors.GetValue<int?>("PreflightMaxAgeSeconds") ?? 600;

    options.AddPolicy("Frontend", policy =>
    {
        if (allowAnyOrigin && !allowCredentials)
        {
            // برای توسعه: بدون کوکی/کرِدِنشال → امن‌ترین گزینه
            policy.AllowAnyOrigin();
        }
        else if (origins.Length > 0)
        {
            policy.WithOrigins(origins);
            if (allowCredentials)
                policy.AllowCredentials();
        }
        else
        {
            // پیش‌فرض امن برای توسعه لوکال
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200");
        }

        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetPreflightMaxAge(TimeSpan.FromSeconds(preflightSeconds));
        // اگر نیاز به هدرهای خاص سمت فرانت داری:
        // policy.WithExposedHeaders("X-Total-Count");
    });

    // اختیاری: اجازه به زیردامنه‌ها در پروکسی/CDN (بدون Credentials)
    options.AddPolicy("WildcardSubdomains", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                return uri.Host.EndsWith(".donclub.com", StringComparison.OrdinalIgnoreCase);
            return false;
        })
        .AllowAnyHeader().AllowAnyMethod();
        // توجه: با Credentials سازگار نیست.
    });
});

// ForwardedHeaders برای سناریوهای Proxy/Container
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // در صورت نیاز KnownProxies/KnownNetworks را از تنظیمات بخوان:
    // options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
});

var app = builder.Build();

// ---------- Middleware pipeline ----------

// HSTS فقط در Production
if (app.Environment.IsProduction())
{
    app.UseHsts();
}

// Swagger (با فلگ config در همه محیط‌ها قابل‌روشن شدن است)
var swaggerEnable = app.Configuration.GetValue<bool?>("Swagger:Enable") ?? app.Environment.IsDevelopment();
if (swaggerEnable)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DonClub API v1");
        c.RoutePrefix = "swagger";
    });
}

// خطای سراسری (قبلاً داشتید—همان اول اجرا شود)
app.UseGlobalErrorHandling(); // middleware سفارشی شما

app.UseHttpsRedirection();

// Forwarded Headers (قبل از احراز هویت)
app.UseForwardedHeaders();

// CORS باید قبل از Auth/Authorization اجرا شود
app.UseCors("Frontend"); // یا "WildcardSubdomains" بسته به نیاز دامنه‌ها

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

