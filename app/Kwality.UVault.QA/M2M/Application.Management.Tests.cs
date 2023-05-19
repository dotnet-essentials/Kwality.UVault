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

using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Exceptions;
using Kwality.UVault.Keys;
using Kwality.UVault.QA.Internal.Factories;
using Kwality.UVault.QA.Internal.Xunit.Traits;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Operations.Mappers.Abstractions;
using Kwality.UVault.M2M.Stores.Abstractions;
using Kwality.UVault.Models;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
public sealed class ApplicationManagementTests
{
    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, pageSize: More than the total amount) succeeds.")]
    internal async Task GetAll_FirstPageWithMoreElementsThanTotal_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(model, new CreateOperationMapper())
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

        result.ResultSet
              .Take(1)
              .First()
              .Should()
              .BeEquivalentTo(
                  model, static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, pageSize: More than the total amount) succeeds.")]
    internal async Task GetAll_SecondPageWithMoreElementsThanTotal_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(model, new CreateOperationMapper())
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
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, pageSize: Less than the total amount) succeeds.")]
    internal async Task GetAll_FirstPageWithLessElementsThanTotal_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(modelOne, new CreateOperationMapper())
                     .ConfigureAwait(false);

        await manager.CreateAsync(modelTwo, new CreateOperationMapper())
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

        result.ResultSet
              .Take(1)
              .First()
              .Should()
              .BeEquivalentTo(
                  modelOne, static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, pageSize: Less than the total amount) succeeds.")]
    internal async Task GetAll_SecondPageWithLessElementsThanTotal_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(modelOne, new CreateOperationMapper())
                     .ConfigureAwait(false);

        await manager.CreateAsync(new Model(new IntKey(50), "50"), new CreateOperationMapper())
                     .ConfigureAwait(false);

        await manager.CreateAsync(modelTwo, new CreateOperationMapper())
                     .ConfigureAwait(false);

        // ACT.
        PagedResultSet<Model> result = await manager.GetAllAsync(1, 2)
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
              .BeEquivalentTo(
                  modelTwo, static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(IntKey key)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Custom: Failed to read application: `{key}`. Not found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(false);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(false)).Should()
                                             .BeEquivalentTo(model);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(false);

        // ACT.
        model.Name = "UVault (Sample application)";

        await manager.UpdateAsync(key, model, new UpdateOperationMapper())
                     .ConfigureAwait(false);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(false)).Should()
                                             .BeEquivalentTo(model);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(IntKey key, Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.UpdateAsync(key, model, new UpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Custom: Failed to update user: `{key}`. Not found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(false);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(false);

        // ASSERT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Custom: Failed to read application: `{key}`. Not found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        // ACT.
        Func<Task> act = () => manager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(false);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Rotate client secret succeeds.")]
    internal async Task RotateClientSecret_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager
            = new ApplicationManagerFactory().Create<Model, IntKey>(static options => options.UseStore<Store>());

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(false);

        string? initialClientSecret = model.ClientSecret;

        // ACT.
        await manager.RotateClientSecretAsync(key)
                     .ConfigureAwait(false);

        // ASSERT.
        Model application = await manager.GetByKeyAsync(key)
                                         .ConfigureAwait(false);

        initialClientSecret.Should()
                           .NotMatch(application.ClientSecret);
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class Model : ApplicationModel<IntKey>
#pragma warning restore CA1812
    {
        public Model(IntKey key, string name)
            : base(key)
        {
            this.Name = name;
        }
    }

    private sealed class CreateOperationMapper : IApplicationOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new CreateException(
                    $"Invalid {nameof(IApplicationOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
            return (source as TDestination)!;
        }
    }

    private sealed class UpdateOperationMapper : IApplicationOperationMapper
    {
        public TDestination Create<TSource, TDestination>(TSource source)
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(TSource))
            {
                throw new UpdateException(
                    $"Invalid {nameof(IApplicationOperationMapper)}: Destination is NOT `{nameof(TSource)}`.");
            }

            // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
            return (source as TDestination)!;
        }
    }

#pragma warning disable CA1812 // "Avoid uninstantiated internal classes".
    [UsedImplicitly]
    internal sealed class Store : IApplicationStore<Model, IntKey>
#pragma warning restore CA1812
    {
        private readonly IDictionary<IntKey, Model> collection = new Dictionary<IntKey, Model>();

        public Task<PagedResultSet<Model>> GetAllAsync(int pageIndex, int pageSize)
        {
            IEnumerable<Model> applications = this.collection.Skip(pageIndex * pageSize)
                                                  .Take(pageSize)
                                                  .Select(static kvp => kvp.Value);

            var result = new PagedResultSet<Model>(applications, this.collection.Count > (pageIndex + 1) * pageSize);

            return Task.FromResult(result);
        }

        public Task<Model> GetByKeyAsync(IntKey key)
        {
            if (!this.collection.ContainsKey(key))
            {
                throw new ReadException($"Custom: Failed to read application: `{key}`. Not found.");
            }

            return Task.FromResult(this.collection[key]);
        }

        public Task<IntKey> CreateAsync(Model model, IApplicationOperationMapper mapper)
        {
            this.collection.Add(model.Key, mapper.Create<Model, Model>(model));

            return Task.FromResult(model.Key);
        }

        public async Task UpdateAsync(IntKey key, Model model, IApplicationOperationMapper mapper)
        {
            if (!this.collection.ContainsKey(key))
            {
                throw new UpdateException($"Custom: Failed to update user: `{key}`. Not found.");
            }

            this.collection.Remove(key);

            await this.CreateAsync(model, mapper)
                      .ConfigureAwait(false);
        }

        public Task DeleteByKeyAsync(IntKey key)
        {
            if (this.collection.ContainsKey(key))
            {
                this.collection.Remove(key);
            }

            return Task.CompletedTask;
        }

        public Task<Model> RotateClientSecretAsync(IntKey key)
        {
            if (this.collection.TryGetValue(key, out Model? model))
            {
                model.ClientSecret = Guid.NewGuid()
                                         .ToString();

                this.collection[key] = model;

                return Task.FromResult(model);
            }

            throw new UpdateException($"Failed to update application: `{key}`. Not found.");
        }
    }
}
