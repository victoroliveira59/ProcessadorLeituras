using System.Globalization;
using System.Text.Json;


public class Program
{
    public static async Task Main(string[] args)
    {
        while (true)
        {
            Console.Clear(); 
            Console.WriteLine("======================================================");
            Console.WriteLine("   Processador de Leituras de Hidrômetros v2.0");
            Console.WriteLine("======================================================");

            string diretorioSaida = "C:\\arquivos";
            string arquivoHidrometros = Path.Combine(diretorioSaida, "hidrometros_rota.txt");
            string arquivoCsvSaida = Path.Combine(diretorioSaida, $"resultado_leituras_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv");

            Directory.CreateDirectory(diretorioSaida);

            Console.WriteLine("\nPor favor, digite o caminho completo para o arquivo de leituras (.txt) e pressione Enter:");
            string arquivoTxtEntrada = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(arquivoTxtEntrada) || !File.Exists(arquivoTxtEntrada))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Erro: O arquivo não foi encontrado ou o caminho está em branco.");
                Console.ResetColor();
            }
            else
            {
                var hidrometrosParaBuscar = new HashSet<string>(
                    (await File.ReadAllLinesAsync(arquivoHidrometros)).Select(h => h.Trim())
                );

                var ultimasLeituras = new Dictionary<string, LeituraHidrometro>();
                int linhasProcessadas = 0;
                int registrosInvalidos = 0;

                Console.WriteLine("\nLendo e processando o arquivo de log... Isso pode levar um momento.");

                await foreach (var linha in File.ReadLinesAsync(arquivoTxtEntrada))
                {
                    linhasProcessadas++;
                    if (string.IsNullOrWhiteSpace(linha)) continue;
                    int indiceInicioJson = linha.IndexOf('{');
                    if (indiceInicioJson == -1) { registrosInvalidos++; continue; }
                    string jsonString = linha.Substring(indiceInicioJson);
                    try
                    {
                        var payload = JsonSerializer.Deserialize<PayloadRotas>(jsonString);
                        if (payload?.Rotas != null)
                        {
                            foreach (var rota in payload.Rotas)
                            {
                                if (rota.DadosLeitura != null)
                                {
                                    foreach (var leituraAtual in rota.DadosLeitura)
                                    {
                                        if (leituraAtual != null && !string.IsNullOrWhiteSpace(leituraAtual.Hidrometro))
                                        {
                                            string hidrometroLimpo = leituraAtual.Hidrometro.Trim();
                                            if (hidrometrosParaBuscar.Contains(hidrometroLimpo))
                                            {
                                                if (ultimasLeituras.TryGetValue(hidrometroLimpo, out LeituraHidrometro la))
                                                {
                                                    if (DateTime.ParseExact(leituraAtual.DataAlteracao, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture) > DateTime.ParseExact(la.DataAlteracao, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture))
                                                    {
                                                        ultimasLeituras[hidrometroLimpo] = leituraAtual;
                                                    }
                                                }
                                                else
                                                {
                                                    ultimasLeituras.Add(hidrometroLimpo, leituraAtual);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception) { registrosInvalidos++; }
                }

                Console.WriteLine("Processamento concluído. Gerando arquivo CSV...");

                using (var streamCsv = new StreamWriter(arquivoCsvSaida))
                {
                    await streamCsv.WriteLineAsync("DataAlteracao,IdentificadorPenaDagua,Hidrometro,LeituraAtual");
                    foreach (var leituraFinal in ultimasLeituras.Values.OrderBy(l => l.Hidrometro))
                    {
                        var linhaCsv = $"{leituraFinal.DataAlteracao},{leituraFinal.IdentificadorPenaDagua},{leituraFinal.Hidrometro},{leituraFinal.LeituraAtual}";
                        await streamCsv.WriteLineAsync(linhaCsv);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nSucesso!");
                Console.ResetColor();
                Console.WriteLine($"Total de linhas lidas do log: {linhasProcessadas}");
                Console.WriteLine($"{ultimasLeituras.Count} leituras únicas e mais recentes foram salvas em '{arquivoCsvSaida}'.");
                if (registrosInvalidos > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"AVISO: {registrosInvalidos} linhas com formato inválido foram ignoradas.");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\n----------------------------------------");
            Console.Write("Deseja processar um novo arquivo? (S/N): ");
            string resposta = Console.ReadLine();

            if (resposta.Trim().Equals("N", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
        }

        Console.WriteLine("\nAplicação encerrada.");
    }
}