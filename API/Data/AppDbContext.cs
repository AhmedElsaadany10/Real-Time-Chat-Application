using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace API.Data
{
    public class AppDbContext:IdentityDbContext<AppUser,AppRole,int,
                IdentityUserClaim<int>,AppUserRole,IdentityUserLogin<int>,
                IdentityRoleClaim<int>,IdentityUserToken<int>>
    {
        public AppDbContext(DbContextOptions options):base(options){

        }
        public DbSet<UserLike>Likes{get; set;}
        public DbSet<Message>Messages{get; set;}
        public DbSet<Group>Groups{get; set;}
        public DbSet<Connection>Connections{get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder){
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>().ToTable("Users", "identity");
            modelBuilder.Entity<AppRole>().ToTable("Roles", "identity");
            modelBuilder.Entity<AppUserRole>().ToTable("UserRoles", "identity");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims", "identity");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins", "identity");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims", "identity");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens", "identity");

            modelBuilder.Entity<AppUser>()
            .HasMany(u=>u.UserRoles)
            .WithOne(u=>u.User)
            .HasForeignKey(u=>u.UserId)
            .IsRequired();

            modelBuilder.Entity<AppRole>()
            .HasMany(u=>u.UserRoles)
            .WithOne(u=>u.Role)
            .HasForeignKey(u=>u.RoleId)
            .IsRequired();

            modelBuilder.Entity<UserLike>()
            .HasKey(k=>new{k.SourceUserId,k.LikedUserId});

            modelBuilder.Entity<UserLike>()
            .HasOne(s=>s.SourceUser)
            .WithMany(l=>l.LikedUsers)
            .HasForeignKey(s=>s.SourceUserId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserLike>()
            .HasOne(s=>s.LikedUser)
            .WithMany(l=>l.LikedByUsers)
            .HasForeignKey(s=>s.LikedUserId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
            .HasOne(s=>s.Sender)
            .WithMany(m=>m.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
            .HasOne(r=>r.Recipient)
            .WithMany(m=>m.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);
            }
            
    }

    public class IdentityDbContextDbContext
    {
    }
}
