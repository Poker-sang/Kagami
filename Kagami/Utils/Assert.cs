using System.Diagnostics.CodeAnalysis;

namespace Kagami.Utils;

/// <summary>
/// 断言工具
/// </summary>
internal static class Assert
{
    /// <summary>
    /// 当不满足条件时抛出指定类型的异常
    /// </summary>
    /// <typeparam name="TException">异常类型</typeparam>
    /// <param name="condition">条件</param>
    /// <param name="args">参数</param>
    /// <exception cref="TException">当不满足条件时抛出</exception>
    public static void ThrowIfNot<TException>(bool condition, params object[] args)
        where TException : Exception, new()
    {
        if (!condition)
            throw (Exception)Activator.CreateInstance(typeof(TException), args)!;
    }

    /// <summary>
    /// 当满足条件时抛出指定类型的异常
    /// </summary>
    /// <typeparam name="TException">异常类型</typeparam>
    /// <param name="condition">条件</param>
    /// <param name="args">参数</param>
    /// <exception cref="TException">当满足条件时抛出</exception>
    public static void ThrowIf<TException>(bool condition, params object[] args)
        where TException : Exception, new()
    {
        if (condition)
            throw (Exception)Activator.CreateInstance(typeof(TException), args)!;
    }

    /// <summary>
    /// 当对象为null时抛出指定类型的异常
    /// </summary>
    /// <typeparam name="TException">异常类型</typeparam>
    /// <param name="obj">对象</param>
    /// <param name="args">参数</param>
    /// <exception cref="TException">当对象为null时抛出</exception>
    public static void IsNotNull<TException>([NotNull] object? obj, params object[] args)
        where TException : Exception, new()
    {
        if (obj is null)
            throw (Exception)Activator.CreateInstance(typeof(TException), args)!;
    }
}
