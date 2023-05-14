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
namespace Kwality.UVault.QA.User.Management;

using System.Net.Http.Json;

using Auth0.Core.Exceptions;
using Auth0.ManagementApi.Models;

using AutoFixture;
using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.QA.Internal.Auth0.Exceptions;
using Kwality.UVault.QA.Internal.Auth0.Models;
using Kwality.UVault.QA.Internal.Factories;
using Kwality.UVault.QA.Internal.System;
using Kwality.UVault.QA.Internal.Xunit.Traits;
using Kwality.UVault.User.Management.Auth0.Configuration;
using Kwality.UVault.User.Management.Auth0.Extensions;
using Kwality.UVault.User.Management.Auth0.Keys;
using Kwality.UVault.User.Management.Auth0.Mapping.Abstractions;
using Kwality.UVault.User.Management.Auth0.Operations.Mappers;
using Kwality.UVault.User.Management.Exceptions;
using Kwality.UVault.User.Management.Managers;
using Kwality.UVault.User.Management.Operations.Mappers.Abstractions;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
public sealed class UserManagementAuth0Tests
{
    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Get user by key fails when the key is NOT found.")]
    internal async Task GetByKeyAsync_UnknownKey_Fails(StringKey key)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<UserModel, StringKey> userManager = new UserManagerFactory().Create<UserModel, StringKey>(
            (_, options) => options.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));

        // ACT.
        Func<Task<UserModel>> act = () => userManager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UserNotFoundException>()
                 .WithMessage($"User with key `{key}` NOT found.")
                 .ConfigureAwait(false);
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Get user(s) by email returns NO users when NO users are found.")]
    internal async Task GetByEmailAsync_UnknownEmail_NoUsers(UserModel model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<UserModel, StringKey> userManager = new UserManagerFactory().Create<UserModel, StringKey>(
            (_, options) => options.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));

        StringKey? userId = null;

        try
        {
            userId = await userManager.CreateAsync(model, new UserCreateUserOperationMapper())
                                      .ConfigureAwait(false);

            // ACT.
            IEnumerable<UserModel> users = await userManager.GetByEmailAsync("email@acme.com")
                                                            .ConfigureAwait(false);

            // ASSERT.
            users.Should()
                 .BeEmpty();
        }
        finally
        {
            // Cleanup: Remove the user in Auth0.
            if (userId != null)
            {
                await userManager.DeleteByKeyAsync(userId)
                                 .ConfigureAwait(false);
            }
        }
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Get user(s) by email returns users with the requested email.")]
    internal async Task GetByEmailAsync_SingleUserWithEmail_Users(List<UserModel> models)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<UserModel, StringKey> userManager = new UserManagerFactory().Create<UserModel, StringKey>(
            (_, options) => options.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));

        var userIds = new List<StringKey>();

        try
        {
            foreach (UserModel model in models)
            {
                userIds.Add(
                    await userManager.CreateAsync(model, new UserCreateUserOperationMapper())
                                     .ConfigureAwait(false));
            }

            // ACT.
            UserModel user = models.Skip(1)
                                   .First();

            IEnumerable<UserModel> users = await userManager.GetByEmailAsync(user.Email)
                                                            .ConfigureAwait(false);

            // ASSERT.
            users.Should()
                 .BeEquivalentTo(new[] { user, }, static options => options.Excluding(static user => user.Password));
        }
        finally
        {
            // Cleanup: Remove the user(s) in Auth0.
            foreach (StringKey userId in userIds)
            {
                await userManager.DeleteByKeyAsync(userId)
                                 .ConfigureAwait(false);
            }
        }
    }

    [FixedEmail]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Get user(s) by email returns user with the requested email.")]
    internal async Task GetByEmailAsync_MultipleUsersWithSameEmail_Users(UserModel model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<UserModel, StringKey> userManager = new UserManagerFactory().Create<UserModel, StringKey>(
            (_, options) => options.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));

        var userIds = new List<StringKey>();

        try
        {
            userIds.Add(
                await userManager.CreateAsync(model, new UserCreateUserOperationMapper())
                                 .ConfigureAwait(false));

            userIds.Add(
                await userManager.CreateAsync(model, new UserCreateUserOperationMapper("DEV-CNN-1"))
                                 .ConfigureAwait(false));

            userIds.Add(
                await userManager.CreateAsync(model, new UserCreateUserOperationMapper("DEV-CNN-2"))
                                 .ConfigureAwait(false));

            // ACT.
            IEnumerable<UserModel> users = await userManager.GetByEmailAsync(model.Email)
                                                            .ConfigureAwait(false);

            // ASSERT.
            users.Should()
                 .BeEquivalentTo(
                     new[] { model, model, model, }, static options => options.Excluding(static user => user.Password));
        }
        finally
        {
            // Cleanup: Remove the user(s) in Auth0.
            foreach (StringKey userId in userIds)
            {
                await userManager.DeleteByKeyAsync(userId)
                                 .ConfigureAwait(false);
            }
        }
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Create user succeeds.")]
    internal async Task CreateAsync_Succeeds(UserModel model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<UserModel, StringKey> userManager = new UserManagerFactory().Create<UserModel, StringKey>(
            (_, options) => options.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));

        StringKey? userId = null;

        try
        {
            // ACT.
            userId = await userManager.CreateAsync(model, new UserCreateUserOperationMapper())
                                      .ConfigureAwait(false);

            // ASSERT.
            (await userManager.GetByKeyAsync(userId)
                              .ConfigureAwait(false)).Should()
                                                     .BeEquivalentTo(
                                                         model,
                                                         static options
                                                             => options.Excluding(static user => user.Password));
        }
        finally
        {
            // Cleanup: Remove the user in Auth0.
            if (userId != null)
            {
                await userManager.DeleteByKeyAsync(userId)
                                 .ConfigureAwait(false);
            }
        }
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Created user can authenticate using the specified password.")]
    internal async Task CreateAsync_CreatedUser_CanAuthenticate(UserModel model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<UserModel, StringKey> userManager = new UserManagerFactory().Create<UserModel, StringKey>(
            (_, options) => options.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));

        StringKey? userId = null;

        try
        {
            // ACT.
            userId = await userManager.CreateAsync(model, new UserCreateUserOperationMapper())
                                      .ConfigureAwait(false);

            // ASSERT.
            (await AuthenticateUserAsync(model.Key.Value, model.Password ?? string.Empty)
                    .ConfigureAwait(false)).Should()
                                           .NotBeNullOrEmpty();
        }
        finally
        {
            // Cleanup: Remove the user in Auth0.
            if (userId != null)
            {
                await userManager.DeleteByKeyAsync(userId)
                                 .ConfigureAwait(false);
            }
        }
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Create user fails when the user key already exists.")]
    internal async Task CreateAsync_KeyExists_Fails(UserModel model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<UserModel, StringKey> userManager = new UserManagerFactory().Create<UserModel, StringKey>(
            (_, options) => options.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));

        StringKey? userId = null;

        try
        {
            userId = await userManager.CreateAsync(model, new UserCreateUserOperationMapper())
                                      .ConfigureAwait(false);

            // ACT.
            Func<Task<StringKey>> act = () => userManager.CreateAsync(model, new UserCreateUserOperationMapper());

            // ASSERT.
            await act.Should()
                     .ThrowAsync<UserCreationException>()
                     .WithMessage("An error occured during the creation of the user.")
                     .ConfigureAwait(false);
        }
        finally
        {
            // Cleanup: Remove the user in Auth0.
            if (userId != null)
            {
                await userManager.DeleteByKeyAsync(userId)
                                 .ConfigureAwait(false);
            }
        }
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Update a user succeeds.")]
    internal async Task UpdateAsync_Succeeds(UserModel model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<UserModel, StringKey> userManager = new UserManagerFactory().Create<UserModel, StringKey>(
            (_, options) => options.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));

        StringKey? userId = null;

        try
        {
            userId = await userManager.CreateAsync(model, new UserCreateUserOperationMapper())
                                      .ConfigureAwait(false);

            // ACT.
            model.FirstName = "Updated: FirstName.";
            model.Name = "Updated: Name.";

            await userManager.UpdateAsync(userId, model, new UserUpdateUserOperationMapper())
                             .ConfigureAwait(false);

            // ASSERT.
            (await userManager.GetByKeyAsync(userId)
                              .ConfigureAwait(false)).Should()
                                                     .BeEquivalentTo(
                                                         model,
                                                         static options
                                                             => options.Excluding(static user => user.Password));
        }
        finally
        {
            // Cleanup: Remove the user in Auth0.
            if (userId != null)
            {
                await userManager.DeleteByKeyAsync(userId)
                                 .ConfigureAwait(false);
            }
        }
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Update a user fails when the key is NOT found.")]
    internal async Task UpdateAsync_UnknownKey_Fails(StringKey key, UserModel model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<UserModel, StringKey> userManager = new UserManagerFactory().Create<UserModel, StringKey>(
            (_, options) => options.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));

        // ACT.
        Func<Task> act = () => userManager.UpdateAsync(key, model, new UserUpdateUserOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UserNotFoundException>()
                 .WithMessage($"User with key `{key.Value}` NOT found.")
                 .ConfigureAwait(false);
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Delete a user succeeds.")]
    internal async Task DeleteByKeyAsync_Succeeds(UserModel model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<UserModel, StringKey> userManager = new UserManagerFactory().Create<UserModel, StringKey>(
            (_, options) => options.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));

        StringKey userId = await userManager.CreateAsync(model, new UserCreateUserOperationMapper())
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

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Delete a user by key fails when the key is NOT found.")]
    internal async Task DeleteByKeyAsync_UnknownKey_Fails(StringKey key)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<UserModel, StringKey> userManager = new UserManagerFactory().Create<UserModel, StringKey>(
            (_, options) => options.UseAuth0Store<UserModel, UserModelMapper>(apiConfiguration));

        // ACT.
        Func<Task> act = () => userManager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ErrorApiException>()
                 .ConfigureAwait(false);
    }

    private static ApiConfiguration GetApiConfiguration()
    {
        return new ApiConfiguration(
            new Uri(Environment.ReadString("AUTH0_TOKEN_ENDPOINT")), Environment.ReadString("AUTH0_CLIENT_ID"),
            Environment.ReadString("AUTH0_CLIENT_SECRET"), Environment.ReadString("AUTH0_AUDIENCE"));
    }

    private static async Task<string> AuthenticateUserAsync(string email, string password)
    {
        string endpoint = Environment.ReadString("AUTH0_TOKEN_ENDPOINT");
        string audience = Environment.ReadString("AUTH0_AUDIENCE");
        string clientId = Environment.ReadString("AUTH0_CLIENT_ID");
        string clientSecret = Environment.ReadString("AUTH0_CLIENT_SECRET");
        using var httpClient = new HttpClient();

        var data = new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("audience", audience),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
        };

        using var formUrlEncodedContent = new FormUrlEncodedContent(data);

        HttpResponseMessage responseMessage = await httpClient.PostAsync(new Uri(endpoint), formUrlEncodedContent)
                                                              .ConfigureAwait(false);

        if (!responseMessage.IsSuccessStatusCode)
        {
            string response = await responseMessage.Content.ReadAsStringAsync()
                                                   .ConfigureAwait(false);

            throw new AuthenticationFailureException($"Failed to authenticate user. HTTP Response: {response}");
        }

        Auth0AuthenticationResponse? responseModel = await responseMessage
                                                           .Content.ReadFromJsonAsync<Auth0AuthenticationResponse>()
                                                           .ConfigureAwait(false);

        return responseModel?.AccessToken ?? string.Empty;
    }

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

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    private sealed class UserModelMapper : IModelMapper<UserModel>
#pragma warning restore CA1812
    {
        public UserModel Map(User user)
        {
            return new UserModel(new StringKey(user.Email))
            {
                FirstName = user.FirstName,
                Name = user.LastName,
            };
        }

        // ReSharper disable once UnusedMember.Local
#pragma warning disable CA1822 // "Mark members as static".
#pragma warning disable S1144 // "Unused private types or members should be removed".
        public User ToUser(UserModel model)
#pragma warning restore S1144
#pragma warning restore CA1822
        {
            return new User
            {
                Email = model.Key.Value,
                FirstName = model.FirstName,
                LastName = model.Name,
            };
        }
    }

    private sealed class UserCreateUserOperationMapper : Auth0UserCreateOperationMapper
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
                // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
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

    private sealed class UserUpdateUserOperationMapper : Auth0UserUpdateOperationMapper
    {
        protected override UserUpdateRequest Map<TSource>(TSource source)
        {
            if (source is UserModel model)
            {
                // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
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

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class AutoDomainDataAttribute : AutoDataAttribute
    {
        public AutoDomainDataAttribute()
            : base(
                static () =>
                {
                    var fixture = new Fixture();

                    // Customize AutoFixture.
                    fixture.Customize<StringKey>(
                        composer => composer.FromFactory(() => new StringKey($"{fixture.Create<string>()}@acme.com")));

                    fixture.Customize<UserModel>(
                        composer => composer.FromFactory(
                                                () => new UserModel(
                                                    fixture.Create<StringKey>(), fixture.Create<string>()))
                                            .OmitAutoProperties());

                    return fixture;
                })
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
                    fixture.Customize<StringKey>(composer => composer.FromFactory(() => new StringKey(email)));

                    fixture.Customize<UserModel>(
                        composer => composer.FromFactory(
                                                () => new UserModel(
                                                    fixture.Create<StringKey>(), fixture.Create<string>()))
                                            .OmitAutoProperties());

                    return fixture;
                })
        {
        }
    }
}
