// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace Kagami.Test;

[TestClass]
public class BuildStampTest
{
    [TestMethod("BuildStamp验证")]
    public void GenerateBuildStampTest()
    {
        Dictionary<string, string?>? list = typeof(Entry)
            .Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .ToDictionary(
                i => i.Key,
                i => i.Value);

        Assert.IsTrue(list.Count is not 0);
        Console.WriteLine(list["Branch"]);
        Console.WriteLine(list["Revision"]);
        Console.WriteLine(list["BuildTime"]);
    }
}