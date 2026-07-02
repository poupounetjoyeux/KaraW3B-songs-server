using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace KaraWeb.Server.Host.Swagger
{
    public class RoutePrefixDocumentFilter : IDocumentAsyncFilter
    {
        private readonly string _prefix;

        public RoutePrefixDocumentFilter(string prefix)
        {
            _prefix = prefix.StartsWith('/') ? prefix : $"/{prefix}";
        }

        public Task ApplyAsync(OpenApiDocument swaggerDoc, DocumentFilterContext context,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var paths = swaggerDoc.Paths.Keys.ToList();
                foreach (var path in paths)
                {
                    if (!path.StartsWith(_prefix))
                    {
                        continue;
                    }

                    var pathDocumentation = swaggerDoc.Paths[path];
                    swaggerDoc.Paths.Remove(path);
                    swaggerDoc.Paths.Add(path.Replace(_prefix, string.Empty), pathDocumentation);
                }
            }, cancellationToken);
        }
    }
}