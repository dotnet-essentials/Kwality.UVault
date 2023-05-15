Getting started
===============

To begin working with UVault's User Management component, the first step is to create a class that represents a single
user. In this case, the built-in integer-based key is used to uniquely identify the user.
For a more comprehensive understanding of this concept, refer to :ref:`understanding keys <user_management_understanding-keys>`.

.. code-block:: csharp

    public sealed class UserModel : UserModel<IntKey>
    {
        public UserModel(IntKey key, string email, string fullName)
            : base(key, email)
        {
            this.FullName = fullName;
        }

        public string FullName { get; set; }
    }

Configure ASP.NET
-----------------

UVault's User Management component can be used by adding and configuring UVault's required services to use
User Management. Here's an example of how to do this:

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddUVault((_, options) =>
    {
        options.UseUserManagement<UserModel, IntKey>();
    });

The manager which exposes the User Management operation can be injected in the HTTP handlers.

.. code-block:: csharp

    app.MapGet("/",
        async (HttpContext context, UserManager<UserModel, IntKey> userManager) =>
            await context.Response.WriteAsync("").ConfigureAwait(false));

This will enable UVault's User Management component and use the built-in `static` store.

.. warning::
  As the User Manager is injected into the HTTP handlers, utilizing two different models which uses a different key is
  impossible.

It's also possible to use a different User Manager based on other services by accessing the `IServiceProvider <https://learn.microsoft.com/en-us/dotnet/api/system.iserviceprovider?view=net-7.0>`_.
Here is an example code block in C# that demonstrates the use of a different model when running the application in
development mode.

.. code-block:: csharp

    builder.Services.AddUVault((serviceProvider, options) =>
    {
        var configuration = serviceProvider.GetRequiredService<IHostEnvironment>();

        if (configuration.IsDevelopment())
        {
            options.UseUserManagement<DevelopmentUserModel, IntKey>();
        }
        else
        {
            options.UseUserManagement<UserModel, IntKey>();
        }
    });

    public sealed class DevelopmentUserModel : UserModel<IntKey>
    {
        public DevelopmentUserModel(IntKey key, string email, string fullName)
            : base(key, email)
        {
            this.FullName = "Development: " + fullName;
        }

        public string FullName { get; set; }
    }

    public sealed class UserModel : UserModel<IntKey>
    {
        public UserModel(IntKey key, string email, string fullName)
            : base(key, email)
        {
            this.FullName = fullName;
        }

        public string FullName { get; set; }
    }

Operation mappers
-----------------

When you want to create/update a user, you need to specify an `IUserOperationMapper <https://github.com/dotnet-essentials/Kwality.UVault/blob/feature/3-add-user-management/app/Kwality.UVault.User.Management/Operations/Mappers/Abstractions/IUser.Operation.Mapper.cs>`_.
For a more comprehensive understanding of this concept, refer to :ref:`understanding keys <user_management_understanding-keys>`.

When the `Static` store is used, the model specified in the code may be utilized to execute these operations.
Consequently, the following two mappers, which solely return the designated model, can be utilized.

- `User create operation mapper <https://github.com/dotnet-essentials/Kwality.UVault/blob/feature/3-add-user-management/app/Kwality.UVault.User.Management/Operations/Mappers/User.Create.Operation.Mapper.cs>`_
- `User update operation mapper <https://github.com/dotnet-essentials/Kwality.UVault/blob/feature/3-add-user-management/app/Kwality.UVault.User.Management/Operations/Mappers/User.Update.Operation.Mapper.cs>`_
