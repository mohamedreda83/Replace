using Microsoft.EntityFrameworkCore;

namespace SmartRecycle.Models
{
    public class SmartRecycleContext : DbContext
    {
        public SmartRecycleContext(DbContextOptions<SmartRecycleContext> options)
            : base(options)
        {
        }
        public DbSet<Detection> Detections { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Machines> Machines { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<RecyclingLog> RecyclingLogs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
     

            // إعدادات إضافية للجدول
            modelBuilder.Entity<Detection>()
                .HasIndex(d => d.Timestamp)
                .HasDatabaseName("IX_Detections_Timestamp");

           
            // ✅ علاقة User مع Orders
            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ علاقة User مع RecyclingLogs
            modelBuilder.Entity<User>()
                .HasMany(u => u.RecyclingLogs)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ علاقة User مع Invoices
            modelBuilder.Entity<User>()
                .HasMany(u => u.Invoices)
                .WithOne(i => i.User)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ علاقة Category مع Products
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ علاقة Order مع OrderItems
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ علاقة Cart مع CartItems
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ علاقة Invoice مع Order
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Order)
                .WithMany()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ ضبط دقة الأرقام العشرية
            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // ✅ بيانات أولية (Seed Data)
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "منتجات مدرسية", Description = "كل ما يخص المدرسة من أدوات" },
                new Category { Id = 2, Name = "هدايا", Description = "هدايا متنوعة للطلاب" },
                new Category { Id = 3, Name = "ألعاب", Description = "ألعاب تعليمية وترفيهية" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "قلم رصاص", Description = "قلم رصاص عالي الجودة", PointsPrice = 50, ImageUrl = "/images/products/pencil.jpg", IsAvailable = true, Stock = 100, CategoryId = 1 },
                new Product { Id = 2, Name = "دفتر ملاحظات", Description = "دفتر ملاحظات 100 ورقة", PointsPrice = 100, ImageUrl = "/images/products/notebook.jpg", IsAvailable = true, Stock = 75, CategoryId = 1 },
                new Product { Id = 3, Name = "كتاب قصص", Description = "مجموعة قصص تعليمية", PointsPrice = 200, ImageUrl = "/images/products/storybook.jpg", IsAvailable = true, Stock = 50, CategoryId = 1 },
                new Product { Id = 4, Name = "ميدالية تذكارية", Description = "ميدالية تذكارية مدرسة فتح الله", PointsPrice = 150, ImageUrl = "/images/products/medal.jpg", IsAvailable = true, Stock = 60, CategoryId = 2 },
                new Product { Id = 5, Name = "لعبة تركيب", Description = "لعبة تركيب تعليمية للأطفال", PointsPrice = 300, ImageUrl = "/images/products/puzzle.jpg", IsAvailable = true, Stock = 40, CategoryId = 3 }
            );
            // تكوين الجداول
            modelBuilder.Entity<Rule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RatingValue).IsRequired();
                entity.Property(e => e.Comment).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.IsApproved).HasDefaultValue(false);
            });

            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.IsReplied).HasDefaultValue(false);
            });

            // إضافة بيانات افتراضية للقواعد
            modelBuilder.Entity<Rule>().HasData(
                new Rule
                {
                    Id = 1,
                    Title = "تسجيل الدخول مطلوب",
                    Description = "يجب تسجيل الدخول عبر مسح رمز QR الموجود على الماكينة قبل البدء باستخدامها",
                    Order = 1,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Rule
                {
                    Id = 2,
                    Title = "وقت الجلسة محدود",
                    Description = "مدة الجلسة 5 دقائق فقط، وسيتم تسجيل الخروج تلقائياً بعد انتهاء المدة",
                    Order = 2,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Rule
                {
                    Id = 3,
                    Title = "اضغط للاكتشاف",
                    Description = "يجب الضغط على زر الاكتشاف مع كل عملية لضمان وجودك أمام الماكينة وحماية حسابك",
                    Order = 3,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Rule
                {
                    Id = 4,
                    Title = "مواد نظيفة فقط",
                    Description = "تأكد من أن المواد المعاد تدويرها نظيفة وخالية من السوائل والأوساخ",
                    Order = 4,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Rule
                {
                    Id = 5,
                    Title = "لا تترك حسابك مفتوحاً",
                    Description = "يجب تسجيل الخروج بعد الانتهاء لحماية حسابك ونقاطك من الاستخدام غير المصرح به",
                    Order = 5,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );
        }
    }
}