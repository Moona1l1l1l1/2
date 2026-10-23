using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UniversityApi.Common;

/// <summary>Создаёт отдельный OpenAPI-документ для каждой версии API (v1, v2).</summary>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "University Course Management API",
                Version = description.ApiVersion.ToString(),
                Description = "REST Web API для управления студентами, преподавателями, курсами и записями на курсы. " +
                              "Защита: JWT Bearer, ASP.NET Core Identity, роли Admin / Teacher / Student."
            });
        }
    }
}
