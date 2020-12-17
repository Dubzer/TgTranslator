using Serilog;
using Serilog.Filters;

namespace TgTranslator.Extensions
{
    public static class SerilogExtensions
    {
        /// <summary>
        /// Ignore log events by source
        /// </summary>
        public static LoggerConfiguration Ignore(this LoggerConfiguration config, string[] sources)
        {
            foreach (var source in sources)
            {
                config.Filter.ByExcluding(Matching.FromSource(source));
            }

            return config;
        }
    }
}