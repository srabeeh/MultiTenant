using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace WebApp.Models
{
    [DbConfigurationType(typeof(MultiTenantDataConfiguration))]
    public class MultiTenantContext : DbContext
    {
        public MultiTenantContext() :base("multitenant")
        {
            
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Speaker> Speakers { get; set; }
        public DbSet<Session> Sessions { get; set; }
    }

    public class MultiTenantDataConfiguration : DbConfiguration
    {
        public MultiTenantDataConfiguration()
        {
            SetDatabaseInitializer(new MultiTenantContextInitializer());
        }
    }

    public class MultiTenantContextInitializer: CreateDatabaseIfNotExists<MultiTenantContext>
    {
        protected override void Seed(MultiTenantContext context)
        {
            var tenants = new List<Tenant>
            {
                new Tenant()
                {
                    Name = "SVCC",
                    DomainName = "www.siliconvalley-codecamp.com",
                    Id = 1,
                    Default = true
                },
                new Tenant()
                {
                    Name = "ANGU",
                    DomainName = "angularu.com",
                    Id = 3,
                    Default = false
                },
                new Tenant()
                {
                    Name = "CSSC",
                    DomainName = "codestarssummit.com",
                    Id = 2,
                    Default = false
                }
            };
            tenants.ForEach(a => context.Tenants.Add(a));
            context.SaveChanges();

            CreateSpeakers(context);

            CreateSessions(context);
        }

        private void CreateSpeakers(MultiTenantContext context)
        {
            var speakerJsonAll = GetEmbeddedResourceAsString("WebApp.speaker.json");

            JArray jsonValSpeakers = JArray.Parse(speakerJsonAll);
            dynamic speakersData = jsonValSpeakers;
            foreach (dynamic speaker in speakersData)
            {
                context.Speakers.Add(new Speaker
                {
                    PictureId = speaker.id,
                    FirstName = speaker.firstName,
                    LastName = speaker.lastName,
                    AllowHtml = speaker.allowHtml,
                    Bio = speaker.bio,
                    WebSite = speaker.webSite,

                });
            }
            context.SaveChanges();


        }

        private void CreateSessions(MultiTenantContext context)
        {
            var sessionJsonAll = GetEmbeddedResourceAsString("WebApp.session.json");
            var tenants = context.Tenants.ToList();
            JArray jsonValSessions = JArray.Parse(sessionJsonAll);
            dynamic sessionsData = jsonValSessions;

            var sessionTenantDict = new Dictionary<int, string>();

            foreach (dynamic session in sessionsData)
            {

                //var tenant = context.Tenants.FirstOrDefault(a => a.Name == tenantName);

                sessionTenantDict.Add((int)session.id, (string)session.tenantName);

                var sessionForAdd = new Session
                {
                    SessionId = session.id,
                    Description = session.description,
                    DescriptionShort = session.descriptionShort,
                    Title = session.title
                };

                var speakerPictureIds = new List<int>();
                foreach (dynamic speaker in session.speakers)
                {
                    dynamic pictureId = speaker.id;
                    speakerPictureIds.Add((int)pictureId);
                }

                sessionForAdd.Speakers = new List<Speaker>();
                foreach (var speakerPictureId in speakerPictureIds)
                {
                    var speakerForAdd = context.Speakers.FirstOrDefault(a => a.PictureId == speakerPictureId);
                    sessionForAdd.Speakers.Add(speakerForAdd);
                }
                context.Sessions.Add(sessionForAdd);
            }

            context.SaveChanges();
            foreach (var session in context.Sessions)
            {
                var tenant = tenants.
                    FirstOrDefault(a => a.Name == sessionTenantDict[session.SessionId]);
                session.Tenant = tenant;
            }
            context.SaveChanges();

        }

        private string GetEmbeddedResourceAsString(string resourceName)
        {
            // var s = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            var assembly = Assembly.GetExecutingAssembly();

            string result;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }
    }
}