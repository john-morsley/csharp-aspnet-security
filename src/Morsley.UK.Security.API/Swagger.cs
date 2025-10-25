namespace Morsley.UK.Security.API;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI documentation.
/// </summary>
public static class Swagger
{
    /// <summary>
    /// Configures Swagger/OpenAPI documentation generation with JWT Bearer authentication support.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddSwaggerAndOpenAi(this IServiceCollection services)
    {
        // Register the API explorer service that discovers endpoints in your API.
        // This is required for Swagger to know which controllers and actions exist.
        // It scans your application and builds metadata about all available endpoints
        services.AddEndpointsApiExplorer();
        
        // Register Swagger document generation services.
        // This creates the OpenAPI specification (swagger.json) that describes your API.
        services.AddSwaggerGen(options =>
        {
            // Define a security scheme named "Bearer" in the OpenAPI specification.
            // This tells Swagger UI how authentication works for this API.
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                // Name: The name of the HTTP header where the token will be sent.
                // Standard OAuth 2.0 uses "Authorization" header.
                Name = "Authorization",
                
                // Type: Specifies this is HTTP authentication (as opposed to API key or OAuth2 flow).
                // SecuritySchemeType.Http means the token is sent in an HTTP header.
                Type = SecuritySchemeType.Http,
                
                // Scheme: The authentication scheme to use (Bearer token authentication).
                // This means tokens are prefixed with "Bearer " in the Authorization header.
                // Example: "Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
                Scheme = "Bearer",
                
                // BearerFormat: Indicates the token format (JWT = JSON Web Token).
                // This is informational for documentation purposes.
                BearerFormat = "JWT",
                
                // In: Specifies where the security token should be placed.
                // ParameterLocation.Header means it goes in an HTTP header (not query string or cookie).
                In = ParameterLocation.Header,
                
                // Description: Human-readable text shown in Swagger UI.
                // Helps developers understand how to authenticate.
                Description = "JWT Authorization header using the Bearer scheme."
            });
            
            // Add a security requirement that applies to all endpoints by default.
            // This tells Swagger UI that endpoints require authentication.
            // It creates the "Authorize" button in the Swagger UI interface.
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    // Reference the "Bearer" security scheme defined above.
                    new OpenApiSecurityScheme
                    {
                        // Reference: Links this requirement to the security definition by its Id.
                        Reference = new OpenApiReference
                        {
                            // Type: Indicates we're referencing a security scheme.
                            Type = ReferenceType.SecurityScheme,
                            
                            // Id: Must match the name used in AddSecurityDefinition ("Bearer").
                            Id = "Bearer"
                        }
                    },
                    // Scopes: OAuth 2.0 scopes required (empty for simple JWT authentication).
                    // If you were using OAuth 2.0 scopes like "read:users" or "write:orders", they'd go here.
                    Array.Empty<string>()
                }
            });
        });
        
        // Return the service collection to allow method chaining (fluent API pattern).
        return services;
    }
    
    /// <summary>
    /// Adds Swagger middleware to serve the generated OpenAPI specification and Swagger UI.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for method chaining</returns>
    public static IApplicationBuilder UseSwaggerAndOpenAi(this IApplicationBuilder app)
    {
        // Add middleware to serve the OpenAPI specification as JSON.
        // This makes the swagger.json file available at /swagger/v1/swagger.json
        // The JSON file contains the complete API specification (endpoints, parameters, responses, etc.).
        app.UseSwagger();
        
        // Add middleware to serve the Swagger UI web interface.
        // This provides an interactive HTML page (typically at /swagger) where developers can:
        // - Browse all API endpoints
        // - See request/response schemas
        // - Test endpoints by clicking "Try it out"
        // - Authenticate using the "Authorize" button to test protected endpoints
        app.UseSwaggerUI();
        
        // Return the application builder to allow method chaining (fluent API pattern).
        return app;
    }
}