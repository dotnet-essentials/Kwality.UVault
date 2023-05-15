Use the Auth0 store
===================

In the event that Auth0 is selected as the store, the Auth0 model may not necessarily correspond to the model defined
in the code. To address this, it is essential to develop a class that implements the necessary transform to transform a
user stored in Auth0 to a model that can be understand by your application.

This can be achieved by implementing the `IModelMapper<out TModel> <https://github.com/dotnet-essentials/Kwality.UVault/blob/feature/3-add-user-management/app/Kwality.UVault.User.Management.Auth0/Mapping/Abstractions/IModel.Mapper%7BTModel%7D.cs>`_
interface.

.. code-block:: csharp

    internal sealed class UserModel : UVault.User.Management.Auth0.Models.UserModel
    {
        public UserModel(StringKey email)
            : base(email)
        {
        }

        public UserModel(StringKey email, string password)
            : base(email, password)
        {
        }

        public string? Name { get; set; }
        public string? FirstName { get; set; }
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

This mapper is responsible for converting a User returned by Auth0 into the model specified in the code.

Once you have defined the mapper to use, you can use Auth0 as your store.

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddUVault((_, options) =>
    {
        var authTokenEndpoint = new Uri("https://uvault.eu.auth0.com/oauth/token");
        var clientId = "clientId";
        var clientSecret = "clientSecret";
        var audience = "https://uvault.eu.auth0.com/api/v2/";

        var apiConfiguration = new ApiConfiguration(authTokenEndpoint, clientId, clientSecret);

        options.UseUserManagement<UserModel, StringKey>(uOptions => uOptions.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));
    });

Operation mappers
-----------------

When you want to create/update a user, you need to specify an `IUserOperationMapper <https://github.com/dotnet-essentials/Kwality.UVault/blob/feature/3-add-user-management/app/Kwality.UVault.User.Management/Operations/Mappers/Abstractions/IUser.Operation.Mapper.cs>`_.
For a more comprehensive understanding of this concept, refer to :ref:`understanding keys <user_management_understanding-keys>`.

When the `Auth0` store is used, the model specified in the code can't be used to perform these operations.
Consequently, custom mappers needs to be introduced which translated your model into a request that Auth0 understands.

Create a user
^^^^^^^^^^^^^

Instead of implementing the `IUserOperationMapper <https://github.com/dotnet-essentials/Kwality.UVault/blob/feature/3-add-user-management/app/Kwality.UVault.User.Management/Operations/Mappers/Abstractions/IUser.Operation.Mapper.cs>`_ 
interface (which is off course a valid option), you can opt to inherit from the base class `Auth0UserCreateOperationMapper <https://github.com/dotnet-essentials/Kwality.UVault/blob/feature/3-add-user-management/app/Kwality.UVault.User.Management.Auth0/Operations/Mappers/User.Create.Operation.Mapper.cs>`_.

.. code-block:: csharp

    internal sealed class UserCreateUserOperationMapper : Auth0UserCreateOperationMapper
    {
        private readonly string connection;

        public UserCreateUserOperationMapper(string connection = "Username-Password-Authentication")
        {
            this.connection = connection;
        }

        protected override UserCreateRequest Map<TSource>(TSource source)
        {
            if (source is UserModel model)
            {
                return new UserCreateRequest
                {
                    Email = model.Key.Value,
                    Connection = this.connection,
                    Password = model.Password,
                };
            }

            throw new UserCreationException(
                $"Invalid {nameof(IUserOperationMapper)}: Source is NOT `{nameof(UserModel)}`.");
        }
    }

Update a user
^^^^^^^^^^^^^

Instead of implementing the `IUserOperationMapper <https://github.com/dotnet-essentials/Kwality.UVault/blob/feature/3-add-user-management/app/Kwality.UVault.User.Management/Operations/Mappers/Abstractions/IUser.Operation.Mapper.cs>`_ 
interface (which is off course a valid option), you can opt to inherit from the base class `Auth0UserUpdateOperationMapper <https://github.com/dotnet-essentials/Kwality.UVault/blob/feature/3-add-user-management/app/Kwality.UVault.User.Management.Auth0/Operations/Mappers/User.Update.Operation.Mapper.cs>`_.

.. code-block:: csharp

    internal sealed class UserUpdateUserOperationMapper : Auth0UserUpdateOperationMapper
    {
        protected override UserUpdateRequest Map<TSource>(TSource source)
        {
            if (source is UserModel model)
            {
                return new UserUpdateRequest
                {
                    Email = model.Key.Value,
                    FirstName = model.FirstName,
                    LastName = model.Name,
                };
            }

            throw new UserUpdateException(
                $"Invalid {nameof(IUserOperationMapper)}: Source is NOT `{nameof(UserModel)}`.");
        }
    }
