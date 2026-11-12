namespace ProcessManager.Models;

public sealed record ProcessExecutionRequest(
    string ProgramName,
    string Arguments,
    TimeSpan Timeout
);
