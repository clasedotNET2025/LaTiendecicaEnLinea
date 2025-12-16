using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LaTiendecicaEnLinea.Identity.Extensions;

/// <summary>
/// Provides extension methods for configuring OpenAPI documentation in the application.
/// These extensions handle JWT Bearer authentication, document information, and API version filtering.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Configures JWT Bearer authentication for OpenAPI documents.
    /// Adds a Bearer security scheme and automatically applies security requirements to protected endpoints.
    /// </summary>
    /// <param name="options">The OpenAPI options to configure.</param>
    public static void AddJwtBearerSecurity(this OpenApiOptions options)
    {
        options.AddDocumentTransformer(new JwtBearerSecuritySchemeDocumentTransformer());
        options.AddOperationTransformer(new JwtBearerSecurityRequirementOperationTransformer());
    }

    /// <summary>
    /// Sets the basic information for an OpenAPI document.
    /// </summary>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="title">The title of the API.</param>
    /// <param name="version">The version of the API document.</param>
    /// <param name="description">A description of the API.</param>
    public static void ConfigureDocumentInfo(
        this OpenApiOptions options,
        string title,
        string version,
        string description)
    {
        options.AddDocumentTransformer(new DocumentInfoTransformer(title, version, description));
    }

    /// <summary>
    /// Filters OpenAPI operations to include only those matching a specific API version.
    /// Endpoints without version metadata are excluded from versioned documents.
    /// </summary>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="apiVersion">The API version to filter by (e.g., "v1").</param>
    public static void FilterByApiVersion(this OpenApiOptions options, string apiVersion)
    {
        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            var apiVersionMetadata = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<ApiVersionAttribute>()
                .FirstOrDefault();

            if (apiVersionMetadata == null)
            {
                return Task.FromResult<OpenApiOperation?>(null);
            }

            var endpointVersions = apiVersionMetadata.Versions;
            var documentVersion = apiVersion.TrimStart('v');
            var matches = endpointVersions.Any(v => v.ToString() == documentVersion);

            return Task.FromResult(matches ? operation : null);
        });
    }
}

/// <summary>
/// Transformer that adds a JWT Bearer security scheme to OpenAPI documents.
/// Ensures the security scheme is properly registered in the document's components.
/// </summary>
internal sealed class JwtBearerSecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
    /// <summary>
    /// Adds a Bearer security scheme to the OpenAPI document.
    /// </summary>
    /// <param name="document">The OpenAPI document to transform.</param>
    /// <param name="context">The transformation context.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.RegisterComponents();

        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token in the format: your-token-here"
        };

        document.AddComponent("Bearer", scheme);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Transformer that automatically adds JWT Bearer security requirements to protected endpoints.
/// Analyzes endpoint metadata to determine if authorization is required and applies the security requirement.
/// </summary>
internal sealed class JwtBearerSecurityRequirementOperationTransformer : IOpenApiOperationTransformer
{
    /// <summary>
    /// Adds security requirements to operations that require authorization.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to transform.</param>
    /// <param name="context">The transformation context containing endpoint metadata.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var hasAuthorize = metadata.OfType<IAuthorizeData>().Any();
        var hasAllowAnonymous = metadata.OfType<IAllowAnonymous>().Any();

        if (hasAuthorize && !hasAllowAnonymous)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();

            var bearerRef = new OpenApiSecuritySchemeReference("Bearer", context.Document, null);

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [bearerRef] = new List<string>()
            });
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Transformer that sets the basic information (title, version, description) for an OpenAPI document.
/// </summary>
internal sealed class DocumentInfoTransformer : IOpenApiDocumentTransformer
{
    private readonly string _title;
    private readonly string _version;
    private readonly string _description;

    /// <summary>
    /// Initializes a new instance of the DocumentInfoTransformer class.
    /// </summary>
    /// <param name="title">The title of the API.</param>
    /// <param name="version">The version of the API document.</param>
    /// <param name="description">A description of the API.</param>
    public DocumentInfoTransformer(string title, string version, string description)
    {
        _title = title;
        _version = version;
        _description = description;
    }

    /// <summary>
    /// Applies the document information to the OpenAPI document.
    /// </summary>
    /// <param name="document">The OpenAPI document to transform.</param>
    /// <param name="context">The transformation context.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info ??= new OpenApiInfo();

        document.Info.Title = _title;
        document.Info.Version = _version;
        document.Info.Description = _description;
        return Task.CompletedTask;
    }
}