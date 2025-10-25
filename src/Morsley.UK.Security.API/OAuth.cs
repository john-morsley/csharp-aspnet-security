namespace Morsley.UK.Security.API;

/// <summary>
/// Extension methods for configuring OAuth 2.0 authentication and authorization.
/// </summary>
public static class OAuth
{
    /// <summary>
    /// Configures JWT Bearer token authentication and authorization services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">Application configuration containing authentication settings.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddOAuth(this IServiceCollection services, IConfiguration configuration)
    {
        // Register authentication services with "Bearer" as the default authentication scheme.
        // This tells ASP.NET Core that this API uses Bearer token authentication (JWT tokens in the Authorization header).
        services.AddAuthentication("Bearer")
            // Configure JWT Bearer authentication handler with the scheme name "Bearer".
            // This middleware will validate incoming JWT tokens on protected endpoints.
            .AddJwtBearer("Bearer", options =>
            {
                // Authority: The URL of the OAuth 2.0 authorization server (e.g., Keycloak or Azure Entra ID).
                // This is used to fetch the OpenID Connect metadata from /.well-known/openid-configuration.
                // The metadata contains the issuer name and public keys needed to validate JWT signatures.
                options.Authority = configuration["Authentication:Authority"];
                
                // Audience: The intended recipient of the token (typically this API's identifier).
                // The JWT's "aud" claim must match this value, ensuring the token was issued for THIS API.
                // This prevents tokens meant for other APIs from being accepted here.
                options.Audience = configuration["Authentication:Audience"];
                
                // RequireHttpsMetadata: Whether to require HTTPS when fetching metadata from the Authority.
                // Should be true in production for security, but can be false in local development.
                // When false, allows HTTP connections to Keycloak running locally in Docker.
                options.RequireHttpsMetadata = configuration.GetValue<bool>("Authentication:RequireHttpsMetadata");
            });

        // Register authorization services.
        // This enables the use of [Authorize] attributes on controllers and actions.
        // Authorization determines what authenticated users are allowed to do (permissions/policies).
        services.AddAuthorization();
        
        // Return the service collection to allow method chaining (fluent API pattern).
        return services;
    }
    
    /// <summary>
    /// Adds authentication and authorization middleware to the request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for method chaining.</returns>
    public static IApplicationBuilder UseOAuth(this IApplicationBuilder app)
    {
        // Add authentication middleware to the pipeline.
        // This middleware examines incoming requests for JWT tokens in the Authorization header.
        // It validates the token (signature, expiration, issuer, audience) and populates HttpContext.User.
        // Must be called BEFORE UseAuthorization() and any endpoints that require authentication.
        app.UseAuthentication();
        
        // Add authorization middleware to the pipeline.
        // This middleware checks if the authenticated user has permission to access the requested resource.
        // It enforces [Authorize] attributes and authorization policies.
        // Must be called AFTER UseAuthentication() but BEFORE MapControllers().
        app.UseAuthorization();
        
        // Return the application builder to allow method chaining (fluent API pattern).
        return app;
    }
}