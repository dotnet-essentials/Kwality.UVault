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
namespace Kwality.UVault.IAM.QA;

using System.Diagnostics.CodeAnalysis;
using System.Net;

using AutoFixture.Xunit2;

using FluentAssertions;

using Kwality.UVault.Core.Extensions;
using Kwality.UVault.IAM.Extensions;
using Kwality.UVault.IAM.Internal.Validators;
using Kwality.UVault.IAM.QA.Internal.Factories;
using Kwality.UVault.IAM.QA.Internal.Validators;
using Kwality.UVault.QA.Common.System;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class IAMDefaultTests
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
    [Fact(DisplayName = "Request an HTTP endpoint succeeds (Default JWT validator).")]
    internal async Task RequestEndpoint_Succeeds()
    {
        // ARRANGE.
        (string validIssuer, string validAudience) jwtSettings = GetJwtSettings();

        // ACT / ASSERT.
        await new HttpRequestValidator
            {
                ConfigureServices = services =>
                {
                    // Add `UVault`.
                    services.AddUVault(options =>
                    {
                        // Use `UVault's` Identity & Access Management.
                        options.UseIAM(iamOptions =>
                            iamOptions.UseDefault(jwtSettings.validIssuer, jwtSettings.validAudience));
                    });
                },
                ConfigureApp = static app => app.UseUVault(null),
                ConfigureRoutes = static routes =>
                {
                    routes.MapGet(defaultRoute, static context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;

                        return Task.CompletedTask;
                    });
                },

                // EXPECTATIONS.
                ExpectedHttpStatusCode = HttpStatusCode.OK,
            }.SendHttpRequestAsync(defaultRoute)
             .ConfigureAwait(true);
    }

    [IAM]
    [Fact(DisplayName = "Request a secured HTTP endpoint without a `JWT` fails (Default JWT validator).")]
    internal async Task RequestSecuredEndpoint_NoJwt_Fails()
    {
        // ARRANGE.
        (string validIssuer, string validAudience) jwtSettings = GetJwtSettings();

        // ACT / ASSERT.
        await new HttpRequestValidator
            {
                ConfigureServices = services =>
                {
                    // Add `UVault`.
                    services.AddUVault(options =>
                    {
                        // Use `UVault's` Identity & Access Management.
                        options.UseIAM(iamOptions =>
                            iamOptions.UseDefault(jwtSettings.validIssuer, jwtSettings.validAudience));
                    });
                },
                ConfigureApp = static app => app.UseUVault(static options => options.UseIAM()),
                ConfigureRoutes = static routes =>
                {
                    routes.MapGet(defaultRoute,
                        [Authorize]
                        [ExcludeFromCodeCoverage(Justification = "The user is NOT allowed to visit this endpoint.")]
                        static (context) =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.OK;

                            return Task.CompletedTask;
                        });
                },
                ExpectedHttpStatusCode = HttpStatusCode.Unauthorized,
            }.SendHttpRequestAsync(defaultRoute)
             .ConfigureAwait(true);
    }

    [IAM]
    [Fact(DisplayName = "Request a secured HTTP endpoint with an empty `JWT` fails (Default JWT validator).")]
    internal async Task RequestSecuredEndpoint_EmptyJwt_Fails()
    {
        // ARRANGE.
        (string validIssuer, string validAudience) jwtSettings = GetJwtSettings();

        // ACT / ASSERT.
        await new HttpRequestValidator
            {
                ConfigureServices = services =>
                {
                    // Add `UVault`.
                    services.AddUVault(options =>
                    {
                        // Use `UVault's` Identity & Access Management.
                        options.UseIAM(iamOptions =>
                            iamOptions.UseDefault(jwtSettings.validIssuer, jwtSettings.validAudience));
                    });
                },
                ConfigureApp = static app => app.UseUVault(static options => options.UseIAM()),
                ConfigureRoutes = static routes =>
                {
                    routes.MapGet(defaultRoute,
                        [ExcludeFromCodeCoverage(Justification = "The user is NOT allowed to visit this endpoint.")]
                        [Authorize]
                        static (context) =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.OK;

                            return Task.CompletedTask;
                        });
                },
                Jwt = string.Empty,
                ExpectedHttpStatusCode = HttpStatusCode.Unauthorized,
            }.SendHttpRequestAsync(defaultRoute)
             .ConfigureAwait(true);
    }

    [IAM]
    [Auth0]
    [Fact(DisplayName = "Request a secured HTTP endpoint with a valid `JWT` succeeds (Default JWT validator).")]
    internal async Task RequestSecuredEndpoint_ValidJwt_Succeeds()
    {
        // ARRANGE.
        (string validIssuer, string validAudience) jwtSettings = GetJwtSettings();

        string jwtToken = await Auth0AuthenticationFactory.RequestAsync()
                                                          .ConfigureAwait(true);

        // ACT / ASSERT.
        await new HttpRequestValidator
            {
                ConfigureServices = services =>
                {
                    // Add `UVault`.
                    services.AddUVault(options =>
                    {
                        // Use `UVault's` Identity & Access Management.
                        options.UseIAM(iamOptions =>
                            iamOptions.UseDefault(jwtSettings.validIssuer, jwtSettings.validAudience));
                    });
                },
                ConfigureApp = static app => app.UseUVault(static options => options.UseIAM()),
                ConfigureRoutes = static routes =>
                {
                    routes.MapGet(defaultRoute, [Authorize] static (context) =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;

                        return Task.CompletedTask;
                    });
                },
                Jwt = jwtToken,
                ExpectedHttpStatusCode = HttpStatusCode.OK,
            }.SendHttpRequestAsync(defaultRoute)
             .ConfigureAwait(true);
    }

    [IAM]
    [AutoData]
    [Theory(DisplayName = "The default JWT Validator sets the JwtBearerOptions `Authority`.")]
    internal void Authority_Matches_ValidIssuer(string validIssuer)
    {
        // ARRANGE.
        var jwtValidator = new JwtValidator(validIssuer, "Audience");
        var jwtBearerOptions = new JwtBearerOptions();

        // ACT.
        jwtValidator.Options(jwtBearerOptions);

        // ASSERT.
        jwtBearerOptions.Authority.Should()
                        .Be(validIssuer);
    }

    [IAM]
    [Fact(DisplayName = "The default JWT Validator sets the JwtBearerOptions `Validate Issuer`.")]
    internal void ValidateIssuer_Matches_True()
    {
        // ARRANGE.
        var jwtValidator = new JwtValidator("Issuer", "Audience");
        var jwtBearerOptions = new JwtBearerOptions();

        // ACT.
        jwtValidator.Options(jwtBearerOptions);

        // ASSERT.
        jwtBearerOptions.TokenValidationParameters.ValidateIssuer.Should()
                        .BeTrue();
    }

    [IAM]
    [Fact(DisplayName = "The default JWT Validator sets the JwtBearerOptions `Validate Audience`.")]
    internal void ValidateAudience_Matches_True()
    {
        // ARRANGE.
        var jwtValidator = new JwtValidator("Issuer", "Audience");
        var jwtBearerOptions = new JwtBearerOptions();

        // ACT.
        jwtValidator.Options(jwtBearerOptions);

        // ASSERT.
        jwtBearerOptions.TokenValidationParameters.ValidateAudience.Should()
                        .BeTrue();
    }

    [IAM]
    [Fact(DisplayName = "The default JWT Validator sets the JwtBearerOptions `Validate Lifetime`.")]
    internal void ValidateLifetime_Matches_True()
    {
        // ARRANGE.
        var jwtValidator = new JwtValidator("Issuer", "Audience");
        var jwtBearerOptions = new JwtBearerOptions();

        // ACT.
        jwtValidator.Options(jwtBearerOptions);

        // ASSERT.
        jwtBearerOptions.TokenValidationParameters.ValidateLifetime.Should()
                        .BeTrue();
    }

    [IAM]
    [Fact(DisplayName = "The default JWT Validator sets the JwtBearerOptions `Validate Issuer Signing Key`.")]
    internal void ValidateIssuerSigningKey_Matches_True()
    {
        // ARRANGE.
        var jwtValidator = new JwtValidator("Issuer", "Audience");
        var jwtBearerOptions = new JwtBearerOptions();

        // ACT.
        jwtValidator.Options(jwtBearerOptions);

        // ASSERT.
        jwtBearerOptions.TokenValidationParameters.ValidateIssuerSigningKey.Should()
                        .BeTrue();
    }

    [IAM]
    [AutoData]
    [Theory(DisplayName = "The default JWT Validator sets the JwtBearerOptions `Issuer`.")]
    internal void ValidIssuer_Matches_ValidIssuer(string validIssuer)
    {
        // ARRANGE.
        var jwtValidator = new JwtValidator(validIssuer, "Audience");
        var jwtBearerOptions = new JwtBearerOptions();

        // ACT.
        jwtValidator.Options(jwtBearerOptions);

        // ASSERT.
        jwtBearerOptions.TokenValidationParameters.ValidIssuer.Should()
                        .Be(validIssuer);
    }

    [IAM]
    [AutoData]
    [Theory(DisplayName = "The default JWT Validator sets the JwtBearerOptions `Audience`.")]
    internal void ValidAudience_Matches_ValidAudience(string validAudience)
    {
        // ARRANGE.
        var jwtValidator = new JwtValidator("Issuer", validAudience);
        var jwtBearerOptions = new JwtBearerOptions();

        // ACT.
        jwtValidator.Options(jwtBearerOptions);

        // ASSERT.
        jwtBearerOptions.TokenValidationParameters.ValidAudience.Should()
                        .Be(validAudience);
    }

    private static (string validIssuer, string validAudience) GetJwtSettings()
    {
        return (Environment.AUTH0_VALID_ISSUER, Environment.AUTH0_VALID_AUDIENCE);
    }
}
