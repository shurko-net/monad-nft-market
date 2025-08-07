namespace MonadNftMarket.Configuration;

public class EnvVariables
{
    public string JwtTokenSecret { get; set; } = string.Empty;
    public string CookieName { get; set; } = string.Empty;
    public string HyperSyncQueryUrl { get; set; } = string.Empty;
    public string AlternativeHyperSyncQueryUrl { get; set; } = string.Empty;
    public string MonadRpcUrl { get; set; } = string.Empty;
    public string ContractAddress { get; set; } = string.Empty;

    public static string ToEnvVariables(string pascalName)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < pascalName.Length; i++)
        {
            char c = pascalName[i];
            if (i > 0 && char.IsUpper(c))
            {
                sb.Append('_');
            }
            sb.Append(char.ToUpperInvariant(c));
        }

        return sb.ToString();
    }

    public static string ToDockerVariables(string pascalName)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < pascalName.Length; i++)
        {
            char c = pascalName[i];
            if (i > 0 && char.IsUpper(c))
            {
                sb.Append('_');
            }
            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }
}