﻿// =====================================================================================================================
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
namespace Kwality.UVault.Users.Auth0.QA;

using System.Net.Http.Json;

using AutoFixture;
using AutoFixture.Xunit2;

using FluentAssertions;

using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Auth0.Keys;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.QA.Common.System;
using Kwality.UVault.QA.Common.Xunit.Traits;
using Kwality.UVault.Users.Auth0.Extensions;
using Kwality.UVault.Users.Auth0.Mapping.Abstractions;
using Kwality.UVault.Users.Auth0.Models;
using Kwality.UVault.Users.Auth0.Operations.Mappers;
using Kwality.UVault.Users.Auth0.QA.Internal.Factories;
using Kwality.UVault.Users.Auth0.QA.Internal.Models;
using Kwality.UVault.Users.Managers;
using Kwality.UVault.Users.Operations.Mappers.Abstractions;

using Xunit;

using AuthenticationFailureException = Kwality.UVault.Users.Auth0.QA.Internal.Exceptions.AuthenticationFailureException;

using static UVault.QA.Common.Properties.Delays;

[Collection("Auth0")]
public sealed class UserManagementAuth0Tests
{
    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(StringKey key)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<Model, StringKey> manager
            = new UserManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        // ACT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        await Task.Delay(RateLimitDelay)
                  .ConfigureAwait(true);

        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read user: `{key}`.")
                 .ConfigureAwait(true);
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Get by email returns NO users when NO users are found.")]
    internal async Task GetByEmail_UnknownEmail_ReturnsEmptyCollection(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<Model, StringKey> manager
            = new UserManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? key = null;

        try
        {
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            key = await manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

            // ACT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            IEnumerable<Model> result = await manager.GetByEmailAsync("email@acme.com")
                                                     .ConfigureAwait(true);

            // ASSERT.
            result.Should()
                  .BeEmpty();
        }
        finally
        {
            // Cleanup: Remove the user in Auth0.
            if (key != null)
            {
                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Get by email returns the matches.")]
    internal async Task GetByEmail_SingleMatch_ReturnsMatches(List<Model> models)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<Model, StringKey> manager
            = new UserManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        var keys = new List<StringKey>();

        try
        {
            foreach (Model model in models)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                await Task.Delay(RateLimitDelay)
                          .ConfigureAwait(true);

                keys.Add(await manager.CreateAsync(model, new CreateOperationMapper())
                                      .ConfigureAwait(true));
            }

            // ACT.
            Model expected = models.Skip(1)
                                   .First();

            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            IEnumerable<Model> result = await manager.GetByEmailAsync(expected.Email)
                                                     .ConfigureAwait(true);

            // ASSERT.
            result.Should()
                  .BeEquivalentTo(new[] { expected },
                      static options => options.Excluding(static user => user.Password));
        }
        finally
        {
            // Cleanup: Remove the user(s) in Auth0.
            foreach (StringKey key in keys)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                await Task.Delay(RateLimitDelay)
                          .ConfigureAwait(true);

                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(true);
            }
        }
    }

