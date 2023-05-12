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
namespace Kwality.UVault.QA.Internal.Factories;

using Kwality.UVault.Extensions;
using Kwality.UVault.User.Management.Extensions;
using Kwality.UVault.User.Management.Managers;
using Kwality.UVault.User.Management.Models;
using Kwality.UVault.User.Management.Options;

using Microsoft.Extensions.DependencyInjection;

internal sealed class UserManagerFactory
{
    private readonly IServiceCollection serviceCollection;

    public UserManagerFactory()
    {
        this.serviceCollection = new ServiceCollection();
    }

    public UserManager<TModel, TKey> Create<TModel, TKey>()
        where TModel : UserModel<TKey>
        where TKey : IEqualityComparer<TKey>
    {
        return this.Create<TModel, TKey>(null);
    }

    public UserManager<TModel, TKey> Create<TModel, TKey>(Action<UserManagementOptions<TModel, TKey>>? optionsAction)
        where TModel : UserModel<TKey>
        where TKey : IEqualityComparer<TKey>
    {
        this.serviceCollection.AddUVault(options => options.UseUserManagement(optionsAction));

        return this.serviceCollection.BuildServiceProvider()
                   .GetRequiredService<UserManager<TModel, TKey>>();
    }
}
