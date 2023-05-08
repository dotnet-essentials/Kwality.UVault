Custom JWT validator
====================

In some cases, you might want to override the default IAM options.
For example, in development, you might want to disable the validation of any JWT.

This can be achieved by creating a class which implements the ``IJwtValidator`` interface.

The following implementation disables all validation checks.

.. code-block:: csharp

    public sealed class JwtValidator : IJwtValidator
    {
        public Action<JwtBearerOptions> Options
            => static options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
    #pragma warning disable CA5404 // "Do not disable token validation checks".
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
    #pragma warning restore CA5404
                    ValidateIssuerSigningKey = false,
                    RequireSignedTokens = false,
                    SignatureValidator = static (token, _) => new JwtSecurityToken(token),
                };
            };
    }

Then, UVault's Identity & access management component can be configured to use this implementation instead of the
default one.

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddUVault(options =>
    {
        options.UseIAM(static (_, options) => options.UseJwtValidator<JwtValidator>());
    });