    [FixedEmail]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Get by email returns the matches.")]
    internal async Task GetByEmail_MultipleMatches_ReturnsMatches(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<Model, StringKey> userManager
            = new UserManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        var keys = new List<StringKey>();

        try
        {
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            keys.Add(await userManager.CreateAsync(model, new CreateOperationMapper())
                                      .ConfigureAwait(true));

            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            keys.Add(await userManager.CreateAsync(model, new CreateOperationMapper("DEV-CNN-1"))
                                      .ConfigureAwait(true));

            keys.Add(await userManager.CreateAsync(model, new CreateOperationMapper("DEV-CNN-2"))
                                      .ConfigureAwait(true));

            // ACT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            IEnumerable<Model> result = await userManager.GetByEmailAsync(model.Email)
                                                         .ConfigureAwait(true);

            // ASSERT.
            result.Should()
                  .BeEquivalentTo(new[] { model, model, model },
                      static options => options.Excluding(static user => user.Password));
        }
        finally
        {
            // Cleanup: Remove the user(s) in Auth0.
            foreach (StringKey key in keys)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                await Task.Delay(RateLimitDelay)
                          .ConfigureAwait(true);

                await userManager.DeleteByKeyAsync(key)
                                 .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<Model, StringKey> manager
            = new UserManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? key = null;

        try
        {
            // ACT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            key = await manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

            // ASSERT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            (await manager.GetByKeyAsync(key)
                          .ConfigureAwait(true)).Should()
                                                .BeEquivalentTo(model,
                                                    static options => options.Excluding(static user => user.Password));
        }
        finally
        {
            // Cleanup: Remove the user in Auth0.
            if (key != null)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                await Task.Delay(RateLimitDelay)
                          .ConfigureAwait(true);

                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Create succeeds, and the created user can authenticate.")]
    internal async Task Create_CreatedUser_CanAuthenticate(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<Model, StringKey> userManager
            = new UserManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? userId = null;

        try
        {
            // ACT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            userId = await userManager.CreateAsync(model, new CreateOperationMapper())
                                      .ConfigureAwait(true);

            // ASSERT.
            (await AuthenticateUserAsync(model.Key.Value, model.Password ?? string.Empty)
                    .ConfigureAwait(true)).Should()
                                          .NotBeNullOrEmpty();
        }
        finally
        {
            // Cleanup: Remove the user in Auth0.
            if (userId != null)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                await Task.Delay(RateLimitDelay)
                          .ConfigureAwait(true);

                await userManager.DeleteByKeyAsync(userId)
                                 .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Create raises an exception when another user with the same key already exist.")]
    internal async Task Create_KeyExists_RaisesException(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<Model, StringKey> manager
            = new UserManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? key = null;

        try
        {
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            key = await manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

            // ACT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            Func<Task<StringKey>> act = () => manager.CreateAsync(model, new CreateOperationMapper());

            // ASSERT.
            await act.Should()
                     .ThrowAsync<CreateException>()
                     .WithMessage("Failed to create user.")
                     .ConfigureAwait(true);
        }
        finally
        {
            // Cleanup: Remove the user in Auth0.
            if (key != null)
            {
                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<Model, StringKey> manager
            = new UserManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? key = null;

        try
        {
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            key = await manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

            // ACT.
            model.FirstName = "Updated: FirstName.";
            model.Name = "Updated: Name.";

            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            await manager.UpdateAsync(key, model, new UpdateOperationMapper())
                         .ConfigureAwait(true);

            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            await Task.Delay(RateLimitDelay)
                      .ConfigureAwait(true);

            (await manager.GetByKeyAsync(key)
                          .ConfigureAwait(true)).Should()
                                                .BeEquivalentTo(model,
                                                    static options => options.Excluding(static user => user.Password));
        }
        finally
        {
            // Cleanup: Remove the user in Auth0.
            if (key != null)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                await Task.Delay(RateLimitDelay)
                          .ConfigureAwait(true);

                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(StringKey key, Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<Model, StringKey> manager
            = new UserManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        // ACT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        await Task.Delay(RateLimitDelay)
                  .ConfigureAwait(true);

        Func<Task> act = () => manager.UpdateAsync(key, model, new UpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update user: `{key}`.")
                 .ConfigureAwait(true);
    }

    [AutoDomainData]
    [UserManagement]
    [Auth0]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        UserManager<Model, StringKey> manager
            = new UserManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        await Task.Delay(RateLimitDelay)
                  .ConfigureAwait(true);

        StringKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                     .ConfigureAwait(true);

        // ACT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        await Task.Delay(RateLimitDelay)
                  .ConfigureAwait(true);

        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read user: `{key}`.")
                 .ConfigureAwait(true);
    }

    private static ApiConfiguration GetApiConfiguration()
    {
        return new ApiConfiguration(new Uri(Environment.ReadString("AUTH0_TOKEN_ENDPOINT")),
            Environment.ReadString("AUTH0_CLIENT_ID"), Environment.ReadString("AUTH0_CLIENT_SECRET"),
            Environment.ReadString("AUTH0_AUDIENCE"));
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

    internal sealed class Model : UserModel
    {
        public Model(StringKey email)
            : base(email)
        {
        }

        public Model(StringKey email, string password)
            : base(email, password)
        {
        }

        public string? Name { get; set; }
        public string? FirstName { get; set; }
    }

    [UsedImplicitly]
#pragma warning disable CA1812
    private sealed class ModelMapper : IModelMapper<Model>
#pragma warning restore CA1812
    {
        public Model Map(User user)
        {
            return new Model(new StringKey(user.Email)) { FirstName = user.FirstName, Name = user.LastName };
        }
    }

    private sealed class CreateOperationMapper(string connection = "Username-Password-Authentication")
        : Auth0UserCreateOperationMapper
    {
        protected override UserCreateRequest Map<TSource>(TSource source)
        {
            if (source is Model model)
            {
                return new UserCreateRequest
                {
                    Email = model.Key.Value, Connection = connection, Password = model.Password,
                };
            }

            throw new CreateException($"Invalid {nameof(IUserOperationMapper)}: Source is NOT `{nameof(Model)}`.");
        }
    }

    private sealed class UpdateOperationMapper : Auth0UserUpdateOperationMapper
    {
        protected override UserUpdateRequest Map<TSource>(TSource source)
        {
            if (source is Model model)
            {
                return new UserUpdateRequest
                {
                    Email = model.Key.Value, FirstName = model.FirstName, LastName = model.Name,
                };
            }

            throw new UpdateException($"Invalid {nameof(IUserOperationMapper)}: Source is NOT `{nameof(Model)}`.");
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class AutoDomainDataAttribute() : AutoDataAttribute(SetupFixture)
    {
        private static IFixture SetupFixture()
        {
            var fixture = new Fixture();

            fixture.Customize<StringKey>(composer =>
                composer.FromFactory(() => new StringKey($"{fixture.Create<string>()}@acme.com")));

            fixture.Customize<Model>(composer => composer
                                                 .FromFactory(() => new Model(fixture.Create<StringKey>(),
                                                     fixture.Create<string>()))
                                                 .OmitAutoProperties());

            return fixture;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class FixedEmailAttribute() : AutoDataAttribute(SetupFixture)
    {
        private static IFixture SetupFixture()
        {
            var fixture = new Fixture();
            var email = $"{fixture.Create<string>()}@acme.com";
            fixture.Customize<StringKey>(composer => composer.FromFactory(() => new StringKey(email)));

            fixture.Customize<Model>(composer => composer
                                                 .FromFactory(() => new Model(fixture.Create<StringKey>(),
                                                     fixture.Create<string>()))
                                                 .OmitAutoProperties());

            return fixture;
        }
    }
}
