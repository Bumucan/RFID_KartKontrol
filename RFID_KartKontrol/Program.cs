using Microsoft.EntityFrameworkCore;
using RFID_KartKontrol.Models;

var builder = WebApplication.CreateBuilder(args);

// LAN üzerinden eriþim (ESP görebilir)
builder.WebHost.UseUrls("http://0.0.0.0:7115");

// MVC
builder.Services.AddControllersWithViews();

// Session ekleme
builder.Services.AddSession();

// MSSQL - Windows Authentication
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        "Server=.;Database=Rfýd;Trusted_Connection=True;TrustServerCertificate=True;"
    )
);

var app = builder.Build();

// Hata yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ESP HTTPS desteklemez, sadece HTTP
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

// Session middleware
app.UseSession();

app.UseAuthorization();

// Kök URL yönlendirme
app.MapGet("/", async context =>
{
    context.Response.Redirect("/Rfid/Index");
    await Task.CompletedTask;
});

// Default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Rfid}/{action=Index}/{id?}"
);

app.Run();
