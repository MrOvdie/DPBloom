using System.Text.Json;
using DPBloom.Core.Exam;
using DPBloom.Infrastructure.Bloom;
using DPBloom.Infrastructure.Course;
using DPBloom.Infrastructure.Exam;
using DPBloom.Infrastructure.Lecture;
using DPBloom.Infrastructure.Topic;
using DPBloom.Infrastructure.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<LectureDao> Lectures { get; set; }
    public DbSet<TopicDao> Topics { get; set; }
    public DbSet<CourseDao> Courses { get; set; }
    public DbSet<UserEnrollmentDao> UserEnrollments { get; set; }

    public DbSet<UserExamAttemptDao> UserExamAttempts { get; set; }
    public DbSet<UserQuestionAnswerDao> UserAnswers { get; set; }
    public DbSet<QuestionDao> Questions { get; set; }
    public DbSet<AnswerOptionDao> AnswerOptions { get; set; }

    public DbSet<ExamDao> Exams { get; set; }
    public DbSet<QuestionResultDao> QuestionResults { get; set; }
    public DbSet<AttemptResultDao> AttemptResults { get; set; }

    public DbSet<BloomAnalysisDao> BloomAnalyses { get; set; }
    public DbSet<BloomLevelPerformanceDao> BloomLevelPerformances { get; set; }
    public DbSet<RecommendedMaterialDao> RecommendedMaterials { get; set; }
    public DbSet<RecommendationTemplateDao> RecommendationTemplates { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CourseDao>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<TopicDao>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<LectureDao>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<ExamDao>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<QuestionDao>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<AnswerOptionDao>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<BloomAnalysisDao>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<UserExamAttemptDao>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<UserEnrollmentDao>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<RecommendationTemplateDao>().HasQueryFilter(x => !x.IsDeleted);


        builder.Entity<CourseDao>()
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<CourseDao>()
            .HasOne(c => c.LastUpdater)
            .WithMany()
            .HasForeignKey(c => c.LastUpdaterId)
            .OnDelete(DeleteBehavior.Restrict); 
        
        builder.Entity<CourseDao>()
            .HasIndex(e => e.Title)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");


        builder.Entity<TopicDao>()
            .HasOne(t => t.Author)
            .WithMany()
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.Restrict); 
        
        builder.Entity<TopicDao>()
            .HasIndex(e => new { e.CourseId, e.Title })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");


        builder.Entity<ExamDao>()
            .HasOne(e => e.Author)
            .WithMany()
            .HasForeignKey(e => e.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ExamDao>()
            .HasIndex(e => new { e.CourseId, e.Title, e.TopicId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");


        builder.Entity<LectureDao>()
            .HasOne(l => l.Author)
            .WithMany()
            .HasForeignKey(l => l.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<LectureDao>()
            .HasIndex(e => new { e.CourseId, e.Title, e.TopicId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");


        builder.Entity<AttemptResultDao>()
            .HasOne(ar => ar.Exam)
            .WithMany()
            .HasForeignKey(ar => ar.ExamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AttemptResultDao>()
            .HasOne(ar => ar.User)
            .WithMany()
            .HasForeignKey(ar => ar.UserId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Entity<UserQuestionAnswerDao>()
            .HasOne(ua => ua.Question)
            .WithMany()
            .HasForeignKey(ua => ua.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserQuestionAnswerDao>()
            .HasOne(ua => ua.Attempt)
            .WithMany()
            .HasForeignKey(ua => ua.AttemptId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<UserQuestionAnswerDao>()
            .HasOne(a => a.Attempt)
            .WithMany(e => e.Answers)
            .HasForeignKey(a => a.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<UserQuestionAnswerDao>()
            .Property(x => x.SelectedOptionIds)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions)null)
            );


        builder.Entity<UserExamAttemptDao>()
            .HasOne(a => a.AttemptResult)
            .WithOne(ar => ar.Attempt)
            .HasForeignKey<AttemptResultDao>(ar => ar.AttemptId);
        
        builder.Entity<UserExamAttemptDao>()
            .HasOne(a => a.Course) 
            .WithMany()            
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Entity<QuestionResultDao>()
            .HasOne(qr => qr.Question)
            .WithMany()
            .HasForeignKey(qr => qr.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuestionResultDao>()
            .HasOne(qr => qr.Attempt)
            .WithMany()
            .HasForeignKey(qr => qr.AttemptId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuestionResultDao>()
            .HasOne(qr => qr.AttemptResult)
            .WithMany()
            .HasForeignKey(qr => qr.AttemptResultId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Entity<UserEnrollmentDao>()
            .HasOne(ar => ar.User)
            .WithMany()
            .HasForeignKey(ar => ar.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserEnrollmentDao>()
            .HasOne(ar => ar.Course)
            .WithMany()
            .HasForeignKey(ar => ar.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserEnrollmentDao>()
            .HasIndex(e => new { e.UserId, e.CourseId })
            .IsUnique();
        
        
        builder.Entity<AttemptResultDao>() 
            .HasOne(ar => ar.Attempt)
            .WithOne(a => a.AttemptResult) 
            .HasForeignKey<AttemptResultDao>(ar => ar.AttemptId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        builder.Entity<RecommendationTemplateDao>() 
            .HasOne(ar => ar.Course)
            .WithMany()
            .HasForeignKey(ar => ar.CourseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}