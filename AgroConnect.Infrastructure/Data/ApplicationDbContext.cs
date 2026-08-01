using AgroConnect.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgroConnect.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FarmerProfile> FarmerProfiles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ForumTopic> ForumTopics { get; set; }
        public DbSet<ForumReply> ForumReplies { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Additional configurations can go here
            builder.Entity<FarmerProfile>()
                .HasOne(f => f.ApplicationUser)
                .WithOne(u => u.FarmerProfile)
                .HasForeignKey<FarmerProfile>(f => f.ApplicationUserId);

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ForumTopic>()
                .HasOne(ft => ft.ApplicationUser)
                .WithMany()
                .HasForeignKey(ft => ft.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ForumReply>()
                .HasOne(fr => fr.ApplicationUser)
                .WithMany()
                .HasForeignKey(fr => fr.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatMessage>()
                .HasOne(cm => cm.ApplicationUser)
                .WithMany()
                .HasForeignKey(cm => cm.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ProductReview>()
    .HasOne(r => r.Product)
    .WithMany(p => p.Reviews)
    .HasForeignKey(r => r.ProductId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductReview>()
                .HasOne(r => r.ApplicationUser)
                .WithMany()
                .HasForeignKey(r => r.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Wishlist>()
    .HasOne(w => w.ApplicationUser)
    .WithMany()
    .HasForeignKey(w => w.ApplicationUserId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
