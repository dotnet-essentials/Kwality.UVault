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
namespace Kwality.UVault.QA.Users;

using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Exceptions;
using Kwality.UVault.Keys;
using Kwality.UVault.QA.Internal.Factories;
using Kwality.UVault.QA.Internal.Xunit.Traits;
using Kwality.UVault.Users.Managers;
using Kwality.UVault.Users.Models;
using Kwality.UVault.Users.Operations.Mappers;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
public sealed class UserManagementDefaultTests
{
    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(IntKey key)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        // ACT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<NotFoundException>()
                 .WithMessage($"User with key `{key}` NOT found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get by email returns NO users when NO users are found.")]
    internal async Task GetByEmail_UnknownEmail_ReturnsEmptyCollection(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(model, new UserCreateOperationMapper())
                     .ConfigureAwait(false);

        // ACT.
        IEnumerable<Model> result = await manager.GetByEmailAsync("email")
                                                 .ConfigureAwait(false);

        // ASSERT.
        result.Should()
              .BeEmpty();
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get by email returns the matches.")]
    internal async Task GetByEmail_SingleMatch_ReturnsMatches(List<Model> models)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        foreach (Model model in models)
        {
            await manager.CreateAsync(model, new UserCreateOperationMapper())
                         .ConfigureAwait(false);
        }

        // ACT.
        Model expected = models.Skip(1)
                               .First();

        IEnumerable<Model> result = await manager.GetByEmailAsync(expected.Email)
                                                 .ConfigureAwait(false);

        // ASSERT.
        result.Should()
              .BeEquivalentTo(new[] { expected, });
    }

    [FixedEmail]
    [UserManagement]
    [Theory(DisplayName = "Get by email returns the matches.")]
    internal async Task GetByEmail_MultipleMatches_ReturnsMatches(List<Model> models)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        foreach (Model model in models)
        {
            await manager.CreateAsync(model, new UserCreateOperationMapper())
                         .ConfigureAwait(false);
        }

        // ACT.
        Model expected = models.Skip(1)
                               .First();

        IEnumerable<Model> result = await manager.GetByEmailAsync(expected.Email)
                                                 .ConfigureAwait(false);

        // ASSERT.
        result.Should()
              .BeEquivalentTo(models);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        // ACT.
        IntKey key = await manager.CreateAsync(model, new UserCreateOperationMapper())
                                  .ConfigureAwait(false);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(false)).Should()
                                             .BeEquivalentTo(model);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Create raises an exception when another user with the same key already exist.")]
    internal async Task Create_KeyExists_RaisesException(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(model, new UserCreateOperationMapper())
                     .ConfigureAwait(false);

        // ACT.
        Func<Task<IntKey>> act = () => manager.CreateAsync(model, new UserCreateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<CreateException>()
                 .WithMessage($"Another user with the same key `{model.Key}` already exists.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        IntKey userId = await manager.CreateAsync(model, new UserCreateOperationMapper())
                                     .ConfigureAwait(false);

        // ACT.
        model.Email = "kwality.uvault@github.com";

        await manager.UpdateAsync(userId, model, new UserUpdateOperationMapper())
                     .ConfigureAwait(false);

        // ASSERT.
        (await manager.GetByKeyAsync(userId)
                      .ConfigureAwait(false)).Should()
                                             .BeEquivalentTo(model);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(IntKey key, Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        // ACT.
        Func<Task> act = () => manager.UpdateAsync(key, model, new UserUpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<NotFoundException>()
                 .WithMessage($"User with key `{model.Key}` NOT found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        IntKey key = await manager.CreateAsync(model, new UserCreateOperationMapper())
                                  .ConfigureAwait(false);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(false);

        // ASSERT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<NotFoundException>()
                 .WithMessage($"User with key `{key}` NOT found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Delete raises an exception when the key is not found.")]
    internal async Task Delete_UnknownKey_RaisesException(IntKey key)
    {
        // ARRANGE.
        UserManager<Model, IntKey> userManager = new UserManagerFactory().Create<Model, IntKey>();

        // ACT.
        Func<Task> act = () => userManager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<NotFoundException>()
                 .WithMessage($"User with key `{key}` NOT found.")
                 .ConfigureAwait(false);
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class Model : UserModel<IntKey>
#pragma warning restore CA1812
    {
        public Model(IntKey key, string email)
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
                if (request is Type type && type == typeof(Model))
                {
                    return new Model(context.Create<IntKey>(), this.email);
                }

                return new NoSpecimen();
            }
        }
    }
}
