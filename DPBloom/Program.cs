using AutoMapper;
using DPBloom.Application.Course;
using DPBloom.Application.Exam;
using DPBloom.Application.Lecture;
using DPBloom.Application.Topic;
using DPBloom.Infrastructure.Course;
using DPBloom.Infrastructure.Data;
using DPBloom.Infrastructure.Exam;
using DPBloom.Infrastructure.Lecture;
using DPBloom.Infrastructure.Topic;
using DPBloom.Infrastructure.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LaptopConnection")));

//TODO: add validation

builder.Services.AddScoped<IdentityUser, ApplicationUser>();

builder.Services.AddAutoMapper(config =>
    config.AddProfiles(new List<Profile>
    {
        new LectureDaoProfile(), new CourseDaoProfile(), new TopicDaoProfile(),
        new LectureDtoProfile(), new TopicDtoProfile(), new CourseDtoProfile()
    }));

builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILectureRepository, LectureRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireTeacherPrivileges", policy => policy.RequireRole("Admin", "Teacher"));

    options.AddPolicy("CanManageSystemData", policy => policy.RequireClaim("Permission", "ManageSystemData"));
});


builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("My API")
            .WithTheme(ScalarTheme.Mars)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .WithDarkMode();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapIdentityApi<ApplicationUser>(); //adds basic endpoints for using Identity for user 

app.Run();