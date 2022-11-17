// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Kagami.Services;

namespace Kagami.Test.Services;

[TestClass]
public class MemeTest
{
    [TestMethod]
    public async Task TestGetMemeImageSourcesAsync()
    {
        if (await Meme.GetMemeImageSourcesAsync(122) is { } list)
        {
            Console.WriteLine("Images: [\n  ");
            Console.WriteLine(string.Join(",\n  ", list));
            Console.WriteLine("]");
        }
        else 
            Console.WriteLine("Wrong issue");
    }
}
