internal class Collector
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public Collector(IConfiguration configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    internal void Start()
    {
        string currentMachine = "none";
        try
        {
            foreach (string machine in Machine.LoadNamesFromFile(_configuration.MachinesFilePath, _configuration.EnvironmentKind))
            {
                currentMachine = machine;
                Execute($"{machine}", () =>
                {
                    string sourceBase = string.Format(_configuration.SourceBasePathTemplate, machine);
                    DirectoryInfo logDirectory = new DirectoryInfo(sourceBase);
                    foreach (DirectoryInfo directory in logDirectory.EnumerateDirectories())
                    {
                        ProcessDirectory(machine, sourceBase, _configuration.DestinationBasePath, directory);
                    }
                });
            }
        }
        catch (Exception exception)
        {
            _logger.Mail($"Unexpected exception while processing log files for {currentMachine}. Terminating log file collection. {exception}");
        }
    }

    private void ProcessDirectory(string machine, string sourceBase, string destinationBase, DirectoryInfo directory)
    {
        // Example: with source base = \\Server1\Logs\ and destination base = \\LogFileServer\ApplicationLogs\
        // this source file                                 \\Server1\Logs\App1\Instance1\2022_01_26.log
        // becomes this destination file:  \\LogFileServer\ApplicationLogs\App1\Instance1\2022_01_26.Server1.log

        _logger.Info($"Processing {directory.FullName}...");
        int successCount = 0;
        int errorCount = 0;
        Execute(directory.FullName, () =>
        {
            string destinationDirectory = GetDestinationDirectory(sourceBase, destinationBase, directory.FullName);
            foreach (FileInfo file in directory.EnumerateFiles())
            {
                if (Execute(file.FullName, () =>
                {
                    string destinationFileName = GetDestinationFileName(machine, file.Name);
                    file.MoveTo(Path.Combine(destinationDirectory, destinationFileName));
                }))
                {
                    successCount++;
                }
                else
                {
                    errorCount++;
                }
            }
        });
        _logger.Info($"Processed {directory.FullName}: {successCount} successes, {errorCount} errors.");

        sourceBase = Path.Combine(sourceBase, directory.Name);
        destinationBase = Path.Combine(destinationBase, directory.Name);
        foreach (DirectoryInfo directoryInfo in directory.EnumerateDirectories())
        {
            ProcessDirectory(machine, sourceBase, destinationBase, directoryInfo);
        }
    }

    private bool Execute(string path, Action action)
    {
        bool result = false;
        try
        {
            action();
            result = true;
        }
        catch (UnauthorizedAccessException exception)
        {
            Log(exception);
        }
        catch (IOException exception)
        {
            Log(exception);
        }
        return result;

        void Log(Exception exception) => _logger.Info($"Ignored exception while processing {path} and continuing with next item. {exception}");
    }

    internal static string GetDestinationDirectory(string sourceBase, string destinationBase, string directory) => Path.Combine(destinationBase, directory.Substring(sourceBase.Length));

    internal static string GetDestinationFileName(string machine, string sourceFileName) => Path.ChangeExtension(sourceFileName, $"{machine}{Path.GetExtension(sourceFileName)}");
}
