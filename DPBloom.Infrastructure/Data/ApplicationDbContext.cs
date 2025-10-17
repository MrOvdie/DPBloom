using DPBloom.Infrastructure.Course;
using DPBloom.Infrastructure.Exam;
using DPBloom.Infrastructure.Lecture;
using DPBloom.Infrastructure.Topic;
using DPBloom.Infrastructure.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { } 
    
    public DbSet<LectureDao> Lectures { get; set; }
    public DbSet<TopicDao> Topics { get; set; }
    public DbSet<CourseDao> Courses { get; set; }
    
    public DbSet<UserExamAttemptDao> UserExamAttempts { get; set; }
    public DbSet<QuestionResultDao> QuestionResults { get; set; }
    public DbSet<UserAnswerDao> UserAnswers { get; set; }
    public DbSet<QuestionDao> Questions { get; set; }
    public DbSet<AnswerOptionDao> AnswerOptions { get; set; }
    public DbSet<ExamDao> Exams { get; set; }
    public DbSet<QuestionResultDao> AnswerReviews { get; set; }
    public DbSet<AttemptResultDao> AttemptResults { get; set; }
}