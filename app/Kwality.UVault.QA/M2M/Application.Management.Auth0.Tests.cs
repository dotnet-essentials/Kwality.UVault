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
namespace Kwality.UVault.QA.M2M;

using AutoFixture;
using AutoFixture.Xunit2;

using FluentAssertions;

using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.Auth0.Configuration;
using Kwality.UVault.Auth0.Keys;
using Kwality.UVault.Auth0.M2M.Extensions;
using Kwality.UVault.Auth0.M2M.Mapping.Abstractions;
using Kwality.UVault.Auth0.M2M.Models;
using Kwality.UVault.Auth0.M2M.Operations.Mappers;
using Kwality.UVault.Exceptions;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Operations.Mappers.Abstractions;
using Kwality.UVault.QA.Internal.Factories;
using Kwality.UVault.QA.Internal.System;
using Kwality.UVault.QA.Internal.Xunit.Traits;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
[Collection("Auth0")]
public sealed class ApplicationManagementAuth0Tests
{
    [AutoData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(StringKey key)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        ApplicationManager<Model, StringKey> manager = new ApplicationManagerFactory().Create<Model, StringKey>(
            options => options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        // ACT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read application: `{key}`.")
                 .ConfigureAwait(false);
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        ApplicationManager<Model, StringKey> manager = new ApplicationManagerFactory().Create<Model, StringKey>(
            options => options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? key = null;

        try
        {
            // ACT.
            key = await manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(false);

            // ASSERT.
            (await manager.GetByKeyAsync(key)
                          .ConfigureAwait(false)).Should()
                                                 .BeEquivalentTo(
                                                     model,
                                                     static options => options.Excluding(
                                                         static application => application.ClientSecret));
        }
        finally
        {
            // Cleanup: Remove the application in Auth0.
            if (key != null)
            {
                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(false);
            }
        }
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        ApplicationManager<Model, StringKey> manager = new ApplicationManagerFactory().Create<Model, StringKey>(
            options => options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? key = null;

        try
        {
            key = await manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(false);

            // ACT.
            model.Name = "UVault (Sample application)";

            await manager.UpdateAsync(key, model, new UpdateOperationMapper())
                         .ConfigureAwait(false);

            (await manager.GetByKeyAsync(key)
                          .ConfigureAwait(false)).Should()
                                                 .BeEquivalentTo(
                                                     model,
                                                     static options => options.Excluding(
                                                         static application => application.ClientSecret));
        }
        finally
        {
            // Cleanup: Remove the application in Auth0.
            if (key != null)
            {
                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(false);
            }
        }
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(StringKey key, Model model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        ApplicationManager<Model, StringKey> manager = new ApplicationManagerFactory().Create<Model, StringKey>(
            options => options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        // ACT.
        Func<Task> act = () => manager.UpdateAsync(key, model, new UpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update application: `{key}`.")
                 .ConfigureAwait(false);
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        ApplicationManager<Model, StringKey> manager = new ApplicationManagerFactory().Create<Model, StringKey>(
            options => options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                     .ConfigureAwait(false);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(false);

        // ASSERT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read application: `{key}`.")
                 .ConfigureAwait(false);
    }

    [AutoDomainData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(StringKey key)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        ApplicationManager<Model, StringKey> manager = new ApplicationManagerFactory().Create<Model, StringKey>(
            options => options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        // ACT.
        Func<Task> act = () => manager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(false);
    }

    [AutoData]
    [M2MManagement]
    [Auth0]
    [Theory(DisplayName = "Rotate client secret succeeds.")]
    internal async Task RotateClientSecret_Succeeds(Model model)
    {
        // To ensure that we don't Auth0's "Rate Limit", we wait for 2 seconds before executing this test.
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // ARRANGE.
        ApiConfiguration apiConfiguration = GetApiConfiguration();

        ApplicationManager<Model, StringKey> manager = new ApplicationManagerFactory().Create<Model, StringKey>(
            options => options.UseAuth0Store<Model, ModelMapper>(apiConfiguration));

        StringKey? key = null;

        try
        {
            key = await manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(false);

            Model initialApplication = await manager.GetByKeyAsync(key)
                                                    .ConfigureAwait(false);

            // ACT.
            await manager.RotateClientSecretAsync(key)
                         .ConfigureAwait(false);

            // ASSERT.
            Model application = await manager.GetByKeyAsync(key)
                                             .ConfigureAwait(false);

            initialApplication.ClientSecret.Should()
                              .NotMatch(application.ClientSecret);
        }
        finally
        {
            // Cleanup: Remove the application in Auth0.
            if (key != null)
            {
                await manager.DeleteByKeyAsync(key)
                             .ConfigureAwait(false);
            }
        }
    }

    private static ApiConfiguration GetApiConfiguration()
    {
        return new ApiConfiguration(
            new Uri(Environment.ReadString("AUTH0_TOKEN_ENDPOINT")), Environment.ReadString("AUTH0_CLIENT_ID"),
            Environment.ReadString("AUTH0_CLIENT_SECRET"), Environment.ReadString("AUTH0_AUDIENCE"));
    }

    internal sealed class Model : ApplicationModel
    {
        public Model(StringKey name)
            : base(name)
        {
            this.Name = name.Value;
        }
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    private sealed class ModelMapper : IModelMapper<Model>
#pragma warning restore CA1812
    {
        public Model Map(Client client)
        {
            return new Model(new StringKey(client.Name))
            {
                Name = client.Name,
                ClientSecret = client.ClientSecret,
            };
        }
    }

    private sealed class CreateOperationMapper : Auth0ApplicationCreateOperationMapper
    {
        protected override ClientCreateRequest Map<TSource>(TSource source)
        {
            if (source is Model model)
            {
                // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
                return new ClientCreateRequest
                {
                    Name = model.Key.Value,
                };
            }

            throw new CreateException(
                $"Invalid {nameof(IApplicationOperationMapper)}: Source is NOT `{nameof(Model)}`.");
        }
    }

    private sealed class UpdateOperationMapper : Auth0ApplicationUpdateOperationMapper
    {
        protected override ClientUpdateRequest Map<TSource>(TSource source)
        {
            if (source is Model model)
            {
                // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
                return new ClientUpdateRequest
                {
                    Name = model.Name,
                };
            }

            throw new UpdateException(
                $"Invalid {nameof(IApplicationOperationMapper)}: Source is NOT `{nameof(Model)}`.");
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
                    fixture.Customize<Model>(static composer => composer.OmitAutoProperties());

                    return fixture;
                })
        {
        }
    }
}
