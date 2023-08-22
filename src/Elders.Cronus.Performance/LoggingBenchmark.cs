using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;

//BenchmarkRunner.Run<LoggingBench>();

[MemoryDiagnoser]
public class LoggingBench
{
    private readonly ILogger _logger;
    private string _name = "Batman";
    private int _age = 82;

    public LoggingBench()
    {
        _logger = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new MyLoggerProvider());
        }).CreateLogger(typeof(LoggingBench));
    }

    [Benchmark(Baseline = true)]
    public void Default() => Log.Log0(_logger, _name, _age);

    [Benchmark]
    public void SkipEnabledCheck() => Log.Log1(_logger, _name, _age);

    private static class Log
    {
        static LogDefineOptions logOptions = new LogDefineOptions()
        {
            SkipEnabledCheck = true
        };

        private static readonly Action<ILogger, string, int, Exception?> s_log0 = LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(1001),
            "Name {Name} is {Age} years old");

        private static readonly Action<ILogger, string, int, Exception?> s_log1 = LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(1001),
            "Name {Name} is {Age} years old",
            logOptions);

        public static void Log0(ILogger logger, string name, int age) => s_log0(logger, name, age, null);
        public static void Log1(ILogger logger, string name, int age) => s_log1(logger, name, age, null);
    }
}

public class MyLogger : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        => LogCore(formatter(state, exception));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void LogCore(string _) { }
}

public class MyLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new MyLogger();
    public void Dispose() { }
}
