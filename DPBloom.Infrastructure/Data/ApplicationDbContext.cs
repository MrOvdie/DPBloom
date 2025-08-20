using DPBloom.Core;
using DPBloom.Infrastructure.Lecture;
using DPBloom.Infrastructure.Topic;
using DPBloom.Infrastructure.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DPBloom.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { } 
    
    public DbSet<LectureDao> Lectures { get; set; }
    public DbSet<TopicDao> Topics { get; set; }
    
}