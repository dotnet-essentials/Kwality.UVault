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
namespace Kwality.UVault.APIs.Auth0.QA;

using AutoFixture;
using AutoFixture.Xunit2;

using FluentAssertions;

using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.APIs.Auth0.Extensions;
using Kwality.UVault.APIs.Auth0.Keys;
using Kwality.UVault.APIs.Auth0.Mapping.Abstractions;
using Kwality.UVault.APIs.Auth0.Models;
using Kwality.UVault.APIs.Auth0.Operations.Mappers;
using Kwality.UVault.APIs.Auth0.QA.Internal.Factories;
using Kwality.UVault.APIs.Managers;
using Kwality.UVault.APIs.Operations.Mappers.Abstractions;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.QA.Common.System;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
[Collection("Auth0")]
public sealed class ApiManagementAuth0Tests
{
    [AutoData]
    [ApiManagement]
    [Auth0]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(StringKey key)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        ApiManager<Model, StringKey> manager
            = new ApiManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        // ACT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read API: `{key}`.")
                 .ConfigureAwait(true);
    }

    [AutoDomainData]
    [ApiManagement]
    [Auth0]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        ApiManager<Model, StringKey> manager
            = new ApiManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? key = null;

        try
        {
            // ACT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            key = await manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

            // ASSERT.
            // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
            Thread.Sleep(TimeSpan.FromSeconds(2));

            (await manager.GetByKeyAsync(key)
                          .ConfigureAwait(true)).Should()
                                                .BeEquivalentTo(model);
        }
        finally
        {
            // Cleanup: Remove the application in Auth0.
            if (key != null)
            {
                // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
                Thread.Sleep(TimeSpan.FromSeconds(2));

                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(true);
            }
        }
    }

    [AutoDomainData]
    [ApiManagement]
    [Auth0]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(StringKey key)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        ApiManager<Model, StringKey> manager
            = new ApiManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        // ACT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read API: `{key}`.")
                 .ConfigureAwait(true);
    }

    [AutoDomainData]
    [ApiManagement]
    [Auth0]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        ApiManager<Model, StringKey> manager
            = new ApiManagerFactory().Create<Model, StringKey>(options =>
                options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        StringKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                     .ConfigureAwait(true);

        // ACT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read API: `{key}`.")
                 .ConfigureAwait(true);
    }

    private static ApiConfiguration GetApiConfiguration()
    {
        return new ApiConfiguration(new Uri(Environment.ReadString("AUTH0_TOKEN_ENDPOINT")),
            Environment.ReadString("AUTH0_CLIENT_ID"), Environment.ReadString("AUTH0_CLIENT_SECRET"),
            Environment.ReadString("AUTH0_AUDIENCE"));
    }

    internal sealed class Model(StringKey name) : ApiModel(name);

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    private sealed class ModelMapper : IModelMapper<Model>
#pragma warning restore CA1812
    {
        public Model Map(ResourceServer resourceServer)
        {
            return new Model(new StringKey(resourceServer.Id));
        }
    }

    private sealed class CreateOperationMapper : Auth0ApiCreateOperationMapper
    {
        protected override ResourceServerCreateRequest Map<TSource>(TSource source)
        {
            if (source is Model model)
            {
                // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
                return new ResourceServerCreateRequest { Identifier = model.Key.Value };
            }

            throw new CreateException($"Invalid {nameof(IApiOperationMapper)}: Source is NOT `{nameof(Model)}`.");
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class AutoDomainDataAttribute() : AutoDataAttribute(static () =>
    {
        var fixture = new Fixture();
        fixture.Customize<Model>(static composer => composer.OmitAutoProperties());

        return fixture;
    })
    {
        // Customize AutoFixture.
    }
}
