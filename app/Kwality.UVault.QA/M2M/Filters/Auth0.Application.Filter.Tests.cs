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
namespace Kwality.UVault.QA.M2M.Filters;

using FluentAssertions;

using global::Auth0.ManagementApi.Models;

using global::System.Linq.Expressions;

using Kwality.UVault.Auth0.M2M.Operations.Filters;
using Kwality.UVault.Exceptions;
using Kwality.UVault.M2M.Operations.Filters.Abstractions;
using Kwality.UVault.QA.Internal.Xunit.Traits;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
public sealed class Auth0ApplicationFilterTests
{
    [M2MManagement]
    [Fact(DisplayName = "Map to an invalid destination raises an exception.")]
    internal void Map_InvalidDestination_RaisesException()
    {
        // ARRANGE.
        var mapper = new ApplicationFilter();

        // ACT.
        Action act = () => mapper.Create<Expression<Func<string, bool>>>();

        // ASSERT.
        act.Should()
           .Throw<CreateException>()
           .WithMessage($"Invalid {nameof(IApplicationFilter)}: Destination is NOT `{nameof(GetClientsRequest)}`.");
    }

    [M2MManagement]
    [Fact(DisplayName = "Map succeeds.")]
    internal void Map_Succeeds()
    {
        // ARRANGE.
        var mapper = new ApplicationFilter();

        // ACT.
        var result = mapper.Create<GetClientsRequest>();

        // ASSERT.
        result.Should()
              .BeEquivalentTo(new GetClientsRequest());
    }

    private sealed class ApplicationFilter : Auth0ApplicationFilter
    {
        protected override GetClientsRequest Map()
        {
            return new GetClientsRequest();
        }
    }
}
