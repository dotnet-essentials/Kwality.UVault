.. _choosing-a-user-management-store:

Stores
######

UVault provides a static store as a built-in feature, which is capable of persisting user data in an in-memory
collection. Additionally, a NuGet package is made available that enables user storage in Auth0. For further guidance on
utilizing this store, please refer to the section :ref:`use the Auth0 store <user-management-use-auth0>`.

If the provided stores do not meet your specific requirements, it is possible to create a custom store that fits your
needs. Further information on this subject can be found in the :ref:`use a custom store <user-management-custom-store>`
section.

Using the Static store
**********************

To use the user management component with the static store, you need to add and configure the necessary services from
UVault. Below is an example of how to do this:

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddUVault((_, options) =>
    {
        options.UseUserManagement<UserModel, IntKey>();
    });

    var app = builder.Build();

    app.UseUVault();
    app.Run();

.. note::
    In order to use the static store, it is necessary to have a model that implements the integer-based key.
    Additional details on keys can be found in the :ref:`keys <user-management-key>` section.

.. _user-management-use-auth0:

Using the Auth0 store
*********************

Installation
============

To use the Auth0 store, you must first include a reference to `Kwality.UVault.Auth0.dll` in your project.
The most straightforward approach to do this is through either the NuGet package manager or the dotnet CLI.

To accomplish this through the NuGet package manager console within Visual Studio, execute the following command:

.. code-block:: console

    Install-Package Kwality.UVault.Auth0

Alternatively, you can use the .NET Core CLI from a terminal window:

.. code-block:: console

    dotnet add package Kwality.UVault.Auth0

Designing a model mapper
========================

It's also essential to define a mapper that converts the Auth0-stored model to a format that can be understood by your
code and vice versa. Additional information on model mappers is available  in :ref:`this <model-mapper-concept>` section.

It is possible to create a custom mapper by implementing the `IModelMapper<out TModel>`_ interface.

Here is an example code snippet that demonstrates a model suitable for use with Auth0, along with a mapper that
translates an Auth0 user to this model.

.. note::
    In order to use the Auth0 store, it is necessary to have a model that implements the string-based key.
    Additional details on keys can be found in the :ref:`keys <user-management-key>` section.

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

        public User ToUser(UserModel model)
        {
            return new User
            {
                Email = model.Key.Value,
                FirstName = model.FirstName,
                LastName = model.Name,
            };
        }
    }

Configure ASP.NET
=================

Once the mapper and the model are defined, you can add and configure the necessary services from UVault. Below is an
example on how to do this. 

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

.. _user-management-custom-store:

Using a custom store
********************

In the event that none of the pre-existing stores satisfy your particular needs, it is possible to design a custom
store. One potential option is to develop a store that saves users in a database using Entity Framework, or even to
persist data in JSON documents on the underlying filesystem, among other possibilities.

In order to do so, it's necessary to write a class which implements the `IUserStore<TModel, TKey>`_ interface which can
be used when you're configuring UVault's services.

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddUVault((_, options) =>
    {
        options.UseUserManagement<UserModel, StringKey>(managementOptions =>
            managementOptions.UseStore<MyStore>());
    });

    var app = builder.Build();

    app.UseUVault();
    app.Run();

.. _IModelMapper<out TModel>: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault.Auth0/Users/Mapping/Abstractions/IModel.Mapper%7BTModel%7D.cs
.. _IUserStore<TModel, TKey>: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault/Users/Stores/Abstractions/IUser.Store%7BTModel%2C%20TKey%7D.cs
