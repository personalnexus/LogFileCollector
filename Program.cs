Console.WriteLine(Collector.GetDestinationDirectory(@"\\Server1\Logs\", @"\\LogFileServer\ApplicationLogs", @"\\Server1\Logs\App1\Instance1\"));
Console.WriteLine(Collector.GetDestinationFileName("Server1", "data.log"));

var configuration = new Configuration
{
    MachinesFilePath = @"C:\Config\Machines.xml",
    SourceBasePathTemplate = @"\\{0}\Logs\"
};
var collector = new Collector(configuration, null);
collector.Start();