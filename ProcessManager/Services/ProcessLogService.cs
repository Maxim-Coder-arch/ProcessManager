using System.Globalization;
using System.Text;
using ProcessManager.Models;

namespace ProcessManager.Services;

public sealed class ProcessLogService
{
    private readonly string _logFilePath;

    public ProcessLogService()
    {
        _logFilePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "process-log.txt"
        );
    }

    public async Task SaveAsync(ProcessExecutionResult result)
    {
        string logEntry = BuildLogEntry(result);

        await File.AppendAllTextAsync(
            _logFilePath,
            logEntry,
            Encoding.UTF8
        );
    }

    private static string BuildLogEntry(ProcessExecutionResult result)
    {
        var logBuilder = new StringBuilder();

        logBuilder.AppendLine(
            $"[{result.StartedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}]"
        );

        logBuilder.AppendLine($"Program: {result.ProgramName}");
        logBuilder.AppendLine($"Arguments: {result.Arguments}");

        logBuilder.AppendLine(
            $"PID: {result.ProcessId?.ToString() ?? "N/A"}"
        );

        logBuilder.AppendLine(
            $"Duration: {result.Duration.ToString(@"hh\:mm\:ss\.fff")}"
        );

        logBuilder.AppendLine(
            $"ExitCode: {result.ExitCode?.ToString() ?? "N/A"}"
        );

        logBuilder.AppendLine(
            $"TimedOut: {(result.TimedOut ? "Yes" : "No")}"
        );

        logBuilder.AppendLine(
            $"Error: {result.Error ?? "None"}"
        );

        logBuilder.AppendLine("--------------------------------");

        return logBuilder.ToString();
    }
}
