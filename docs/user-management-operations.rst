Operations
##########

In order to carry out user creation or modification operations, an `IUserOperationMapper`_ must be specified.
For further elaboration on this concept, please refer to the :ref:`operation mappers <operation-mapper-concept>`
section.

Creating a user
***************

Using the static store
======================

When using the static store, the model utilized to represent a user is identical to the model required for user
creation. Thus, the built-in `UserCreateOperationMapper`_ may be specified when creating a user.

.. code-block:: csharp

    app.MapGet("/", (HttpContext context, UserManager<UserModel, IntKey> userManager) =>
    {
        await userManager.CreateAsync(model, new UserCreateOperationMapper())
                         .ConfigureAwait(false);

        return Task.CompletedTask;
    });

Using the Auth0 store
======================

The Auth0 store necessitates a specific model to execute its operations. As a result, you must develop a custom
operation mapper to convert your model into a request that can be comprehended by Auth0. This requires the creation of
a class that inherits from the `Auth0UserCreateOperationMapper`_ class.

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

You may now employ this mapper when creating a user.

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

When using the static store, the model utilized to represent a user is identical to the model required for user
updating. Thus, the built-in `UserUpdateOperationMapper`_ may be specified when updating a user.

.. code-block:: csharp

    app.MapGet("/", (HttpContext context, UserManager<UserModel, IntKey> userManager) =>
    {
        await userManager.UpdateAsync(model, new UserUpdateOperationMapper())
                         .ConfigureAwait(false);

        return Task.CompletedTask;
    });

Using the Auth0 store
======================

The Auth0 store necessitates a specific model to execute its operations. As a result, you must develop a custom
operation mapper to convert your model into a request that can be comprehended by Auth0. This requires the creation of
a class that inherits from the `Auth0UserUpdateOperationMapper`_ class.

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
