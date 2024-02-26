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
namespace Kwality.UVault.IAM.Extensions;

using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using Kwality.UVault.Core.Options;
using Kwality.UVault.IAM.Options;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class UVaultOptionsExtensions
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void UseIAM(this UVaultOptions options, Action<IAMOptions>? action)
    {
        ArgumentNullException.ThrowIfNull(action);

        // Setup authentication / authorization.
        AuthenticationBuilder authenticationBuilder
            = options.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

        var iamOptions = new IAMOptions(authenticationBuilder);

        // Configure UVault's IAM component.
        action.Invoke(iamOptions);

        // Add ASP.NET's services to support authorization.
        options.Services.AddAuthorization();
    }
}
