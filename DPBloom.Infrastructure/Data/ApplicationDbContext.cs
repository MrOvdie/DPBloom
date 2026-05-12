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
    public DbSet<UserAnswerDao> UserAnswers { get; set; }
    public DbSet<QuestionDao> Questions { get; set; }
    public DbSet<AnswerOptionDao> AnswerOptions { get; set; }

    public DbSet<ExamDao> Exams { get; set; }
    public DbSet<QuestionResultDao> QuestionResults { get; set; }
    public DbSet<AttemptResultDao> AttemptResults { get; set; }

    public DbSet<BloomAnalysisDao> BloomAnalyses { get; set; }
    public DbSet<BloomLevelPerformanceDao> BloomLevelPerformances { get; set; }
    public DbSet<RecommendedMaterialDao> RecommendedMaterials { get; set; }

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


        builder.Entity<CourseDao>()
            .HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Entity<TopicDao>()
            .HasOne(t => t.Author)
            .WithMany()
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Entity<ExamDao>()
            .HasOne(e => e.Author)
            .WithMany()
            .HasForeignKey(e => e.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Entity<LectureDao>()
            .HasOne(l => l.Author)
            .WithMany()
            .HasForeignKey(l => l.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);


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


        builder.Entity<UserAnswerDao>()
            .HasOne(ua => ua.Question)
            .WithMany()
            .HasForeignKey(ua => ua.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserAnswerDao>()
            .HasOne(ua => ua.Attempt)
            .WithMany()
            .HasForeignKey(ua => ua.AttemptId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Entity<UserExamAttemptDao>()
            .HasOne(a => a.AttemptResult)
            .WithOne(ar => ar.Attempt)
            .HasForeignKey<AttemptResultDao>(ar => ar.AttemptId);


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
    }
}