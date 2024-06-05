using System.Runtime.CompilerServices;

namespace legallead.reader.component
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods",
        Justification = "Following convention from remote package.")]
    public interface ILoggingRepository
    {
        string ClassContext { get; set; }
        Task LogCritical(string message, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerMethodName = "");


        Task LogDebug(string message, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerMethodName = "");

        Task LogError(Exception exception, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerMethodName = "");
        Task LogError(Exception exception, string namespaceName, string className, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerMethodName = "");
        Task LogInformation(string message, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerMethodName = "");
        Task LogInformation(string message, string namespaceName, string className, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerMethodName = "");
        Task LogVerbose(string message, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerMethodName = "");

        Task LogWarning(string message, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerMethodName = "");
    }
}
