using System.Reflection;
using System.Web.Http;
using System.Web.Http.Routing;
using Autofac;
using Autofac.Integration.WebApi;
using ApiApp.Application;
using ApiApp.Data;
using Newtonsoft.Json;

namespace ApiApp.Api
{
    public static class ApiBootStrapper
    {
        private static void RegisterAutofac(HttpConfiguration configuration)
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new ApplicationModule());
            builder.RegisterModule(new DataModule());

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly())
                    .InstancePerRequest();

            var container = builder.Build();

            var autofacResolver = new AutofacWebApiDependencyResolver(container);
            configuration.DependencyResolver = autofacResolver;
        }

        private static void RegisterApiRoutes(HttpConfiguration configuration)
        {
            //var constraintsResolver = new DefaultInlineConstraintResolver();
            //constraintsResolver.ConstraintMap.Add(“values”, typeof(ValuesConstraint));
            configuration.MapHttpAttributeRoutes();
        }

        private static void RegisterFilters(HttpConfiguration configuration)
        {
        }

        private static void ConfigureFormatter(HttpConfiguration configuration)
        {
            var formatters = configuration.Formatters;
            formatters.Remove(formatters.XmlFormatter);

            var jsonFormatter = configuration.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings = new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
        }

        public static void Boot(HttpConfiguration configuration)
        {
            RegisterApiRoutes(configuration);
            RegisterFilters(configuration);
            RegisterAutofac(configuration);
            ConfigureFormatter(configuration);
        }
    }
}

