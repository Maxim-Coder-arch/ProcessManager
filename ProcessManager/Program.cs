using ProcessManager.Models;
using ProcessManager.Services;

namespace ProcessManager;

public static class Program
{
    private static readonly ProcessRunner ProcessRunner = new();
    private static readonly ProcessLogService ProcessLogService = new();

    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("================================");
        Console.WriteLine("       PROCESS MANAGER");
        Console.WriteLine("================================");
        Console.WriteLine("Лимит выполнения: 10 секунд");
        Console.WriteLine("Для выхода оставьте имя программы пустым.");

        while (true)
        {
            Console.WriteLine();
            Console.Write("Программа: ");

            string? programName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(programName))
            {
                break;
            }

            Console.Write("Аргументы: ");

            string arguments = Console.ReadLine() ?? string.Empty;

            var request = new ProcessExecutionRequest(
                ProgramName: programName.Trim(),
                Arguments: arguments,
                Timeout: TimeSpan.FromSeconds(10)
            );

            Console.WriteLine();
            Console.WriteLine("Запуск процесса...");

            ProcessExecutionResult result =
                await ProcessRunner.RunAsync(request);

            DisplayResult(result);

            try
            {
                await ProcessLogService.SaveAsync(result);

                Console.WriteLine();
                Console.WriteLine("Результат сохранён в process-log.txt");
            }
            catch (IOException exception)
            {
                Console.WriteLine(
                    $"Ошибка сохранения журнала: {exception.Message}"
                );
            }
            catch (UnauthorizedAccessException exception)
            {
                Console.WriteLine(
                    $"Нет доступа к файлу журнала: {exception.Message}"
                );
            }
        }

        Console.WriteLine();
        Console.WriteLine("Process Manager завершил работу.");
    }

    private static void DisplayResult(ProcessExecutionResult result)
    {
        Console.WriteLine();
        Console.WriteLine("========== РЕЗУЛЬТАТ ==========");

        Console.WriteLine($"Программа: {result.ProgramName}");
        Console.WriteLine($"PID: {result.ProcessId?.ToString() ?? "N/A"}");

        Console.WriteLine(
            $"Длительность: {result.Duration:hh\\:mm\\:ss\\.fff}"
        );

        Console.WriteLine(
            $"Код завершения: {result.ExitCode?.ToString() ?? "N/A"}"
        );

        Console.WriteLine(
            $"Тайм-аут: {(result.TimedOut ? "Да" : "Нет")}"
        );

        if (!string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            Console.WriteLine();
            Console.WriteLine("----- STANDARD OUTPUT -----");
            Console.WriteLine(result.StandardOutput);
        }

        if (!string.IsNullOrWhiteSpace(result.StandardError))
        {
            Console.WriteLine();
            Console.WriteLine("----- STANDARD ERROR -----");
            Console.WriteLine(result.StandardError);
        }

        if (!string.IsNullOrWhiteSpace(result.Error))
        {
            Console.WriteLine();
            Console.WriteLine("----- ERROR -----");
            Console.WriteLine(result.Error);
        }

        Console.WriteLine("===============================");
    }
}
