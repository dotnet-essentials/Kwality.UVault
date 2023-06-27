.. _iam_custom-jwt-validation:

Custom JWT validation
#####################

On certain occasions, customizing the Identity & Access Management (IAM) options in UVault becomes necessary. For
instance, during the development phase, it may be required to disable JWT validation altogether.

To achieve this, you can create a new class that implements the `IJwtValidator`_ interface. The following implementation
serves as an example, effectively disabling all validation checks:

.. code-block:: csharp

    public sealed class CustomJwtValidator : IJwtValidator
    {
        public Action<JwtBearerOptions> Options
            => static options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                    RequireSignedTokens = false,
                    SignatureValidator = static (token, _) => new JwtSecurityToken(token),
                };
            };
    }

By creating a class like `CustomJwtValidator` and implementing the `IJwtValidator`_ interface, you can customize the
JWT validation behavior according to your requirements. In this particular case, all validation checks are effectively
disabled.

Once you have the CustomJwtValidator class defined, you can configure UVault to use it by specifying it in your
application's startup code:

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddUVault(options =>
    {
        options.UseIAM(static (_, options) => options.UseJwtValidator<JwtValidator>());
    });

.. _IJwtValidator: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault/IAM/Validators/Abstractions/IJwt.Validator.cs
