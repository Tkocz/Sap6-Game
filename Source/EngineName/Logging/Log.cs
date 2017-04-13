namespace EngineName.Logging {

/*--------------------------------------
 * USINGS
 *------------------------------------*/

using System;
using System.IO;
using System.Runtime.CompilerServices;

/*--------------------------------------
 * CLASSES
 *------------------------------------*/

/// <summary>Provides basic logging functionality.</summary>
public class Log {
    /*--------------------------------------
     * NON-PUBLIC FIELDS
     *------------------------------------*/

    /// <summary>The name of the log.</summary>
    private readonly string mName;

    /// <summary>The file log, if file logging is enabled.</summary>
    private static StreamWriter sFile;

    /// <summary>The time that the <see cref="Log"/> class was
    ///          initialized.</summary>
    private static readonly DateTime sInitTime = DateTime.UtcNow;

    /*--------------------------------------
     * CONSTRUCTORS
     *------------------------------------*/

    /// <summary>Creates a new log with the specified name.</summary>
    /// <param name="name">The name of the log.</param>
    private Log(string name) {
        mName = name;
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
    /// <returns>A log.</returns>
    public static Log Get([CallerFilePath  ] string callerFilePath   = "",
                          [CallerMemberName] string callerMemberName = "",
                          [CallerLineNumber] int    callerLineNumber = 0)
    {
        return Get($"{Path.GetFileName(callerFilePath)}:{callerLineNumber}");
    }

    /// <summary>Writes a debug message to the log.</summary>
    /// <param name="message">The message to write to the log.</param>
    /// <param name="args">The message arguments.</param>
    /// <returns>The log instance.</returns>
    public Log Debug(string message, params object[] args) {
#if DEBUG
        var oldColor = Console.ForegroundColor;
        try {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Write("DBG", string.Format(message, args));
        }
        finally {
            Console.ForegroundColor = oldColor;
        }
#endif

        return this;
    }

    /// <summary>Writes an error message to the log.</summary>
    /// <param name="message">The message to write to the log.</param>
    /// <param name="args">The message arguments.</param>
    /// <returns>The log instance.</returns>
    public Log Err(string message, params object[] args) {
        var oldColor = Console.ForegroundColor;
        try {
            Console.ForegroundColor = ConsoleColor.Red;
            Write("ERR",  string.Format(message, args));
        }
        finally {
            Console.ForegroundColor = oldColor;
        }

        return this;
    }

    /// <summary>Writes an informational message to the log.</summary>
    /// <param name="message">The message to write to the log.</param>
    /// <param name="args">The message arguments.</param>
    /// <returns>The log instance.</returns>
    public Log Info(string message, params object[] args) {
        var oldColor = Console.ForegroundColor;
        try {
            Console.ForegroundColor = ConsoleColor.White;
            Write("NFO", string.Format(message, args));
        }
        finally {
            Console.ForegroundColor = oldColor;
        }

        return this;
    }

    /// <summary>Enables file logging.</summary>
    /// <param name="fileName">The name of the file to write to.</param>
    public static void ToFile(string fileName=null) {
        if (string.IsNullOrEmpty(fileName)) {
            fileName = AppDomain.CurrentDomain.FriendlyName;
            fileName = Path.GetFileNameWithoutExtension(fileName) + ".log";
        }

        sFile = new StreamWriter(fileName);
    }

    /// <summary>Writes a warning message to the log.</summary>
    /// <param name="message">The message to write to the log.</param>
    /// <param name="args">The message arguments.</param>
    /// <returns>The log instance.</returns>
    public Log Warn(string message, params object[] args) {
        var oldColor = Console.ForegroundColor;
        try {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Write("WRN", string.Format(message, args));
        }
        finally {
            Console.ForegroundColor = oldColor;
        }

        return this;
    }

    /*--------------------------------------
     * NON-PUBLIC METHODS
     *------------------------------------*/

    /// <summary>Writes a message to the log.</summary>
    /// <param name="type">The message type.</param>
    /// <param name="text">The text to write.</param>
    private void Write(string type, string text) {
        var t  = (DateTime.UtcNow - sInitTime).TotalSeconds;
        var s0 = string.Format("({0})", type);
        var s1 = string.Format("{0:0.000} {1,-5} {2}", t, s0, mName);
        var s2 = string.Format("{0,-32}: {1}", s1, text);

        Console.WriteLine(s2);

        if (sFile != null) {
            lock (sFile) {
                sFile.WriteLine(s2);
                sFile.Flush();
            }
        }
    }
}

}
