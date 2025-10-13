var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient("CosmeticApi", client =>
{
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];
    client.BaseAddress = new Uri(apiUrl);
});

// Простая настройка для кук
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Для разработки
});

// Добавляем сервис для доступа к HttpContext
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseCookiePolicy(); 
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();