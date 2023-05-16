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
namespace Kwality.UVault.QA.Users;

using System.Net;

using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

using FluentAssertions;

using Kwality.UVault.Auth0.Configuration;
using Kwality.UVault.Auth0.Exceptions;
using Kwality.UVault.Auth0.Internal.API.Clients;
using Kwality.UVault.QA.Internal.Extensions;
using Kwality.UVault.QA.Internal.Xunit.Traits;

using Moq;

using Xunit;

// ReSharper disable once MemberCanBeFileLocal
[Collection("Auth0")]
public sealed class Auth0ManagementClientTests
{
    private const string testPrefix = "Request an API management token";
    private readonly ApiConfiguration apiConfiguration;

    public Auth0ManagementClientTests()
    {
        this.apiConfiguration = new ApiConfiguration(
            new Uri("http://localhost/"), string.Empty, string.Empty, string.Empty);
    }

    [UserManagement]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} fails when the REST endpoint can't be requested.")]
    internal async Task RequestToken_RequestFails_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient)
    {
        // MOCK SETUP.
        messageHandler.SetupSendAsyncException();

        // ACT.
        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(false);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ManagementApiException>()
                 .WithMessage("Failed to retrieve an Auth0 `User Management` token.")
                 .ConfigureAwait(false);
    }

    [UserManagement]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} fails when the result differs from HTTP OK (NO Content).")]
    internal async Task RequestToken_ResponseNoOkWithoutContent_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient)
    {
        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent(string.Empty),
        };

        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);

        // ACT.
        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(false);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ManagementApiException>()
                 .WithMessage("Failed to retrieve an Auth0 `User Management` token. HTTP 400.")
                 .ConfigureAwait(false);
    }

    [UserManagement]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} fails when the result differs from HTTP OK.")]
    internal async Task RequestToken_ResponseNoOk_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient, string response)
    {
        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Content = new StringContent(response),
        };

        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);

        // ACT.
        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(false);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ManagementApiException>()
                 .WithMessage($"Failed to retrieve an Auth0 `User Management` token. HTTP 401: `{response}`.")
                 .ConfigureAwait(false);
    }

    [UserManagement]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} fails when the result is NOT valid JSON.")]
    internal async Task RequestToken_ResponseNoValidJson_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient)
    {
        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("Invalid data."),
        };

        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);

        // ACT.
        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(false);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ManagementApiException>()
                 .WithMessage("Failed to retrieve an Auth0 `User Management` token. Reason: Invalid HTTP response.")
                 .ConfigureAwait(false);
    }

    [UserManagement]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} fails when the result does NOT contain an access token.")]
    internal async Task RequestToken_ResponseNoAccessToken_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient)
    {
        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{}"),
        };

        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);

        // ACT.
        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(false);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ManagementApiException>()
                 .WithMessage("The `API Management Token / Access Token` token is `null`.")
                 .ConfigureAwait(false);
    }

    [UserManagement]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} fails when the result does contain an empty access token.")]
    internal async Task RequestToken_ResponseEmptyAccessToken_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient)
    {
        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"access_token\":\"\"}"),
        };

        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);

        // ACT.
        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(false);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ManagementApiException>()
                 .WithMessage("The `API Management Token / Access Token` token is `null`.")
                 .ConfigureAwait(false);
    }

    [UserManagement]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} returns the access token.")]
    internal async Task RequestToken_ReturnsAccessToken(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient, string apiToken)
    {
        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"access_token\":\"" + apiToken + "\"}"),
        };

        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);

        // ACT.
        string result = await managementClient.GetTokenAsync(this.apiConfiguration)
                                              .ConfigureAwait(false);

        // ASSERT.
        result.Should()
              .Be(apiToken);
    }

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class AutoDomainDataAttribute : AutoDataAttribute
    {
        public AutoDomainDataAttribute()
            : base(
                static () =>
                {
                    var fixture = new Fixture();
                    fixture.Customize(new AutoMoqCustomization());

                    fixture.Customize<HttpClient>(
                        static composer => composer
                                           .FromFactory(static (HttpMessageHandler handler) => new HttpClient(handler))
                                           .OmitAutoProperties());

                    return fixture;
                })
        {
        }
    }
}
