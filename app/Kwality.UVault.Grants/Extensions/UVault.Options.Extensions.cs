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
namespace Kwality.UVault.Grants.Extensions;

using JetBrains.Annotations;

using Kwality.UVault.Core.Options;
using Kwality.UVault.Grants.Internal.Stores;
using Kwality.UVault.Grants.Managers;
using Kwality.UVault.Grants.Models;
using Kwality.UVault.Grants.Options;
using Kwality.UVault.Grants.Stores.Abstractions;

using Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class UVaultOptionsExtensions
{
    public static void UseGrantManagement<TModel, TKey>(this UVaultOptions options)
        where TModel : GrantModel<TKey>
        where TKey : IEquatable<TKey>
    {
        options.UseGrantManagement<TModel, TKey>(null);
    }

    public static void UseGrantManagement<TModel, TKey>(this UVaultOptions options, ServiceLifetime storeLifetime)
        where TModel : GrantModel<TKey>
        where TKey : IEquatable<TKey>
    {
        options.UseGrantManagement<TModel, TKey>(null, storeLifetime);
    }

    public static void UseGrantManagement<TModel, TKey>(
        this UVaultOptions options, Action<GrantManagementOptions<TModel, TKey>>? action,
        ServiceLifetime storeLifetime = ServiceLifetime.Scoped)
        where TModel : GrantModel<TKey>
        where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Services.AddScoped<GrantManager<TModel, TKey>>();

        switch (storeLifetime)
        {
            case ServiceLifetime.Singleton:
                options.Services.AddSingleton<IGrantStore<TModel, TKey>, StaticStore<TModel, TKey>>();

                break;

            case ServiceLifetime.Scoped:
                options.Services.AddScoped<IGrantStore<TModel, TKey>, StaticStore<TModel, TKey>>();

                break;

            case ServiceLifetime.Transient:
                options.Services.AddTransient<IGrantStore<TModel, TKey>, StaticStore<TModel, TKey>>();

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(storeLifetime), storeLifetime, null);
        }

        // Configure UVault's User Management component.
        action?.Invoke(new GrantManagementOptions<TModel, TKey>(options.Services));
    }
}
