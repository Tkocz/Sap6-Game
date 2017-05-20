namespace Thengill.Utils {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;
using System.Diagnostics;

using Logging;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

/// <summary>Provides functionality for performing atomic operations.</summary>
public static class DebugUtil {
    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Asserts that the expression evaluates to
    /// <see langword="true"/>. Otherwise, logs an error message and
    /// exits.</summary>
    /// <param name="expr">The expression to evaluate.</param>
    /// <param name="s">The error message to display if the assertion
    ///                 fails.</param>
    public static void Assert(bool expr, string s) {
        if (expr) {
            return;
        }

        var stackFrame = new StackFrame(1);
        var method     = stackFrame.GetMethod();
        var type       = method.DeclaringType;

        var logName = string.Format("{0}.{1}", type.Name, method.Name);
        Log.GetLog(logName).Err($"Assertion failed: {s}");

#if DEBUG
        if (Debugger.IsAttached) Debugger.Launch();
        else                     Environment.Exit(255);
#else
        Environment.Exit(255);
#endif

    }
}

}
