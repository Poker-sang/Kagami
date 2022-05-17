using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kagami.Test;

[TestClass]
public class BuildStampTest
{
    [TestMethod("BuildStamp验证")]
    public void GenerateBuildStampTest()
    {
        var list = typeof(Entry)
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
