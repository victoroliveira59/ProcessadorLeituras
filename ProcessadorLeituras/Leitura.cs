using System.Text.Json.Serialization;

public class PayloadRotas
{
    [JsonPropertyName("Rotas")]
    public List<Rota> Rotas { get; set; }
}

public class Rota
{
    [JsonPropertyName("dadosLeitura")]
    public List<LeituraHidrometro> DadosLeitura { get; set; }
}

public class LeituraHidrometro
{
    [JsonPropertyName("IdentificadorPenaDagua")]
    public int IdentificadorPenaDagua { get; set; }

    [JsonPropertyName("Hidrometro")]
    public string Hidrometro { get; set; }

    [JsonPropertyName("LeituraAtual")]
    public int LeituraAtual { get; set; }

    [JsonPropertyName("DataAlteracao")]
    public string DataAlteracao { get; set; }
}