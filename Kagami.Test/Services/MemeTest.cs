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
        return;
        var list = await Meme.GetMemeImageSourcesAsync("二百四十四");
        Console.WriteLine("Images: [\n  ");
        Console.WriteLine(string.Join(",\n  ", list));
        Console.WriteLine("]");
    }
}
