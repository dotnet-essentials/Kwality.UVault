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
namespace Kwality.UVault.User.Management.Auth0.Extensions;

using JetBrains.Annotations;

using Kwality.UVault.User.Management.Auth0.Configuration;
using Kwality.UVault.User.Management.Auth0.Internal.API.Clients;
using Kwality.UVault.User.Management.Auth0.Internal.Stores;
using Kwality.UVault.User.Management.Auth0.Keys;
using Kwality.UVault.User.Management.Auth0.Mapping.Abstractions;
using Kwality.UVault.User.Management.Auth0.Models;
using Kwality.UVault.User.Management.Options;

using Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class UserManagementOptionsExtensions
{
    public static void UseAuth0Store<TModel, TMapper>(
        this UserManagementOptions<TModel, StringKey> options, ApiConfiguration configuration)
        where TModel : UserModel
        where TMapper : class, IModelMapper<TModel>
    {
        options.UseStore<UserStore<TModel>>();

        // Register additional services.
        options.ServiceCollection.AddScoped<IModelMapper<TModel>, TMapper>();
        options.ServiceCollection.AddHttpClient<ManagementClient>();
        options.ServiceCollection.AddSingleton(configuration);
    }
}
