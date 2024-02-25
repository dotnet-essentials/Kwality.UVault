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
namespace Kwality.UVault.Grants.Auth0.QA;

using AutoFixture.Xunit2;

using FluentAssertions;

using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Models;
using Kwality.UVault.Grants.Auth0.Extensions;
using Kwality.UVault.Grants.Auth0.Keys;
using Kwality.UVault.Grants.Auth0.Mapping.Abstractions;
using Kwality.UVault.Grants.Auth0.Models;
using Kwality.UVault.Grants.Auth0.Operations.Filters;
using Kwality.UVault.Grants.Auth0.Operations.Mappers;
using Kwality.UVault.Grants.Auth0.QA.Internal.Factories;
using Kwality.UVault.Grants.Managers;
using Kwality.UVault.Grants.Operations.Mappers.Abstractions;
using Kwality.UVault.QA.Common.System;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
[Collection("Auth0")]
public sealed class GrantManagementAuth0Tests
{
    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        GrantManager<Model, StringKey> manager
            = new GrantManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? key = null;

        try
        {
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            key = await manager.CreateAsync(model,
                                   new CreateOperationMapper(
                                       Environment.ReadString("AUTH0_TEST_APPLICATION_1_CLIENT_ID")))
                               .ConfigureAwait(true);

            // ACT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            PagedResultSet<Model> result = await manager.GetAllAsync(0, 3)
                                                        .ConfigureAwait(true);

            // ASSERT.
            result.HasNextPage.Should()
                  .BeFalse();

            result.ResultSet.Count()
                  .Should()
                  .Be(2);

            result.ResultSet.Skip(1)
                  .Take(1)
                  .First()
                  .Should()
                  .BeEquivalentTo(model, static options => options.Excluding(static grant => grant.Scopes));
        }
        finally
        {
            // Cleanup: Remove the client grant in Auth0.
            if (key != null)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                Thread.Sleep(TimeSpan.FromSeconds(2));

                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        GrantManager<Model, StringKey> manager
            = new GrantManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? key = null;

        try
        {
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            key = await manager.CreateAsync(model,
                                   new CreateOperationMapper(
                                       Environment.ReadString("AUTH0_TEST_APPLICATION_1_CLIENT_ID")))
                               .ConfigureAwait(true);

            // ACT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            PagedResultSet<Model> result = await manager.GetAllAsync(1, 10)
                                                        .ConfigureAwait(true);

            // ASSERT.
            result.HasNextPage.Should()
                  .BeFalse();

            result.ResultSet.Count()
                  .Should()
                  .Be(0);
        }
        finally
        {
            // Cleanup: Remove the client grant in Auth0.
            if (key != null)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                Thread.Sleep(TimeSpan.FromSeconds(2));

                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data NOT showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        GrantManager<Model, StringKey> manager
            = new GrantManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? keyOne = null;
        StringKey? keyTwo = null;

        try
        {
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            keyOne = await manager.CreateAsync(modelOne,
                                      new CreateOperationMapper(
                                          Environment.ReadString("AUTH0_TEST_APPLICATION_1_CLIENT_ID")))
                                  .ConfigureAwait(true);

            keyTwo = await manager.CreateAsync(modelTwo,
                                      new CreateOperationMapper(
                                          Environment.ReadString("AUTH0_TEST_APPLICATION_2_CLIENT_ID")))
                                  .ConfigureAwait(true);

            // ACT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            PagedResultSet<Model> result = await manager.GetAllAsync(1, 1)
                                                        .ConfigureAwait(true);

            // ASSERT.
            result.HasNextPage.Should()
                  .BeTrue();

            result.ResultSet.Count()
                  .Should()
                  .Be(1);

            result.ResultSet.Take(1)
                  .First()
                  .Should()
                  .BeEquivalentTo(modelOne, static options => options.Excluding(static grant => grant.Scopes));
        }
        finally
        {
            // Cleanup: Remove the client grants in Auth0.
            if (keyOne != null)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                Thread.Sleep(TimeSpan.FromSeconds(2));

                await manager.DeleteByKeyAsync(keyOne)
                             .ConfigureAwait(true);
            }

            if (keyTwo != null)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                Thread.Sleep(TimeSpan.FromSeconds(2));

                await manager.DeleteByKeyAsync(keyTwo)
                             .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Get all with filter succeeds.")]
    internal async Task GetAll_WithFilter_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        GrantManager<Model, StringKey> manager
            = new GrantManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? keyOne = null;
        StringKey? keyTwo = null;

        try
        {
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            keyOne = await manager.CreateAsync(modelOne,
                                      new CreateOperationMapper(
                                          Environment.ReadString("AUTH0_TEST_APPLICATION_1_CLIENT_ID")))
                                  .ConfigureAwait(true);

            keyTwo = await manager.CreateAsync(modelTwo,
                                      new CreateOperationMapper(
                                          Environment.ReadString("AUTH0_TEST_APPLICATION_2_CLIENT_ID")))
                                  .ConfigureAwait(true);

            // ACT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            PagedResultSet<Model> result = await manager.GetAllAsync(0, 10, new OperationFilter(modelTwo.Audience))
                                                        .ConfigureAwait(true);

            // ASSERT.
            result.ResultSet.Count()
                  .Should()
                  .Be(3);

            result.ResultSet.Should()
                  .ContainEquivalentOf(modelOne);

            result.ResultSet.Should()
                  .ContainEquivalentOf(modelTwo);
        }
        finally
        {
            // Cleanup: Remove the client grants in Auth0.
            if (keyOne != null)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                Thread.Sleep(TimeSpan.FromSeconds(2));

                await manager.DeleteByKeyAsync(keyOne)
                             .ConfigureAwait(true);
            }

            if (keyTwo != null)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                Thread.Sleep(TimeSpan.FromSeconds(2));

                await manager.DeleteByKeyAsync(keyTwo)
                             .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        GrantManager<Model, StringKey> manager
            = new GrantManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? key = null;

        try
        {
            // ACT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            key = await manager.CreateAsync(model,
                                   new CreateOperationMapper(
                                       Environment.ReadString("AUTH0_TEST_APPLICATION_1_CLIENT_ID")))
                               .ConfigureAwait(true);

            // ASSERT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            (await manager.GetAllAsync(0, 100)
                          .ConfigureAwait(true)).ResultSet.Should()
                                                .ContainEquivalentOf(model);
        }
        finally
        {
            // Cleanup: Remove the client grant in Auth0.
            if (key != null)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                Thread.Sleep(TimeSpan.FromSeconds(2));

                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        GrantManager<Model, StringKey> manager
            = new GrantManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? key = null;

        try
        {
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            key = await manager.CreateAsync(model,
                                   new CreateOperationMapper(
                                       Environment.ReadString("AUTH0_TEST_APPLICATION_1_CLIENT_ID")))
                               .ConfigureAwait(true);

            // ACT.
            model.Scopes = new[] { "read:authentication_methods" };

            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            await manager.UpdateAsync(key, model, new UpdateOperationMapper())
                         .ConfigureAwait(true);

            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            (await manager.GetAllAsync(0, 100)
                          .ConfigureAwait(true)).ResultSet.Should()
                                                .ContainEquivalentOf(model);
        }
        finally
        {
            // Cleanup: Remove the client grant in Auth0.
            if (key != null)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                Thread.Sleep(TimeSpan.FromSeconds(2));

                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(true);
            }
        }
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(StringKey key, Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        GrantManager<Model, StringKey> manager
            = new GrantManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        // ACT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));
        Func<Task> act = () => manager.UpdateAsync(key, model, new UpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update client grant: `{key}`.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        GrantManager<Model, StringKey> manager
            = new GrantManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        StringKey key = await manager.CreateAsync(model,
                                         new CreateOperationMapper(
                                             Environment.ReadString("AUTH0_TEST_APPLICATION_1_CLIENT_ID")))
                                     .ConfigureAwait(true);

        // ACT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(true)).ResultSet.Should()
                                            .NotContainEquivalentOf(model);
    }

    private static ApiConfiguration GetApiConfiguration()
    {
        return new ApiConfiguration(new Uri(Environment.ReadString("AUTH0_TOKEN_ENDPOINT")),
            Environment.ReadString("AUTH0_CLIENT_ID"), Environment.ReadString("AUTH0_CLIENT_SECRET"),
            Environment.ReadString("AUTH0_AUDIENCE"));
    }

    internal sealed class Model(StringKey key, IEnumerable<string> scopes) : GrantModel(key)
    {
        public IEnumerable<string> Scopes { get; set; } = scopes;
        public string Audience { get; } = Environment.ReadString("AUTH0_AUDIENCE");
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    private sealed class ModelMapper : IModelMapper<Model>
#pragma warning restore CA1812
    {
        public Model Map(ClientGrant clientGrant)
        {
            return new Model(new StringKey(clientGrant.Id), clientGrant.Scope);
        }
    }

    private sealed class OperationFilter(string audience) : Auth0GrantFilter
    {
        protected override GetClientGrantsRequest Map()
        {
            return new GetClientGrantsRequest { Audience = audience };
        }
    }

    private sealed class CreateOperationMapper(string clientId) : Auth0GrantCreateOperationMapper
    {
        protected override ClientGrantCreateRequest Map<TSource>(TSource source)
        {
            if (source is Model model)
            {
                return new ClientGrantCreateRequest
                {
                    Scope = model.Scopes.ToList(), Audience = model.Audience, ClientId = clientId,
                };
            }

            throw new CreateException($"Invalid {nameof(IGrantOperationMapper)}: Source is NOT `{nameof(Model)}`.");
        }
    }

    private sealed class UpdateOperationMapper : Auth0GrantUpdateOperationMapper
    {
        protected override ClientGrantUpdateRequest Map<TSource>(TSource source)
        {
            if (source is Model model)
            {
                return new ClientGrantUpdateRequest { Scope = model.Scopes.ToList() };
            }

            throw new UpdateException($"Invalid {nameof(IGrantOperationMapper)}: Source is NOT `{nameof(Model)}`.");
        }
    }
}
