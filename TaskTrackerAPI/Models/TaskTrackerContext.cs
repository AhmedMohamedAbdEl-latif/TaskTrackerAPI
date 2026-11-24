using Microsoft.EntityFrameworkCore;


namespace TaskTrackerAPI.Models
{
    public class TaskTrackerContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> Tasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<LoginAttemptOperation> LoginAttemptOperations { get; set; }
        public TaskTrackerContext(DbContextOptions<TaskTrackerContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProjectTask>()
                .ToTable("Tasks", table => table.HasCheckConstraint("CK_Tasks_Status", "[Status] IN (1, 2, 3)"));

            modelBuilder.Entity<User>()
                .ToTable("Users", table => table.HasCheckConstraint("CK_Users_Role", "[Role] IN (1, 2)"));

            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.Project)
                .WithMany()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.AssignedUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<LoginAttempt>()
                .HasIndex(la => la.EmailHash)
                .IsUnique();

            modelBuilder.Entity<LoginAttempt>()
                .HasIndex(la => la.LastFailedAttempt);

            modelBuilder.Entity<LoginAttemptOperation>()
                .HasIndex(operation => operation.CreatedAt);


            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Task)
                .WithMany()
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
              .HasOne(c => c.User)
              .WithMany()
              .HasForeignKey(c => c.UserId)
              .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
