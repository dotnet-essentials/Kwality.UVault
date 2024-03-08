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
namespace Kwality.UVault.QA.Common.System;

using global::System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage(Justification = "Helper function.")]
public static class Environment
{
    private static string ReadString(string key)
    {
        string value = global::System.Environment.GetEnvironmentVariable(key) ?? string.Empty;

        if (string.IsNullOrEmpty(value))
        {
            throw new NotSupportedException($"No value specified for the environment variable `{key}`.");
        }

        return value;
    }
#pragma warning disable CA1707
    // ReSharper disable once InconsistentNaming
    public static string AUTH0_TOKEN_ENDPOINT => ReadString("AUTH0_TOKEN_ENDPOINT");

    // ReSharper disable once InconsistentNaming
    public static string AUTH0_USERNAME => ReadString("AUTH0_USERNAME");

    // ReSharper disable once InconsistentNaming
    public static string AUTH0_PASSWORD => ReadString("AUTH0_PASSWORD");

    // ReSharper disable once InconsistentNaming
    public static string AUTH0_AUDIENCE => ReadString("AUTH0_AUDIENCE");

    // ReSharper disable once InconsistentNaming
    public static string AUTH0_VALID_AUDIENCE => ReadString("AUTH0_VALID_AUDIENCE");

    // ReSharper disable once InconsistentNaming
    public static string AUTH0_VALID_ISSUER => ReadString("AUTH0_VALID_ISSUER");

    // ReSharper disable once InconsistentNaming
    public static string AUTH0_CLIENT_ID => ReadString("AUTH0_CLIENT_ID");

    // ReSharper disable once InconsistentNaming
    public static string AUTH0_CLIENT_SECRET => ReadString("AUTH0_CLIENT_SECRET");

    // ReSharper disable once InconsistentNaming
    public static string AUTH0_TEST_APPLICATION_1_CLIENT_ID => ReadString("AUTH0_TEST_APPLICATION_1_CLIENT_ID");

    // ReSharper disable once InconsistentNaming
    public static string AUTH0_TEST_APPLICATION_2_CLIENT_ID => ReadString("AUTH0_TEST_APPLICATION_2_CLIENT_ID");
#pragma warning restore CA1707
}
