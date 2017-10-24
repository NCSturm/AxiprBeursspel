﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Beursspel.Data;
using Beursspel.Models;
using Beursspel.Services;
using Beursspel.Tasks;
using Hangfire;
using Hangfire.PostgreSql;

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
                .AddUserSecrets<Startup>()
                .Build();

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration["connectionString"];
            services.AddEntityFrameworkNpgsql()
                .AddDbContext<ApplicationDbContext>(builder => builder.UseNpgsql(connectionString));


            services.AddIdentity<ApplicationUser, IdentityRole>(x =>
                {
                    x.Password.RequireDigit = false;
                    x.Password.RequiredLength = 10;
                    x.Password.RequiredUniqueChars = 0;
                    x.Password.RequireLowercase = false;
                    x.Password.RequireNonAlphanumeric = false;
                    x.Password.RequireUppercase = false;

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

            services.AddHangfire(x =>
            {
                x.UsePostgreSqlStorage(connectionString);
            });

            services.AddMvc();

        }

        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var existingRole in ExistingRoles)
            {
                await CreateRoleWithName(existingRole, roleManager, userManager);
            }
        }

        private async Task CreateRoleWithName(string name, RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHangfireServer();
            app.UseHangfireDashboard();

            app.UseStaticFiles();

            app.UseAuthentication();


            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
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
        }
    }
}