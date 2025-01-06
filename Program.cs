namespace HTMLToPDF_WebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string licenseValidation = System.IO.File.ReadAllText(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "License.txt")));
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseValidation);
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure cookie settings
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.Secure = CookieSecurePolicy.Always; 
                options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
            });
            builder.Services.AddAntiforgery(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
                options.Cookie.HttpOnly = true; 
                options.Cookie.SameSite = SameSiteMode.Strict; 
            });
            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("Content-Security-Policy", "upgrade-insecure-requests; script-src 'self' 'unsafe-inline' 'unsafe-eval'  *.syncfusion.com; style-src 'self' 'unsafe-inline'  *.syncfusion.com; frame-src  *.syncfusion.com; object-src 'none'; base-uri 'none'; default-src 'self'; font-src 'self' *.syncfusion.com; connect-src 'self'  *.syncfusion.com; img-src 'self' *.syncfusion.com; form-action 'self'; frame-ancestors 'self'");

                await next();
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}