using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using KaraW3B.SDK;
using KaraW3B.SDK.Helpers;
using KaraW3B.Server.Core;
using KaraW3B.Server.Core.Persistence;
using KaraW3B.Server.Core.Services.SchedulerService;
using KaraW3B.Server.Core.Services.SongParser;
using KaraWeb.Server.Host.Conventions;
using KaraWeb.Server.Host.Helpers;
using KaraWeb.Server.Host.Providers.Libraries;
using KaraWeb.Server.Host.Providers.Songs;
using KaraWeb.Server.Host.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace KaraWeb.Server.Host
{
    internal sealed class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JsonOptions>(o => JsonHelper.ConfigureJsonSerializer(o.JsonSerializerOptions));

            services.AddDbContext<KaraW3BDbContext>();

            services.AddControllers(o =>
                o.Conventions.Add(new GlobalRoutePrefixConvention(KaraW3BApiConstants.ApiMainRoutePrefix)));
            services.AddSignalR();

            services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true);
                c.EnableAnnotations();
                c.SwaggerDoc(KaraW3BConstants.ApplicationName, new OpenApiInfo
                {
                    Title = KaraW3BConstants.ApplicationName,
                    Version = $"{GetType().Assembly.GetName().Version}",
                    Description = "KaraW3B allows you to manage and server your karaoke sound files"
                });
                c.DocumentAsyncFilter<RoutePrefixDocumentFilter>(KaraW3BApiConstants.ApiMainRoutePrefix);
            });

            RegisterServices(services);
            RegisterProviders(services);
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services
                .AddSingleton<ISongParserService, SongParserService>()
                .AddSingleton<ISchedulerService, SchedulerService>()
                .AddSingleton<IFileHelper, KaraW3BFileHelper>()
                .AddHostedService(s => s.GetService<ISchedulerService>());
        }

        private static void RegisterProviders(IServiceCollection services)
        {
            services
                .AddScoped<ILibrariesProvider, LibrariesProvider>()
                .AddScoped<ISongsProvider, SongsProvider>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            var swaggerPrefix = KaraW3BApiConstants.ApiMainRoutePrefix + "/swagger";
            const string docName = "openapi.json";
            app.UseSwagger(c =>
            {
                c.RouteTemplate = swaggerPrefix + "/{documentName}/" + docName;
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new()
                        {
                            Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/{KaraW3BApiConstants.ApiMainRoutePrefix}"
                        }
                    };
                });
            });
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = swaggerPrefix;
                c.SwaggerEndpoint($"{KaraW3BConstants.ApplicationName}/{docName}", KaraW3BConstants.ApplicationName);
            });
        }
    }
}