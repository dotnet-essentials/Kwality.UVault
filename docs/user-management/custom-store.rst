.. _user_management_custom-store:

Custom store
============

If any of the pre-existing built-in stores do not meet your requirements, it is possible to create a customized store by
implementing the `IUserStore<TModel, TKey> <https://github.com/dotnet-essentials/Kwality.UVault/blob/feature/3-add-user-management/app/Kwality.UVault.User.Management/Stores/Abstractions/IUserStore%7BTModel%2C%20TKey%7D.cs>`_
interface.

The following code sample demonstrates a custom store which uses an `IDictionary<TKey,TValue> <https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.idictionary-2?view=net-7.0>`_.

.. code-block:: csharp

    internal sealed class UserModel : UserModel<IntKey>
    {
        public UserModel(IntKey key, string email)
            : base(key, email)
        {
        }
    }

    internal sealed class UserStore : IUserStore<UserModel, IntKey>
    {
        private readonly IDictionary<IntKey, UserModel> userCollection = new Dictionary<IntKey, UserModel>();

        public Task<UserModel> GetByKeyAsync(IntKey key)
        {
            if (!this.userCollection.ContainsKey(key))
            {
                throw new UserNotFoundException($"Custom: User with key `{key}` NOT found.");
            }

            return Task.FromResult(this.userCollection[key]);
        }

        public Task<IEnumerable<UserModel>> GetByEmailAsync(string email)
        {
            return Task.FromResult(
                this.userCollection.Where(user => user.Value.Email.Equals(email, StringComparison.Ordinal))
                    .Select(static user => user.Value));
        }

        public Task<IntKey> CreateAsync(UserModel model, IUserOperationMapper operationMapper)
        {
            if (!this.userCollection.ContainsKey(model.Key))
            {
                this.userCollection.Add(model.Key, operationMapper.Create<UserModel, UserModel>(model));

                return Task.FromResult(model.Key);
            }

            throw new UserExistsException($"Custom: Another user with the same key `{model.Key}` already exists.");
        }

        public async Task UpdateAsync(IntKey key, UserModel model, IUserOperationMapper operationMapper)
        {
            if (!this.userCollection.ContainsKey(key))
            {
                throw new UserNotFoundException($"Custom: User with key `{key}` NOT found.");
            }

            this.userCollection.Remove(key);

            await this.CreateAsync(model, operationMapper)
                      .ConfigureAwait(false);
        }

        public Task DeleteByKeyAsync(IntKey key)
        {
            if (!this.userCollection.ContainsKey(key))
            {
                throw new UserNotFoundException($"Custom: User with key `{key}` NOT found.");
            }

            this.userCollection.Remove(key);

            return Task.CompletedTask;
        }
    }

Once you have defined your custom store, you can configure your ASP.NET application to use this one.

.. code-block:: csharp

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddUVault((_, options) =>
    {
        options.UseUserManagement<UserModel, IntKey>(uOptions => uOptions.UseStore<UserStore>());
    });
