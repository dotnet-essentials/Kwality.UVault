Default JWT validator
=====================

UVault's default implementation of Identity & Access Management incorporates the following characteristics:

- The JWT's `authority` must match a predefined value.
- The JWT's `issuer` must match a predefined value.
- The JWT's `audience` must match a predefined value.
- The JWT's `lifetime`` must be valid.
- The JWT's `signature key`` must be valid.

UVault's Identity & Access Management component can be used by adding and configuring UVault's `core` services to use
IAM. Here's an example of how to do this:

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    const string validIssuer = "https://uvault.eu.auth0.com/";
    const string validAudience = "https://uvault.eu.auth0.com/api/v2/";

    builder.Services.AddUVault(options =>
    {
        options.UseIAM((_, iamOptions) => iamOptions.UseDefault(validIssuer, validAudience));
    });

In this example, the ``UseDefault`` method is used to define the validation characteristics of JWTs. The `authority`,
`issuer`, and `audience` of the JWT must match predefined values, and the lifetime and signature key of the JWT must be
valid.

.. code-block:: csharp

    var app = builder.Build();

    app.UseUVault();

    // TODO: Map the routes.

    app.Run();

UVault's Identity & Access Management component provides the flexibility to use different IAM options depending on other
services.

To achieve this, first, the UVault `core` services must be added and configured to use Identity & Access Management.
Then, in the configuration, you can provide different values.

Here is an example code block in C# that demonstrates the use of different IAM options:

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddUVault(options =>
    {
        options.UseIAM((serviceProvider, iamOptions) =>
        {
            var configuration = serviceProvider.GetRequiredService<IHostEnvironment>();

            if (configuration.IsDevelopment())
            {
                const string validIssuer = "https://uvault.eu.auth0.com/";
                const string validAudience = "https://uvault.eu.auth0.com/api/v2/";
                
                iamOptions.UseDefault(validIssuer, validAudience);
            }
            else
            {
                const string validIssuer = "https://uvault-dev.eu.auth0.com/";
                const string validAudience = "https://uvault-dev.eu.auth0.com/api/v2/";
                
                iamOptions.UseDefault(validIssuer, validAudience);
            }
        });
    });
