Default JWT validator
=====================

The default implementation of UVault's Identity & access management uses the following characteristics.

- The `authority` of the JWT must match a predefined value.
- The `issuer` of the JWT must match a predefined value.
- The `audience` of the JWT must match a predefined value.
- The `lifetime` of the JWT must be valid.
- The `signature key` of the JWT must be valid.

In order to use it, UVault's `core` services must be added and configured to use Identity & access management.

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    const string validIssuer = "https://uvault.eu.auth0.com/";
    const string validAudience = "https://uvault.eu.auth0.com/api/v2/";

    builder.Services.AddUVault(options =>
    {
        options.UseIAM((_, iamOptions) => iamOptions.UseDefault(validIssuer, validAudience));
    });

Next, UVault's middleware components must be added to the HTTP request pipeline.

.. code-block:: csharp

    var app = builder.Build();

    app.UseUVault();

    // TODO: Map the routes.

    app.Run();

It's also possible to use different IAM options, depending on the environment you're running.

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
