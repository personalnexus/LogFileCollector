internal class Configuration: IConfiguration
{
    public string MachinesFilePath { get; set; }
    public string SourceBasePathTemplate { get; set; }
    public string DestinationBasePath { get; set; }
    public EnvironmentKind EnvironmentKind { get; set; }
}
