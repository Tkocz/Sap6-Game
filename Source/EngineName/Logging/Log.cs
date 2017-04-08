namespace EngineName.Logging {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;
using System.Diagnostics;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

/// <summary>Provides basic logging functionality.</summary>
public class Log {
    /*--------------------------------------
     * NON-PUBLIC FIELDS
     *------------------------------------*/

    /// <summary>The name of the log.</summary>
    private readonly string m_Name;

    /// <summary>The time that the <see cref="Log"/> class was
    //           initialized.</summary>
    private static readonly DateTime s_InitTime = DateTime.UtcNow;

    /*--------------------------------------
     * CONSTRUCTORS
     *------------------------------------*/

    /// <summary>Creates a new log with the specified name.</summary>
    /// <param name="name">The name of the log.</param>
    private Log(string name) {
        m_Name = name;
    }

    /*--------------------------------------
     * PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Gets a log with the specified name.</summary>
    /// <param name="name">The name of the log.</param>
    /// <returns>A log.</returns>
    public static Log Get(string name) {
        return new Log(name);
    }

    /// <summary>Gets a log for the calling method.</summary>
    /// <param name="name">The name of the log.</param>
    /// <returns>A log.</returns>
    public static Log Get() {
        var stackFrame = new StackFrame(1);
        var method     = stackFrame.GetMethod();
        var type       = method.DeclaringType;

        return Get(string.Format("{0}.{1}", type.Name, method.Name));
    }

    /// <summary>Writes a debug message to the log.</summary>
    /// <param name="message">The message to write to the log.</param>
    /// <param name="args">The message arguments.</param>
    [Conditional("DEBUG")]
    public void Debug(string message, params object[] args) {
        var oldColor = Console.ForegroundColor;
        try {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Write("DBG", string.Format(message, args));
        }
        finally {
            Console.ForegroundColor = oldColor;
        }
    }

    /// <summary>Writes an error message to the log.</summary>
    /// <param name="message">The message to write to the log.</param>
    /// <param name="args">The message arguments.</param>
    public void Err(string message, params object[] args) {
        var oldColor = Console.ForegroundColor;
        try {
            Console.ForegroundColor = ConsoleColor.Red;
            Write("ERR",  string.Format(message, args));
        }
        finally {
            Console.ForegroundColor = oldColor;
        }
    }

    /// <summary>Writes an informational message to the log.</summary>
    /// <param name="message">The message to write to the log.</param>
    /// <param name="args">The message arguments.</param>
    public void Info(string message, params object[] args) {
        var oldColor = Console.ForegroundColor;
        try {
            Console.ForegroundColor = ConsoleColor.White;
            Write("NFO", string.Format(message, args));
        }
        finally {
            Console.ForegroundColor = oldColor;
        }
    }

    /// <summary>Writes a warning message to the log.</summary>
    /// <param name="message">The message to write to the log.</param>
    /// <param name="args">The message arguments.</param>
    public void Warn(string message, params object[] args) {
        var oldColor = Console.ForegroundColor;
        try {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Write("WRN", string.Format(message, args));
        }
        finally {
            Console.ForegroundColor = oldColor;
        }
    }

    /*--------------------------------------
     * NON-PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Writes a message to the log.</summary>
    /// <param name="type">The message type.</param>
    /// <param name="text">The text to write.</param>
    private void Write(string type, string text) {
        var t  = (DateTime.UtcNow - s_InitTime).TotalSeconds;
        var s0 = string.Format("({0})", type);
        var s1 = string.Format("[{0:0.000}] {1,-5} {2}()", t, s0, m_Name);
        Console.WriteLine(string.Format("{0}: {1}", s1, text));
    }
}

}
