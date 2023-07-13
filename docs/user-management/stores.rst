.. _choosing-a-user-management-store:

Stores
######

UVault offers a built-in static store as a core functionality, allowing user data to be persisted in an in-memory
collection. Furthermore, a dedicated NuGet package is provided to facilitate user storage in Auth0. For detailed
instructions on utilizing the Auth0 store, please refer to the section titled
":ref:`use the Auth0 store <user-management-use-auth0>`."

flexibility to create a custom store tailored to your needs. Comprehensive information on this topic can be found in the
section titled ":ref:`use a custom store <user-management-custom-store>`."

By exploring the provided resources, you can gain a deeper understanding of how to leverage the built-in static store,
integrate with Auth0 for user storage, or develop a custom store that aligns precisely with your unique requirements.

Using the Static store
**********************

In order to utilize the user management component with the static store in UVault, it is necessary to add and configure
the required services from UVault. The following example demonstrates how to accomplish this:

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddUVault((_, options) =>
    {
        options.UseUserManagement<UserModel, IntKey>();
    });

    var app = builder.Build();

    app.UseUVault();
    app.Run();

In this example, the AddUVault method is invoked on the services object, which represents the service collection in your
application's startup or configuration code.

By following this example and configuring the necessary services, you can integrate the user management component of
UVault with the static store in your application. This enables seamless management and interaction with users within the
UVault ecosystem.

.. note::
    In order to use the static store, it is necessary to have a model that implements the integer-based key.
    Additional details on keys can be found in the :ref:`keys <user-management-key>` section.

.. _user-management-use-auth0:

Using the Auth0 store
*********************

Installation
============

In order to use the Auth0 store in UVault, you need to add a reference to the `Kwality.UVault.Auth0.dll` in your
project. This can be done conveniently through either the NuGet package manager or the dotnet CLI.

To add the reference via the NuGet package manager console in Visual Studio, you can execute the following command:

.. code-block:: console

    Install-Package Kwality.UVault.Auth0

Alternatively, you can use the .NET Core CLI from a terminal window:

.. code-block:: console

    dotnet add package Kwality.UVault.Auth0

Designing a model mapper
========================

In order to ensure seamless communication between the Auth0 store and your codebase, it is crucial to define a mapper
that facilitates the conversion of the Auth0-stored model to a format that can be easily understood by your code, and
vice versa. Detailed information on model mappers can be found in the ":ref:`this <model-mapper-concept>`" section.

To create a custom mapper, you can implement the `IModelMapper<out TModel>_` interface, which provides the necessary
structure for translating models.

Here is an example code snippet that demonstrates a model suitable for use with Auth0, along with a corresponding mapper
that converts an Auth0 user to this model:

.. code-block:: csharp

    public sealed class UserModel : Kwality.UVault.Auth0.Models.UserModel
    {
        public UserModel(StringKey email, string firstName, string lastName)
            : base(email)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public UserModel(StringKey email, string password, string firstName, string lastName)
            : base(email, password)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    private sealed class UserModelMapper : IModelMapper<UserModel>
    {
        public UserModel Map(User user)
        {
            return new UserModel(new StringKey(user.Email))
            {
                FirstName = user.FirstName,
                Name = user.LastName,
            };
        }
    }

In this example, the `UserModel` class represents a model suitable for working with Auth0. It includes properties
such as first name and last name to capture the relevant information from the Auth0 user.

The `UserModelMapper` class implements the `IModelMapper<UserModel>` interface. Within the `Map` method, it converts a
user stored in Auth0 to a `UserModel` object, ensuring that the necessary properties are mapped accordingly.

.. note::
    In order to use the Auth0 store, it is necessary to have a model that implements the string-based key.
    Additional details on keys can be found in the :ref:`keys <user-management-key>` section.

Configure ASP.NET
=================

Once you have defined the mapper and the model, the next step is to add and configure the required services from UVault.
Here is an example of how to accomplish this:

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    const string auth0TokenEndpoint = "https://uvault.eu.auth0.com/oauth/token";
    const string auth0ClientId = "Client ID";
    const string auth0ClientSecret = "Client Secret";
    const string auth0Audience = "https://uvault.eu.auth0.com/api/v2/";

    var apiConfiguration = new ApiConfiguration(auth0TokenEndpoint,
        auth0ClientId, auth0ClientSecret, auth0Audience);

    builder.Services.AddUVault((_, options) =>
    {
        options.UseUserManagement<UserModel, StringKey>(managementOptions => 
            managementOptions.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));
    });

    var app = builder.Build();

    app.UseUVault();
    app.Run();

By following this example and configuring the necessary services, you can integrate the user management component of
UVault with the Auth0 store in your application. This enables seamless management and interaction with users within the
UVault ecosystem.

.. _user-management-custom-store:

Using a custom store
********************

If none of the pre-existing stores meet your specific requirements, UVault provides the flexibility to design a custom
store tailored to your needs. One option is to develop a store that persists user data in a database using Entity
Framework or stores data in JSON documents on the underlying file system, among other possibilities.

To create a custom store, you need to implement the `IUserStore<TModel, TKey>`_ interface in a class. This class will
serve as the implementation for the custom store and can be used when configuring UVault's services.

Here is an example of how to create a custom store class:

.. code-block:: csharp

    public class MyStore : IUserStore<UserModel, StringKey>
    {
        // Implement the required methods of IUserStore interface
        // to interact with your custom data storage mechanism
    }

In this example, `MyStore` is a class that implements the `IUserStore<UserModel, string>` interface. It is responsible
for defining the behavior of the custom store, including methods for user retrieval, creation, modification, and
removal. You would need to implement these methods based on your specific data storage mechanism.

Once you have implemented the custom store, you can use it when configuring UVault's services by adding it as a
parameter to the AddUVault method:

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddUVault((_, options) =>
    {
        options.UseUserManagement<UserModel, StringKey>(managementOptions => managementOptions.UseStore<MyStore>());
    });

    var app = builder.Build();

    app.UseUVault();
    app.Run();

In this example, the AddUVault method is invoked on the services object, which represents the service collection in your
application's startup or configuration code.

By following this example and configuring the necessary services, you can integrate the user management component of
UVault with any custom store in your application. This enables seamless management and interaction with users within the
UVault ecosystem.

.. _IModelMapper<out TModel>: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault.Auth0/Users/Mapping/Abstractions/IModel.Mapper%7BTModel%7D.cs
.. _IUserStore<TModel, TKey>: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault/Users/Stores/Abstractions/IUser.Store%7BTModel%2C%20TKey%7D.cs
