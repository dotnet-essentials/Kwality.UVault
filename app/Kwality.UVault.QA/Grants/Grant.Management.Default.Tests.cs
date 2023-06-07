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
namespace Kwality.UVault.QA.Grants;

using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Exceptions;
using Kwality.UVault.Keys;
using Kwality.UVault.Grants.Managers;
using Kwality.UVault.Grants.Models;
using Kwality.UVault.Grants.Operations.Filters.Abstractions;
using Kwality.UVault.Grants.Operations.Mappers;
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
                     .ConfigureAwait(false);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(0, 10)
                                                    .ConfigureAwait(false);

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
                     .ConfigureAwait(false);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(1, 10)
                                                    .ConfigureAwait(false);

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
                     .ConfigureAwait(false);

        await manager.CreateAsync(modelTwo, new GrantCreateOperationMapper())
                     .ConfigureAwait(false);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(0, 1)
                                                    .ConfigureAwait(false);

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
                     .ConfigureAwait(false);

        await manager.CreateAsync(modelTwo, new GrantCreateOperationMapper())
                     .ConfigureAwait(false);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(1, 1)
                                                    .ConfigureAwait(false);

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
    [Auth0]
    [Theory(DisplayName = "Get all with filter succeeds.")]
    internal async Task GetAll_WithFilter_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(modelOne, new GrantCreateOperationMapper())
                     .ConfigureAwait(false);

        await manager.CreateAsync(modelTwo, new GrantCreateOperationMapper())
                     .ConfigureAwait(false);

        PagedResultSet<Model> result = await manager
                                             .GetAllAsync(0, 10, new OperationFilter(modelTwo.Scopes))
                                             .ConfigureAwait(false);

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
                     .ConfigureAwait(false);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(false)).ResultSet.Should()
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
                                  .ConfigureAwait(false);

        // ACT.
        model.Scopes = new[] { "newScope", "newScope2", };

        await manager.UpdateAsync(key, model, new GrantUpdateOperationMapper())
                     .ConfigureAwait(false);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(false)).ResultSet.Count()
                                             .Should()
                                             .Be(1);

        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(false)).ResultSet.Should()
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
                 .ConfigureAwait(false);
    }

    [AutoData]
    [GrantManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        GrantManager<Model, IntKey> manager = new GrantManagerFactory().Create<Model, IntKey>();

        IntKey key = await manager.CreateAsync(model, new GrantCreateOperationMapper())
                                  .ConfigureAwait(false);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(false);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(false)).ResultSet.Should()
                                             .BeEmpty();
    }

    [AutoData]
    [GrantManagement]
    [Auth0]
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
                 .ConfigureAwait(false);
    }

    private sealed class OperationFilter : IGrantFilter
    {
        private readonly IEnumerable<string> scopes;

        public OperationFilter(IEnumerable<string> scopes)
        {
            this.scopes = scopes;
        }

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
                return Equals(model.Scopes, this.scopes);
            }
        }
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class Model : GrantModel<IntKey>
#pragma warning restore CA1812
    {
        public Model(IntKey key)
            : base(key)
        {
        }

        public IEnumerable<string> Scopes { get; set; } = Array.Empty<string>();
    }
}
