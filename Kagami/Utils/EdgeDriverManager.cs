using System;
using Kagami.Exceptions;
using OpenQA.Selenium.Edge;

namespace Kagami.Utils;

public static class EdgeDriverManager
{
    private static EdgeDriver EdgeDriver { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeout">minutes</param>
    /// <returns></returns>
    /// <exception cref="EdgeDriverBusyException"></exception>
    public static EdgeDriver GetEdgeDriver(int timeout = 1)
    {
        if (EdgeDriver is null)
        {
            var options = new EdgeOptions();
            options.AddArguments(
                // "--headless",
                "blink-settings=imagesEnabled=false",
                "--disable-blink-features=AutomationControlled");
            return EdgeDriver = new EdgeDriver(EdgeDriverService.CreateDefaultService(), options, new TimeSpan(0, timeout, 0));
        }

        throw new EdgeDriverBusyException("Only one EdgeDriver instance can exist at a time.");
    }

    public static void Quit()
    {
        if (EdgeDriver is null)
            return;
        EdgeDriver.Quit();
        EdgeDriver = null;
        GC.Collect();
    }
}