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
namespace Kwality.UVault.QA.M2M;

using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Exceptions;
using Kwality.UVault.Keys;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Operations.Filters.Abstractions;
using Kwality.UVault.M2M.Operations.Mappers;
using Kwality.UVault.Models;
using Kwality.UVault.QA.Internal.Factories;
using Kwality.UVault.QA.Internal.Xunit.Traits;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
public sealed class ApplicationManagementDefaultTests
{
    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenAllDataShowed_Succeeds(Model model)
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

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(model, static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenAllDataShowed_Succeeds(Model model)
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
    [Theory(DisplayName = "Get all (pageIndex: 0, all data NOT showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
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

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(modelOne, static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data NOT showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(modelOne, new CreateOperationMapper())
                     .ConfigureAwait(false);

        await manager.CreateAsync(modelTwo, new CreateOperationMapper())
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
              .BeEquivalentTo(modelTwo, static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all with filter succeeds.")]
    internal async Task GetAll_WithFilter_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(modelOne, new CreateOperationMapper())
                     .ConfigureAwait(false);

        await manager.CreateAsync(modelTwo, new CreateOperationMapper())
                     .ConfigureAwait(false);

        PagedResultSet<Model> result = await manager.GetAllAsync(0, 10, new OperationFilter(modelTwo.Name ?? string.Empty))
                                                    .ConfigureAwait(false);

        // ASSERT.
        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.First()
              .Should()
              .BeEquivalentTo(modelTwo, static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(IntKey key)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

        // ACT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read application: `{key}`. Not found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

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
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(false);

        // ACT.
        model.Name = "UVault (Sample application)";

        await manager.UpdateAsync(key, model, new ApplicationUpdateOperationMapper())
                     .ConfigureAwait(false);

        // ASSERT.
        (await manager.GetAllAsync(0, 100)
                      .ConfigureAwait(false)).ResultSet.Count()
                                             .Should()
                                             .Be(1);

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
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

        // ACT.
        Func<Task> act = () => manager.UpdateAsync(key, model, new ApplicationUpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update application: `{key}`. Not found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

        IntKey key = await manager.CreateAsync(model, new CreateOperationMapper())
                                  .ConfigureAwait(false);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(false);

        // ASSERT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read application: `{key}`. Not found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> userManager = new ApplicationManagerFactory().Create<Model, IntKey>();

        // ACT.
        Func<Task> act = () => userManager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(false);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Rotate client secret raises an exception when the key is NOT found.")]
    internal async Task RotateClientSecret_UnknownKey_RaisesException(IntKey key)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

        // ACT.
        Func<Task<Model>> act = () => manager.RotateClientSecretAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update application: `{key}`. Not found.")
                 .ConfigureAwait(false);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Rotate client secret succeeds.")]
    internal async Task RotateClientSecret_Succeeds(Model model)
    {
        // ARRANGE.
        ApplicationManager<Model, IntKey> manager = new ApplicationManagerFactory().Create<Model, IntKey>();

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

    private sealed class OperationFilter : IApplicationFilter
    {
        private readonly string name;

        public OperationFilter(string name)
        {
            this.name = name;
        }

        public TDestination Create<TDestination>()
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(Func<Model, bool>))
            {
                throw new ReadException($"Invalid {nameof(IApplicationFilter)}: Destination is NOT `{typeof(Func<Model, bool>).Name}`.");
            }

            // ReSharper disable once NullableWarningSuppressionIsUsed - Known to be safe. See previous statement.
            return ((Func<Model, bool>)Filter as TDestination)!;

            // The filter which is filters out data in the store.
            bool Filter(Model model)
            {
                return model.Name == this.name;
            }
        }
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
}
