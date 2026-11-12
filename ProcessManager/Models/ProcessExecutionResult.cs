namespace ProcessManager.Models;

public sealed record ProcessExecutionResult(
    DateTimeOffset StartedAt,
    string ProgramName,
    string Arguments,
    int? ProcessId,
    TimeSpan Duration,
    int? ExitCode,
    string? Error,
    string StandardOutput,
    string StandardError,
    bool TimedOut
);
