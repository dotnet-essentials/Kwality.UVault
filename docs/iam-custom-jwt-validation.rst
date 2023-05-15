.. _iam_custom-jwt-validation:

Custom JWT validation
#####################

Sometimes, it may be necessary to customize the Identity & Access Management (IAM) options in UVault.
For instance, in development, it might be necessary to turn off JWT validation.

To accomplish this, you can create a new class that implements the `IJwtValidator`_ interface.

The following implementation completely disables all validation checks:

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

To use this custom JWT validator in the Identity & Access Management component in UVault, the following code can be
used:

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddUVault(options =>
    {
        options.UseIAM(static (_, options) => options.UseJwtValidator<JwtValidator>());
    });

.. _IJwtValidator: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault.IAM/Validators/Abstractions/IJwt.Validator.cs
