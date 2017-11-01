using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Beursspel.Data;
using Beursspel.Models;
using Beursspel.Models.AccountViewModels;
using Beursspel.Services;
using Beursspel.Tasks;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Beursspel
{
    public class Startup
    {
        private static readonly string[] ExistingRoles = {
            "Betaald", "Admin", "Tapper"
        };

        public Startup(IConfiguration configuration)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Startup>()
                .Build();

            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration["connectionString"] ?? Configuration["prod:connectionString"];
            Debug.Assert(connectionString != null, "connecting string is null");
            services.AddEntityFrameworkNpgsql()
                .AddDbContext<ApplicationDbContext>(builder => builder.UseNpgsql(connectionString));


            services.AddIdentity<ApplicationUser, IdentityRole>(x =>
                {
                    //Relevant
                    x.Password.RequiredLength = 10;
                    //minder relevant
                    x.Password.RequireDigit = false;
                    x.Password.RequiredUniqueChars = 0;
                    x.Password.RequireLowercase = false;
                    x.Password.RequireNonAlphanumeric = false;
                    x.Password.RequireUppercase = false;
                    //Om brute forcing te voorkomen, blokkeren we een account tijdelijk als er te vaak een verkeerd
                    //wachtwoord wordt ingevoerd. 10 verkeerde logins blokkeert voor 2 minuten
                    x.Lockout = new LockoutOptions
                    {
                        AllowedForNewUsers = true,
                        DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2),
                        MaxFailedAccessAttempts = 10
                    };
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                // enables immediate logout, after updating the user's stat.
                options.ValidationInterval = TimeSpan.FromMinutes(5);
            });

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
            });

            services.AddHangfire(x =>
            {
                x.UsePostgreSqlStorage(connectionString);
            });

            services.AddMvc();

        }

        private static async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var existingRole in ExistingRoles)
            {
                await CreateRoleWithName(existingRole, roleManager);
            }
        }

        private static async Task CreateRoleWithName(string name, RoleManager<IdentityRole> roleManager)
        {
            var roleExists = await roleManager.RoleExistsAsync(name);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole(name));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            app.UsePathBase("/beursspel");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }


            app.UseSession();
            app.UseHangfireServer();

            app.UseStaticFiles();

            app.UseAuthentication();



            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(y => typeof(IRecurringTask).IsAssignableFrom(y));
            foreach (var type in types)
            {
                if (type.IsInterface)
                    continue;
                var o = (IRecurringTask)Activator.CreateInstance(type);
                if (o.Enabled)
                    RecurringJob.AddOrUpdate(() => o.ExecuteAsync(), o.Cron);
            }


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            var task = CreateRoles(serviceProvider);
            task.Wait();
            var options = new DashboardOptions
            {
                Authorization = new []{new HangfireAuthentication()}
            };
            app.UseHangfireDashboard("/hangfire", options);

            using (var context = new ApplicationDbContext())
            {
                context.Database.Migrate();
            }


        }
    }
}
