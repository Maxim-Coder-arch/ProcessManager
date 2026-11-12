using System.Diagnostics;
using ProcessManager.Models;

namespace ProcessManager.Services;

public sealed class ProcessRunner
{
    public async Task<ProcessExecutionResult> RunAsync(
        ProcessExecutionRequest request)
    {
        var startedAt = DateTimeOffset.Now;
        var stopwatch = Stopwatch.StartNew();

        int? processId = null;
        int? exitCode = null;

        string standardOutput = string.Empty;
        string standardError = string.Empty;
        string? error = null;

        bool timedOut = false;
        bool processStarted = false;

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = request.ProgramName,
                Arguments = request.Arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        Task<string>? outputTask = null;
        Task<string>? errorTask = null;

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException(
                    "Не удалось запустить процесс."
                );
            }

            processStarted = true;
            processId = process.Id;

            outputTask = process.StandardOutput.ReadToEndAsync();
            errorTask = process.StandardError.ReadToEndAsync();

            Task exitTask = process.WaitForExitAsync();
            Task timeoutTask = Task.Delay(request.Timeout);

            Task completedTask = await Task.WhenAny(
                exitTask,
                timeoutTask
            );

            if (completedTask != exitTask && !process.HasExited)
            {
                timedOut = true;

                error = $"Превышено время выполнения: {request.Timeout}.";

                process.Kill(entireProcessTree: true);
            }

            await exitTask;

            exitCode = process.ExitCode;

            standardOutput = await outputTask;
            standardError = await errorTask;
        }

        catch (Exception exception)
        {
            error = string.IsNullOrWhiteSpace(error)
                ? exception.Message
                : $"{error}{Environment.NewLine}{exception.Message}";

            if (processStarted)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch (Exception terminationException)
                {
                    error += $"{Environment.NewLine}" +
                             $"Ошибка завершения процесса: {terminationException.Message}";
                }

                try
                {
                    await process.WaitForExitAsync();

                    exitCode = process.ExitCode;
                }
                catch (Exception waitException)
                {
                    error += $"{Environment.NewLine}" +
                             $"Ошибка ожидания завершения: {waitException.Message}";
                }

                if (outputTask is not null)
                {
                    try
                    {
                        standardOutput = await outputTask;
                    }
                    catch (Exception outputException)
                    {
                        error += $"{Environment.NewLine}" +
                                 $"Ошибка чтения stdout: {outputException.Message}";
                    }
                }

                if (errorTask is not null)
                {
                    try
                    {
                        standardError = await errorTask;
                    }
                    catch (Exception outputException)
                    {
                        error += $"{Environment.NewLine}" +
                                 $"Ошибка чтения stderr: {outputException.Message}";
                    }
                }
            }
        }
        finally
        {
            stopwatch.Stop();
        }

        return new ProcessExecutionResult(
            StartedAt: startedAt,
            ProgramName: request.ProgramName,
            Arguments: request.Arguments,
            ProcessId: processId,
            Duration: stopwatch.Elapsed,
            ExitCode: exitCode,
            Error: error,
            StandardOutput: standardOutput,
            StandardError: standardError,
            TimedOut: timedOut
        );
    }
}
