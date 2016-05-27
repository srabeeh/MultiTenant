using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web;
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

        internal static readonly object Locker = new object();

        private Tenant GetTenantFromUrl(String host)
        {
            if (String.IsNullOrEmpty(host))
            {
                throw new ApplicationException("Host Url must be specified");
            }

            Tenant tenant;
                string cacheName = "all-tenants-cache-name";
                int cacheTimeOutSeconds = 30;

                List<Tenant> tenants = (List<Tenant>) HttpContext.Current.Cache.Get(cacheName);

            if (tenants == null)
            {
                lock (Locker)
                {
                    if (tenants == null)
                    {
                        using (var context = new MultiTenantContext())
                        {
                            tenants = context.Tenants.ToList();
                            HttpContext.Current.Cache.Insert(cacheName, tenants, null,
                                DateTime.Now.Add(new TimeSpan(0, 0, cacheTimeOutSeconds)), TimeSpan.Zero);
                        }
                    }
                }



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