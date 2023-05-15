Getting started
###############

The default implementation of Identity & Access Management incorporates the following characteristics:

- The JWT's `authority` must match a predefined value.
- The JWT's `issuer` must match a predefined value.
- The JWT's `audience` must match a predefined value.
- The JWT's `lifetime` must be valid.
- The JWT's `signature key` must be valid.

It's also possible to use a customized implementation, see :ref:`customize JWT validation <iam_custom-jwt-validation>`
for more information.

Configure ASP.NET
*****************

The Identity & Access Management component can be used by adding and configuring UVault's required services to use
IAM. Here's an example of how to do this:

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    const string validIssuer = "https://uvault.eu.auth0.com/";
    const string validAudience = "https://uvault.eu.auth0.com/api/v2/";

    builder.Services.AddUVault((_, options) =>
    {
        options.UseIAM(iamOptions => iamOptions.UseDefault(validIssuer, validAudience));
    });

    var app = builder.Build();

    app.UseUVault();
    app.Run();

In this example, the ``UseDefault`` method is used to use the default validation characteristics of JWTs.

It's also possible to use different IAM options based on other services by accessing the `IServiceProvider`_.
The following code block is an example written in C# that demonstrates how to use a different valid issuer and valid
audience depending on the environment in which the application is operating:

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddUVault((serviceProvider, options) =>
    {
        options.UseIAM(iamOptions =>
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

.. _IServiceProvider: https://learn.microsoft.com/en-us/dotnet/api/system.iserviceprovider?view=net-7.0>
