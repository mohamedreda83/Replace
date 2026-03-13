using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SmartRecycle.Models;
using SmartRecycle.Repositories;
using SmartRecycle.Hubs;
using SmartRecycle.Services;

var builder = WebApplication.CreateBuilder(args);

// إضافة خدمة قاعدة البيانات
builder.Services.AddDbContext<SmartRecycleContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SmartRecycleDatabase")));

// إضافة خدمات المخازن (Repositories)
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IDetectionRepository, DetectionRepository>(); // ✅ جديد

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

builder.Services.AddHostedService<MaintenanceLogWatcher>();

// إضافة CORS للسماح بالوصول من Flutter App و ESP32
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// إضافة خدمات المصادقة
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    })
    .AddCookie("MachineSession", options =>
    {
        options.LoginPath = "/Machines/LoginToMachine";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.SlidingExpiration = false;
        options.Cookie.Name = "MachineAuth";
    });
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// إضافة خدمات MVC
builder.Services.AddControllersWithViews();

// إضافة خدمة الجلسة (Session)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// تكوين بيئة المعالجة لطلبات HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Index");
    app.UseStatusCodePagesWithReExecute("/Error/StatusCode", "?code={0}");
    app.UseHsts();
}
else
{
    // حتى في Development هنستخدم الصفحات المخصصة
    app.UseExceptionHandler("/Error/Index");
    app.UseStatusCodePagesWithReExecute("/Error/StatusCode", "?code={0}");
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ⭐ مهم: ترتيب الـ Middleware
app.UseCors("AllowAll");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<MaintenanceHub>("/maintenanceHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();