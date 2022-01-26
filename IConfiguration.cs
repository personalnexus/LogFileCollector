internal interface IConfiguration
{
    string MachinesFilePath { get; }
    string SourceBasePathTemplate { get; }
    string DestinationBasePath { get; }
    EnvironmentKind EnvironmentKind { get; }
}