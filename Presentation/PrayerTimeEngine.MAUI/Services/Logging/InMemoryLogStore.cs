using Microsoft.Extensions.Logging;

namespace PrayerTimeEngine.Services.Logging;

/// <summary>A single log entry as it is shown on the logs page.</summary>
public sealed record LogEntry(
    DateTime Timestamp,
    LogLevel Level,
    string Category,
    string Message,
    Exception? Exception)
{
    /// <summary>The complete text of the entry (also what the text filter searches through).</summary>
    public string FullText { get; } =
        Exception is null
            ? $"{Timestamp:HH:mm:ss.fff} {Level} {Category}{System.Environment.NewLine}{Message}"
            : $"{Timestamp:HH:mm:ss.fff} {Level} {Category}{System.Environment.NewLine}{Message}{System.Environment.NewLine}{Exception}";
}

/// <summary>
/// Keeps the most recent log entries of the running app in memory for the logs page. The log files
/// (which can be shared) are written separately by MetroLog and keep entries of the last days as well.
/// </summary>
public sealed class InMemoryLogStore
{
    public const int CAPACITY = 5000;

    public static InMemoryLogStore Instance { get; } = new();

    private readonly Queue<LogEntry> _entries = new(CAPACITY);
    private readonly Lock _lock = new();

    public void Add(LogEntry entry)
    {
        lock (_lock)
        {
            if (_entries.Count == CAPACITY)
            {
                _entries.Dequeue();
            }

            _entries.Enqueue(entry);
        }
    }

    /// <summary>All entries, the oldest first.</summary>
    public List<LogEntry> GetEntries()
    {
        lock (_lock)
        {
            return [.. _entries];
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }
}

/// <summary>Writes the log entries into the <see cref="InMemoryLogStore"/>.</summary>
public sealed class InMemoryLogStoreLoggerProvider(InMemoryLogStore store) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new InMemoryLogStoreLogger(store, categoryName);

    public void Dispose()
    {
    }

    private sealed class InMemoryLogStoreLogger(InMemoryLogStore store, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        // which levels are written is decided by the logging configuration (see AppLogLevel)
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            store.Add(new LogEntry(DateTime.Now, logLevel, category, formatter(state, exception), exception));
        }
    }
}
