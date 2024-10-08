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

            var app = builder.Build();

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