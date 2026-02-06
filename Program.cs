using Ekutivala_EAD.Services;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao contêiner
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICursoService, CursoService>();

// Configurações de cookies
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Adiciona serviços de sessão
builder.Services.AddMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Tempo de inatividade antes da expiração
    options.Cookie.HttpOnly = true; // Impede acesso via JavaScript
    options.Cookie.IsEssential = true; // Essencial para o funcionamento
});
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Registra IHttpContextAccessor

var app = builder.Build();

// Configurações do pipeline de requisição
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // 🚀 ADICIONADO AQUI para garantir que a sessão seja usada antes da autorização

app.UseAuthorization();

// Mapeamento de rotas
app.MapControllerRoute(
    name: "homeRoute",
    pattern: "Files1/{action=Index}/{id?}",
    defaults: new { controller = "Files1" });
    
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
