using System.Reflection;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UniversityApi.Common;
using UniversityApi.Data;
using UniversityApi.Mapping;
using UniversityApi.Middleware;
using UniversityApi.Models;
using UniversityApi.Repositories.Implementations;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Implementations;
using UniversityApi.Services.Interfaces;

const string CorsPolicyName = "ClientApps";

var builder = WebApplication.CreateBuilder(args);

// --- База данных (PostgreSQL) ---
// DbContext регистрируется как Scoped: один экземпляр на HTTP-запрос.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- JWT: настройки. Ключ подписи НЕ хранится в репозитории (user-secrets / переменная окружения Jwt__Key) ---
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key is missing or shorter than 32 bytes. Set it outside the repository: " +
        "dotnet user-secrets set \"Jwt:Key\" \"<random string, 32+ chars>\" (or environment variable Jwt__Key).");
}

// --- ASP.NET Core Identity (EF Core) ---
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        // Минимальные требования к паролю
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        // Уникальные имя пользователя и email
        options.User.RequireUniqueEmail = true;

        // Блокировка после нескольких неудачных попыток входа
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager();

// --- Authentication (JWT Bearer) ---
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // claims остаются с именами из токена (sub, role, ...)
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = AppClaims.Name,
            RoleClaimType = AppClaims.Role
        };

        options.Events = new JwtBearerEvents
        {
            // Нет токена / просрочен / подпись изменена / некорректный формат -> 401 в формате ReturnResult
            OnChallenge = async context =>
            {
                context.HandleResponse();
                await ApiErrorWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized,
                    ErrorCodes.Unauthorized, "Authentication is required or the token is invalid or expired");
            },
            // Пользователь аутентифицирован, но прав недостаточно -> 403
            OnForbidden = context => ApiErrorWriter.WriteAsync(context.HttpContext, StatusCodes.Status403Forbidden,
                ErrorCodes.Forbidden, "You do not have permission to access this resource"),
            // В лог попадает только тип ошибки, сам токен не логируется
            OnAuthenticationFailed = context =>
            {
                context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer")
                    .LogWarning("JWT rejected: {Reason}", context.Exception.GetType().Name);
                return Task.CompletedTask;
            }
        };
    });

// --- Authorization (роли + политики) ---
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(Roles.Admin))
    .AddPolicy(Policies.TeacherOrAdmin, policy => policy.RequireRole(Roles.Teacher, Roles.Admin));

// --- CORS: именованная политика для двух клиентских приложений (адреса из конфигурации) ---
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// --- AutoMapper --- (конфигурация маппинга регистрируется как Singleton: она неизменяема и потокобезопасна)
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// --- Repositories --- (Scoped: зависят от DbContext, поэтому живут столько же, сколько он)
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

// --- Services --- (Scoped: зависят от репозиториев)
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// --- Безопасность ---
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IResourceAccessService, ResourceAccessService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserAdminService, UserAdminService>();

// --- Controllers + единый формат ошибок валидации ---
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value is { Errors.Count: > 0 })
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors
                        .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "The value is invalid." : x.ErrorMessage)
                        .ToArray());

            var result = ReturnResult<object>.Fail(StatusCodes.Status400BadRequest, ErrorCodes.ValidationError,
                "One or more validation errors occurred", errors);
            result.TraceId = context.HttpContext.TraceIdentifier;

            return new ObjectResult(result) { StatusCode = StatusCodes.Status400BadRequest };
        };
    });

// --- API Versioning: /api/v1/..., /api/v2/...; запросы без версии (/api/courses) обслуживаются как v1 ---
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// --- Swagger / OpenAPI: документ на каждую версию + JWT Bearer ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Вставьте JWT (без слова Bearer), полученный из POST /api/auth/login",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

// Первым в конвейере: перехватывает любые необработанные исключения
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
            $"University API {description.GroupName}");
});

// Порядок важен: Routing -> CORS -> Authentication -> Authorization -> endpoints
app.UseRouting();
app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Создание схемы БД (миграции, если они есть; иначе EnsureCreated), роли и тестовые данные
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.GetMigrations().Any())
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();

    await DataSeeder.SeedAsync(db);
    await IdentitySeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}

app.Run();
