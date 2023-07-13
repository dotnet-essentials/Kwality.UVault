Operations
##########

To perform operations related to user creation or modification, it is necessary to specify an IUserOperationMapper_.
For more detailed information on this concept, please consult the :ref:`operation mappers <operation-mapper-concept>`
section.

Creating a user
***************

Using the static store
======================

When employing the static store, the model used to represent a user is the same as the model required for user creation.
As a result, it is possible to specify the built-in `UserCreateOperationMapper`_ when creating a user.

.. code-block:: csharp

    app.MapGet("/", (HttpContext context, UserManager<UserModel, IntKey> userManager) =>
    {
        await userManager.CreateAsync(model, new UserCreateOperationMapper())
                         .ConfigureAwait(false);

        return Task.CompletedTask;
    });

Using the Auth0 store
======================

The Auth0 store requires a specific model for executing its operations. Consequently, it is essential to develop a
custom operation mapper that can convert your model into a request format understandable by Auth0. This involves
creating a class that inherits from the `Auth0UserCreateOperationMapper`_ class.

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

    public sealed class UserCreateOperationMapper : Auth0UserCreateOperationMapper
    {
        protected override UserCreateRequest Map<TSource>(TSource source)
        {
            if (source is UserModel model)
            {
                return new UserCreateRequest
                {
                    Email = model.Key.Value,
                    Password = model.Password,
                    Connection = "Username-Password-Authentication",
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                };
            }

            throw new UserCreationException(
                $"Invalid {nameof(IUserOperationMapper)}: Source is NOT `{nameof(UserModel)}`.");
        }
    }

You can now utilize this mapper when creating a user.

.. code-block:: csharp

    app.MapGet("/", (HttpContext context, UserManager<UserModel, IntKey> userManager) =>
    {
        await userManager.CreateAsync(model, new UserCreateOperationMapper())
                         .ConfigureAwait(false);

        return Task.CompletedTask;
    });

Updating a user
***************

Using the static store
======================

When employing the static store, the model used to represent a user is the same as the model required for user creation.
As a result, it is possible to specify the built-in `UserUpdateOperationMapper`_ when updating a user.

.. code-block:: csharp

    app.MapGet("/", (HttpContext context, UserManager<UserModel, IntKey> userManager) =>
    {
        await userManager.UpdateAsync(model, new UserUpdateOperationMapper())
                         .ConfigureAwait(false);

        return Task.CompletedTask;
    });

Using the Auth0 store
======================

The Auth0 store requires a specific model for executing its operations. Consequently, it is essential to develop a
custom operation mapper that can convert your model into a request format understandable by Auth0. This involves
creating a class that inherits from the `Auth0UserUpdateOperationMapper`_ class.

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

    public sealed class UserUpdateOperationMapper : Auth0UserUpdateOperationMapper
    {
        protected override UserUpdateRequest Map<TSource>(TSource source)
        {
            if (source is UserModel model)
            {
                return new UserUpdateRequest
                {
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                };
            }

            throw new UserCreationException(
                $"Invalid {nameof(IUserOperationMapper)}: Source is NOT `{nameof(UserModel)}`.");
        }
    }

You may now employ this mapper when creating a user.

.. code-block:: csharp

    app.MapGet("/", (HttpContext context, UserManager<UserModel, IntKey> userManager) =>
    {
        await userManager.UpdateAsync(model, new UserCreateOperationMapper())
                         .ConfigureAwait(false);

        return Task.CompletedTask;
    });

.. _IUserOperationMapper: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault/Users/Operations/Mappers/Abstractions/IUser.Operation.Mapper.cs
.. _UserCreateOperationMapper: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault/Users/Operations/Mappers/User.Create.Operation.Mapper.cs
.. _Auth0UserCreateOperationMapper: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault.Auth0/Users/Operations/Mappers/User.Create.Operation.Mapper.cs
.. _Auth0UserUpdateOperationMapper: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault.Auth0/Users/Operations/Mappers/User.Update.Operation.Mapper.cs
.. _UserUpdateOperationMapper: https://github.com/dotnet-essentials/Kwality.UVault/blob/main/app/Kwality.UVault/Users/Operations/Mappers/User.Update.Operation.Mapper.cs
