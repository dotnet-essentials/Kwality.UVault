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
namespace Kwality.UVault.Users.Options;

using JetBrains.Annotations;

using Kwality.UVault.Users.Models;
using Kwality.UVault.Users.Stores.Abstractions;

using Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public sealed class UserManagementOptions<TModel, TKey>
    where TModel : UserModel<TKey>
    where TKey : IEqualityComparer<TKey>
{
    internal UserManagementOptions(IServiceCollection serviceCollection)
    {
        this.ServiceCollection = serviceCollection;
    }

    public IServiceCollection ServiceCollection { get; }
#pragma warning disable S4018
    public void UseStore<TStore>()
#pragma warning restore S4018
        where TStore : class, IUserStore<TModel, TKey>
    {
        this.ServiceCollection.AddScoped<IUserStore<TModel, TKey>, TStore>();
    }

#pragma warning disable S4018
    public void UseStore<TStore>(ServiceLifetime serviceLifetime)
#pragma warning restore S4018
        where TStore : class, IUserStore<TModel, TKey>
    {
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                this.ServiceCollection.AddSingleton<IUserStore<TModel, TKey>, TStore>();

                break;

            case ServiceLifetime.Scoped:
                this.ServiceCollection.AddScoped<IUserStore<TModel, TKey>, TStore>();

                break;

            case ServiceLifetime.Transient:
                this.ServiceCollection.AddTransient<IUserStore<TModel, TKey>, TStore>();

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
        }
    }
}
