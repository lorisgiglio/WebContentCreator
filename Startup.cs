using WebContentCreator.Classes;

namespace WebContentCreator
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient()
                    .AddHostedService<HtmlGeneratorBackgroundService>()
                    .AddSingleton<SitemapGeneratorService>();

            services.AddHostedService<SitemapBackgroundService>();
            services.AddRazorPages();
            services.AddLogging(configure => configure.AddConsole());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

        }
    }
}
