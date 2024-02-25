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
namespace Kwality.UVault.QA.Grants;

using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Exceptions;
using Kwality.UVault.Grants.Managers;
using Kwality.UVault.Grants.Models;
using Kwality.UVault.Grants.Operations.Filters.Abstractions;
using Kwality.UVault.Grants.Operations.Mappers;
using Kwality.UVault.Keys;
using Kwality.UVault.Models;
using Kwality.UVault.QA.Internal.Factories;
using Kwality.UVault.QA.Internal.Xunit.Traits;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
public sealed class GrantManagementDefaultTests
{
    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(model, new GrantCreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(0, 10)
                                                    .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(model, new GrantCreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(1, 10)
                                                    .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(0);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data NOT showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(modelOne, new GrantCreateOperationMapper())
                     .ConfigureAwait(true);

        await manager.CreateAsync(modelTwo, new GrantCreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(0, 1)
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
              .BeEquivalentTo(modelOne);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data NOT showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(modelOne, new GrantCreateOperationMapper())
                     .ConfigureAwait(true);

        await manager.CreateAsync(modelTwo, new GrantCreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(1, 1)
                                                    .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(modelTwo);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Get all with filter succeeds.")]
    internal async Task GetAll_WithFilter_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(modelOne, new GrantCreateOperationMapper())
                     .ConfigureAwait(true);

        await manager.CreateAsync(modelTwo, new GrantCreateOperationMapper())
                     .ConfigureAwait(true);

        PagedResultSet<Model> result = await manager.GetAllAsync(0, 10, new OperationFilter(modelTwo.Scopes))
                                                    .ConfigureAwait(true);

        // ASSERT.
        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.First()
              .Should()
              .BeEquivalentTo(modelTwo);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>();

        // ACT.
        await manager.CreateAsync(model, new GrantCreateOperationMapper())
                     .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(true)).ResultSet.Should()
                                            .ContainEquivalentOf(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>();

        IntKey key = await manager.CreateAsync(model, new GrantCreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        model.Scopes = new[] { "newScope", "newScope2" };

        await manager.UpdateAsync(key, model, new GrantUpdateOperationMapper())
                     .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(true)).ResultSet.Count()
                                            .Should()
                                            .Be(1);

        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(true)).ResultSet.Should()
                                            .ContainEquivalentOf(model);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(IntKey key, Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>();

        // ACT.
        Func<Task> act = () => manager.UpdateAsync(key, model, new GrantUpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update client grant: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>();

        IntKey key = await manager.CreateAsync(model, new GrantCreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(true)).ResultSet.Should()
                                            .BeEmpty();
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> userManager = new GrantManagerFactory().Create<Model, IntKey>();

        // ACT.
        Func<Task> act = () => userManager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(true);
    }

    private sealed class OperationFilter(IEnumerable<string> scopes) : IGrantFilter
    {
        public TDestination Create<TDestination>()
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(Func<Model, bool>))
            {
                throw new ReadException(
                    $"Invalid {nameof(IGrantFilter)}: Destination is NOT `{typeof(Func<Model, bool>).Name}`.");
            }

            // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
            return ((Func<Model, bool>)Filter as TDestination)!;

            // The filter which is filters out data in the store.
            bool Filter(Model model)
            {
                return Equals(model.Scopes, scopes);
            }
        }
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class Model(IntKey key) : GrantModel<IntKey>(key)
#pragma warning restore CA1812
    {
        public IEnumerable<string> Scopes { get; set; } = Array.Empty<string>();
    }
}
