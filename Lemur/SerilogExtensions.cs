using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Lemur;

public static class SerilogExtensions
{
    public static LoggerConfiguration Override<T>(this LoggerMinimumLevelConfiguration conf, LogEventLevel minimumLevel)
    {
        return conf.Override(typeof(T).FullName!, minimumLevel);
    }
}
