// =====================================================================================================================
// = LICENSE:       Copyright (c) 2023 Kevin De Coninck
// =
// =                Permission is hereby granted, free of charge, to any person
// =                obtaining a copy of this software and associated documentation
// =                files (the "Software"), to deal in the Software without
// =                restriction, including without limitation the rights to use,
// =                copy, modify, merge, publish, distribute, sublicense, and/or sell
// =                copies of the Software, and to permit persons to whom the
// =                Software is furnished to do so, subject to the following
// =                conditions:
// =
// =                The above copyright notice and this permission notice shall be
// =                included in all copies or substantial portions of the Software.
// =
// =                THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// =                EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// =                OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// =                NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// =                HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// =                WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// =                FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// =                OTHER DEALINGS IN THE SOFTWARE.
// =====================================================================================================================
namespace Kwality.UVault.QA.Management.User;

using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Keys;
using Kwality.UVault.QA.Internal.Factories;
using Kwality.UVault.QA.Internal.Xunit.Traits;
using Kwality.UVault.Users.Exceptions;
using Kwality.UVault.Users.Managers;
using Kwality.UVault.Users.Models;
using Kwality.UVault.Users.Operations.Mappers;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
public sealed class UserManagementDefaultTests
{
    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get user by key fails when the key is NOT found.")]
    internal async Task GetByKeyAsync_UnknownKey_Fails(IntKey key)
    {
        // ARRANGE.
        UserManager<UserModel, IntKey> userManager = new UserManagerFactory().Create<UserModel, IntKey>();

        // ACT.
        Func<Task<UserModel>> act = () => userManager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UserNotFoundException>()
                 .WithMessage($"User with key `{key}` NOT found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get user(s) by email returns NO users when NO users are found.")]
    internal async Task GetByEmailAsync_UnknownEmail_NoUsers(UserModel model)
    {
        // ARRANGE.
        UserManager<UserModel, IntKey> userManager = new UserManagerFactory().Create<UserModel, IntKey>();

        await userManager.CreateAsync(model, new UserCreateOperationMapper())
                         .ConfigureAwait(false);

        // ACT.
        IEnumerable<UserModel> users = await userManager.GetByEmailAsync("email")
                                                        .ConfigureAwait(false);

        // ASSERT.
        users.Should()
             .BeEmpty();
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get user(s) by email returns users with the requested email.")]
    internal async Task GetByEmailAsync_SingleUserWithEmail_Users(List<UserModel> models)
    {
        // ARRANGE.
        UserManager<UserModel, IntKey> userManager = new UserManagerFactory().Create<UserModel, IntKey>();

        foreach (UserModel model in models)
        {
            await userManager.CreateAsync(model, new UserCreateOperationMapper())
                             .ConfigureAwait(false);
        }

        // ACT.
        UserModel user = models.Skip(1)
                               .First();

        IEnumerable<UserModel> users = await userManager.GetByEmailAsync(user.Email)
                                                        .ConfigureAwait(false);

        // ASSERT.
        users.Should()
             .BeEquivalentTo(new[] { user, });
    }

    [FixedEmail]
    [UserManagement]
    [Theory(DisplayName = "Get user(s) by email returns user with the requested email.")]
    internal async Task GetByEmailAsync_MultipleUsersWithSameEmail_Users(List<UserModel> models)
    {
        // ARRANGE.
        UserManager<UserModel, IntKey> userManager = new UserManagerFactory().Create<UserModel, IntKey>();

        foreach (UserModel model in models)
        {
            await userManager.CreateAsync(model, new UserCreateOperationMapper())
                             .ConfigureAwait(false);
        }

        // ACT.
        UserModel user = models.Skip(1)
                               .First();

        IEnumerable<UserModel> users = await userManager.GetByEmailAsync(user.Email)
                                                        .ConfigureAwait(false);

        // ASSERT.
        users.Should()
             .BeEquivalentTo(models);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Create user succeeds.")]
    internal async Task CreateAsync_Succeeds(UserModel model)
    {
        // ARRANGE.
        UserManager<UserModel, IntKey> userManager = new UserManagerFactory().Create<UserModel, IntKey>();

        // ACT.
        IntKey userId = await userManager.CreateAsync(model, new UserCreateOperationMapper())
                                         .ConfigureAwait(false);

        // ASSERT.
        (await userManager.GetByKeyAsync(userId)
                          .ConfigureAwait(false)).Should()
                                                 .BeEquivalentTo(model);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Create user fails when the user key already exists.")]
    internal async Task CreateAsync_KeyExists_Fails(UserModel model)
    {
        // ARRANGE.
        UserManager<UserModel, IntKey> userManager = new UserManagerFactory().Create<UserModel, IntKey>();

        await userManager.CreateAsync(model, new UserCreateOperationMapper())
                         .ConfigureAwait(false);

        // ACT.
        Func<Task<IntKey>> act = () => userManager.CreateAsync(model, new UserCreateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UserExistsException>()
                 .WithMessage($"Another user with the same key `{model.Key}` already exists.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Update a user succeeds.")]
    internal async Task UpdateAsync_Succeeds(UserModel model)
    {
        // ARRANGE.
        UserManager<UserModel, IntKey> userManager = new UserManagerFactory().Create<UserModel, IntKey>();

        IntKey userId = await userManager.CreateAsync(model, new UserCreateOperationMapper())
                                         .ConfigureAwait(false);

        // ACT.
        model.Email = "kwality.uvault@github.com";

        await userManager.UpdateAsync(userId, model, new UserCreateOperationMapper())
                         .ConfigureAwait(false);

        // ASSERT.
        (await userManager.GetByKeyAsync(userId)
                          .ConfigureAwait(false)).Should()
                                                 .BeEquivalentTo(model);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Update a user fails when the key is NOT found.")]
    internal async Task UpdateAsync_UnknownKey_Fails(IntKey key, UserModel model)
    {
        // ARRANGE.
        UserManager<UserModel, IntKey> userManager = new UserManagerFactory().Create<UserModel, IntKey>();

        // ACT.
        Func<Task> act = () => userManager.UpdateAsync(key, model, new UserUpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UserNotFoundException>()
                 .WithMessage($"User with key `{model.Key}` NOT found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Delete a user succeeds.")]
    internal async Task DeleteByKeyAsync_Succeeds(UserModel model)
    {
        // ARRANGE.
        UserManager<UserModel, IntKey> userManager = new UserManagerFactory().Create<UserModel, IntKey>();

        IntKey userId = await userManager.CreateAsync(model, new UserCreateOperationMapper())
                                         .ConfigureAwait(false);

        // ACT.
        await userManager.DeleteByKeyAsync(userId)
                         .ConfigureAwait(false);

        // ASSERT.
        Func<Task<UserModel>> act = () => userManager.GetByKeyAsync(userId);

        await act.Should()
                 .ThrowAsync<UserNotFoundException>()
                 .WithMessage($"User with key `{userId}` NOT found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Delete a user by key fails when the key is NOT found.")]
    internal async Task DeleteByKeyAsync_UnknownKey_Fails(IntKey key)
    {
        // ARRANGE.
        UserManager<UserModel, IntKey> userManager = new UserManagerFactory().Create<UserModel, IntKey>();

        // ACT.
        Func<Task> act = () => userManager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UserNotFoundException>()
                 .WithMessage($"User with key `{key}` NOT found.")
                 .ConfigureAwait(false);
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class UserModel : UserModel<IntKey>
#pragma warning restore CA1812
    {
        public UserModel(IntKey key, string email)
            : base(key, email)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class FixedEmailAttribute : AutoDataAttribute
    {
        public FixedEmailAttribute()
            : base(
                static () =>
                {
                    var fixture = new Fixture();

                    // Build the configuration value(s) for AutoFixture.
                    var email = $"{fixture.Create<string>()}@acme.com";

                    // Customize AutoFixture.
                    fixture.Customizations.Add(new FixedEmailSpecimenBuilder(email));

                    return fixture;
                })
        {
        }

        private sealed class FixedEmailSpecimenBuilder : ISpecimenBuilder
        {
            private readonly string email;

            public FixedEmailSpecimenBuilder(string email)
            {
                this.email = email;
            }

            public object Create(object request, ISpecimenContext context)
            {
                if (request is Type type && type == typeof(UserModel))
                {
                    return new UserModel(context.Create<IntKey>(), this.email);
                }

                return new NoSpecimen();
            }
        }
    }
}
