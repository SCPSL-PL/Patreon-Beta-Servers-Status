namespace PatreonBetaServerStatus;

public class ServerObject
{
    public string Name { get; set; }

    public string ContinentCode { get; set; }

    public string Players { get; set; }

    public ServerObject(string name, string continentcode, string players)
    {
        Name = name;
        ContinentCode = continentcode;
        Players = players;
    }
}
