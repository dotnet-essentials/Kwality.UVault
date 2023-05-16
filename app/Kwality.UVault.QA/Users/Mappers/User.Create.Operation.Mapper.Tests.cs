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
namespace Kwality.UVault.QA.Users.Mappers;

using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Exceptions;
using Kwality.UVault.QA.Internal.Xunit.Traits;
using Kwality.UVault.Users.Operations.Mappers;
using Kwality.UVault.Users.Operations.Mappers.Abstractions;

using Xunit;

public sealed class UserCreateOperationMapperTests
{
    [UserManagement]
    [AutoData]
    [Theory(DisplayName = "Map to an invalid destination raises an exception.")]
    internal void Map_InvalidDestination_RaisesException(ModelOne model)
    {
        // ARRANGE.
        var mapper = new UserCreateOperationMapper();

        // ACT.
        Action act = () => mapper.Create<ModelOne, ModelTwo>(model);

        // ASSERT.
        act.Should()
           .Throw<CreateException>()
           .WithMessage($"Invalid {nameof(IUserOperationMapper)}: Destination is NOT `{nameof(ModelOne)}`.");
    }

    [UserManagement]
    [AutoData]
    [Theory(DisplayName = "Map succeeds.")]
    internal void Map_Succeeds(ModelOne model)
    {
        // ARRANGE.
        var mapper = new UserCreateOperationMapper();

        // ACT.
        ModelOne result = mapper.Create<ModelOne, ModelOne>(model);

        // ASSERT.
        result.Should()
              .BeEquivalentTo(model);
    }

    [UsedImplicitly]
    internal sealed class ModelOne
    {
        [UsedImplicitly]
        public string? Name { get; set; }
    }

    [UsedImplicitly]
    internal sealed class ModelTwo
    {
        [UsedImplicitly]
        public string? Name { get; set; }
    }
}
