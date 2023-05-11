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
namespace Kwality.UVault.QA.IAM;

using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

using FluentAssertions;

using Kwality.UVault.Extensions;
using Kwality.UVault.IAM.Extensions;
using Kwality.UVault.IAM.Validators.Abstractions;
using Kwality.UVault.QA.Internal.Validators;
using Kwality.UVault.QA.Internal.Xunit.Traits;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Xunit;

// ReSharper disable once InconsistentNaming
// ReSharper disable once MemberCanBeFileLocal
public sealed class IAMTests
{
    private const string defaultRoute = "/";

    [IAM]
    [Fact(DisplayName = "Use IAM without options raises an exception.")]
    internal void NoIAMOptions_RaisesException()
    {
        // ARRANGE.
        var serviceCollection = new ServiceCollection();

        // ACT.
        Action act = () => serviceCollection.AddUVault(static options => options.UseIAM(null));

        // ASSERT.
        act.Should()
           .Throw<ArgumentNullException>();
    }

    [IAM]
    [Fact(DisplayName = "Request an HTTP endpoint succeeds.")]
    internal async Task NoIAM_RequestEndpoint_Succeeds()
    {
        // ACT / ASSERT.
        await new HttpRequestValidator
            {
                ConfigureServices = static services =>
                {
                    services.AddUVault(null);
                },
                ConfigureApp = static app => app.UseUVault(),
                ConfigureRoutes = static routes =>
                {
                    routes.MapGet(
                        defaultRoute, static context =>
                        {
                            context.Response.StatusCode = 200;

                            return Task.CompletedTask;
                        });
                },

                // EXPECTATIONS.
                ExpectedHttpStatusCode = HttpStatusCode.OK,
            }.SendHttpRequestAsync(defaultRoute)
             .ConfigureAwait(false);
    }

    [IAM]
    [Fact(DisplayName = "Request a secured HTTP endpoint without a `JWT` fails.")]
    internal async Task RequestSecuredEndpoint_NoJwt_Fails()
    {
        // ACT / ASSERT.
        await new HttpRequestValidator
            {
                ConfigureServices = static services =>
                {
                    // Add `UVault`.
                    services.AddUVault(
                        static options =>
                        {
                            // Use `UVault's` Identity & Access Management.
                            options.UseIAM(static (_, options) => options.UseJwtValidator<JwtValidator>());
                        });
                },
                ConfigureApp = static app => app.UseUVault(static options => options.UseIAM()),
                ConfigureRoutes = static routes =>
                {
                    routes.MapGet(
                        defaultRoute,
                        [Authorize]
                        [ExcludeFromCodeCoverage(Justification = "The user is NOT allowed to visit this endpoint.")]
                        static (context) =>
                        {
                            context.Response.StatusCode = 200;

                            return Task.CompletedTask;
                        });
                },
                ExpectedHttpStatusCode = HttpStatusCode.Unauthorized,
            }.SendHttpRequestAsync(defaultRoute)
             .ConfigureAwait(false);
    }

    [IAM]
    [Fact(DisplayName = "Request a secured HTTP endpoint with an empty `JWT` fails.")]
    internal async Task RequestSecuredEndpoint_EmptyJwt_Fails()
    {
        // ACT / ASSERT.
        await new HttpRequestValidator
            {
                ConfigureServices = static services =>
                {
                    // Add `UVault`.
                    services.AddUVault(
                        static options =>
                        {
                            // Use `UVault's` Identity & Access Management.
                            options.UseIAM(static (_, options) => options.UseJwtValidator<JwtValidator>());
                        });
                },
                ConfigureApp = static app => app.UseUVault(static options => options.UseIAM()),
                ConfigureRoutes = static routes =>
                {
                    routes.MapGet(
                        defaultRoute,
                        [ExcludeFromCodeCoverage(Justification = "The user is NOT allowed to visit this endpoint.")]
                        [Authorize]
                        static (context) =>
                        {
                            context.Response.StatusCode = 200;

                            return Task.CompletedTask;
                        });
                },
                Jwt = string.Empty,
                ExpectedHttpStatusCode = HttpStatusCode.Unauthorized,
            }.SendHttpRequestAsync(defaultRoute)
             .ConfigureAwait(false);
    }

    [IAM]
    [Fact(DisplayName = "Request a secured HTTP endpoint with a valid `JWT` succeeds.")]
    internal async Task RequestSecuredEndpoint_ValidJwt_Succeeds()
    {
        // ACT / ASSERT.
        await new HttpRequestValidator
            {
                ConfigureServices = static services =>
                {
                    // Add `UVault`.
                    services.AddUVault(
                        static options =>
                        {
                            // Use `UVault's` Identity & Access Management.
                            options.UseIAM(static (_, options) => options.UseJwtValidator<JwtValidator>());
                        });
                },
                ConfigureApp = static app => app.UseUVault(static options => options.UseIAM()),
                ConfigureRoutes = static routes =>
                {
                    routes.MapGet(
                        defaultRoute, [Authorize] static (context) =>
                        {
                            context.Response.StatusCode = 200;

                            return Task.CompletedTask;
                        });
                },
                Jwt = JwtValidator.ValidJwt,
                ExpectedHttpStatusCode = HttpStatusCode.OK,
            }.SendHttpRequestAsync(defaultRoute)
             .ConfigureAwait(false);
    }

    private sealed class JwtValidator : IJwtValidator
    {
        private const string jwtHeader = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9";
        private const string jwtPayload = "eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ";
        private const string jwtSignature = "T7sFpO0XoaJ9JWsu2J1ormK99zs4zIr2s25jjl8RVSw";
        public const string ValidJwt = $"{jwtHeader}.{jwtPayload}.{jwtSignature}";

        public Action<JwtBearerOptions> Options
            => static options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
#pragma warning disable CA5404 // "Do not disable token validation checks".
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
#pragma warning restore CA5404
                    ValidateIssuerSigningKey = false,
                    RequireSignedTokens = false,
                    SignatureValidator = static (token, _) => new JwtSecurityToken(token),
                };
            };
    }
}
