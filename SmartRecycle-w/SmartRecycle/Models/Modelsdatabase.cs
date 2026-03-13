using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRecycle.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string? Username { get; set; }

        public string? Gmail { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Branch { get; set; }

        [Required]
        public string? Roles { get; set; } = "user";

        [Required]
        public string? PasswordHash { get; set; }

        public int Points { get; set; } = 0;

        // Relationships
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<RecyclingLog> RecyclingLogs { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
    }

    public class RecyclingLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string BottleType { get; set; }
        public int PointsAwarded { get; set; }
        public DateTime Timestamp { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int PointsPrice { get; set; }

        public string ImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
        public int Stock { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }

    public class Invoice
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalPoints { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";
        public int TotalPoints { get; set; }
        public string? AvailableBranches { get; set; }
        public string? SelectedBranch { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int PointsPerItem { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }

    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }

    public class Machines
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public string Status { get; set; } = "Active";

        public DateTime? LastMaintenanceDate { get; set; }
        public string? ApiKey { get; set; }
        public Guid URL_GUID { get; set; }
        
    }
    public class MaintenanceLog
    {
        public int Id { get; set; }
        public int MachineId { get; set; }
        public DateTime MaintenanceDate { get; set; } = DateTime.UtcNow;
        public string Command { get; set; }
        [ForeignKey("MachineId")]
        public virtual Machines Machine { get; set; }
    }
    public class Rule
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // التقييمات
    public class Rating
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int RatingValue { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Comment { get; set; }

        public string UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false;
    }

    // الاستفسارات والشكاوي
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } // Question, Complaint, Suggestion, Other

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public bool IsReplied { get; set; } = false;

        public string? Reply { get; set; }

        public DateTime? RepliedAt { get; set; }

        public string? RepliedBy { get; set; }
    }

    // ViewModel للصفحة الرئيسية
    public class HomeViewModel
    {
        public List<Rule> Rules { get; set; }
        public double? AverageRating { get; set; }
        public int TotalRatings { get; set; }
    }
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string FromEmail { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
    }
}