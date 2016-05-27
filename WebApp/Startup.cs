using System;
using System.Data.Entity;
using System.Linq;
using Owin;
using WebApp.Models;

namespace WebApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use(async (ctx, next) =>
            {
                var tenant = GetTenantFromUrl(ctx.Request.Uri.Host);

                if (tenant == null)
                {
                    throw new ApplicationException("Tenant not found");
                }

                ctx.Environment.Add("MultiTenant", tenant);
                await next();

            });
        }

        private Tenant GetTenantFromUrl(String host)
        {
            if (String.IsNullOrEmpty(host))
            {
                throw new ApplicationException("Host Url must be specified");
            }

            Tenant tenant;
            using (var context = new MultiTenantContext())
            {
                DbSet<Tenant> tenants = context.Tenants;
                tenant = tenants.FirstOrDefault(a => a.DomainName.ToLower().Equals(host)) ??
                         tenants.FirstOrDefault(a => a.Default);

                if (tenant == null)
                {
                    throw new ApplicationException("Tenant not found and, no default found");
                }
            }

            return tenant;
        }
    }
}