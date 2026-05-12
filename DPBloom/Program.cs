using System.Security.Claims;
using System.Text;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using DPBloom.Application.ANN;
using DPBloom.Application.Auth;
using DPBloom.Application.Bloom;
using DPBloom.Application.Course;
using DPBloom.Application.Course.Validators;
using DPBloom.Application.Enrollment;
using DPBloom.Application.Exam;
using DPBloom.Application.Lecture;
using DPBloom.Application.Topic;
using DPBloom.Application.User;
using DPBloom.Auth;
using DPBloom.Infrastructure.ANN;
using DPBloom.Infrastructure.Auth;
using DPBloom.Infrastructure.Bloom;
using DPBloom.Infrastructure.Course;
using DPBloom.Infrastructure.Data;
using DPBloom.Infrastructure.Exam;
using DPBloom.Infrastructure.Lecture;
using DPBloom.Infrastructure.Topic;
using DPBloom.Infrastructure.User;
using DPBloom.Middleware.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using IEnrollmentRepository = DPBloom.Application.User.IEnrollmentRepository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(CourseService).Assembly); });

// 1. Налаштування бази даних
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DesktopConnection"))); //"LaptopConnection"

// 2. Налаштування Identity (Без стандартних API ендпоінтів)
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;

        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3. Реєстрація AutoMapper
builder.Services.AddAutoMapper(config =>
{
    config.AddExpressionMapping();
    config.AddProfiles(new List<Profile>
    {
        new LectureDaoProfile(), new CourseDaoProfile(), new TopicDaoProfile(), new ExamDaoProfile(), new UserProfile(),
        new EnrollmentDaoProfile(),
        new LectureDtoProfile(), new TopicDtoProfile(), new CourseDtoProfile(), new ExamDtoProfile(),
        new UserDtoProfile()
    });
});

builder.Services.AddHttpContextAccessor();

// Цей рядок автоматично знайде всі класи-валідатори у тій же збірці, де знаходиться CreateCourseValidator
builder.Services.AddValidatorsFromAssemblyContaining<CreateCourseValidator>();

// 5. Реєстрація репозиторіїв
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILectureRepository, LectureRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<IAttemptRepository, AttemptRepository>();
builder.Services.AddScoped<IAttemptResultRepository, AttemptResultRepository>();
builder.Services.AddScoped<IBloomRepository, BloomRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

// 6. Реєстрація бізнес-сервісів
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<ILectureService, LectureService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IBloomService, BloomService>();
builder.Services.AddScoped<IAttemptService, AttemptService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<IBloomLevelPredictor, OnnxBloomPredictor>();

// 7. Налаштування JWT та авторизації
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is missing");
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
            ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
            ValidAudience = builder.Configuration["Jwt:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireTeacherPrivileges", policy => policy.RequireRole("Admin", "Teacher"));
    options.AddPolicy("CanManageSystemData", policy => policy.RequireClaim("Permission", "ManageSystemData"));
}); //TODO: check, if smth else is required

// 8. Налаштування OpenAPI з підтримкою Bearer Token
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter JWT token, given after logging in."
        });

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("DPBloom API")
            .WithTheme(ScalarTheme.Mars)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .WithDarkMode();
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();