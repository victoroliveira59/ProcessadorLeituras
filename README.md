# Processador de Logs de Leitura de Hidrômetros

![Status](https://img.shields.io/badge/status-ativo-success)
![.NET Version](https://img.shields.io/badge/.NET-8.0-blueviolet)

Ferramenta de console em .NET para processar arquivos de log complexos, extrair dados de leitura de hidrômetros, aplicar regras de negócio para limpeza e consolidação, e exportar o resultado em um formato CSV limpo.

---

## Tabela de Conteúdos

1.  [Objetivo](#1-objetivo)
2.  [Funcionalidades Principais](#2-funcionalidades-principais)
3.  [Pré-requisitos](#3-pré-requisitos)
4.  [Estrutura de Arquivos Esperada](#4-estrutura-de-arquivos-esperada)
5.  [Como Executar](#5-como-executar)
6.  [Como o Código Pode Ser Adaptado](#6-como-o-código-pode-ser-adaptado)
7.  [Autor](#7-autor)

---

## 1. Objetivo

Esta aplicação foi construída para atender a uma demanda real da empresa, com o objetivo de automatizar uma atividade de análise de dados que, se feita manualmente, consumiria um tempo considerável e seria suscetível a erros.

O objetivo principal da ferramenta é processar arquivos de log brutos de leitura de hidrômetros. Muitas vezes, esses logs contêm dados duplicados de múltiplas sincronizações e informações em formatos aninhados (JSON). Esta ferramenta resolve o problema ao:

-   Ler arquivos de log de qualquer tamanho de forma eficiente.
-   Filtrar apenas as leituras dos hidrômetros de interesse.
-   Remover duplicatas, mantendo apenas a leitura mais recente de cada hidrômetro com base na data e hora.
-   Gerar um relatório final em formato CSV, pronto para análise e tomada de decisão.

Em suma, a console transforma um processo demorado e complexo em uma tarefa rápida, automatizada e confiável.

---

## 2. Funcionalidades Principais

-   **Processamento Eficiente:** Lê arquivos grandes linha por linha, sem consumir muita memória.
-   **Filtragem Dinâmica:** Utiliza um arquivo de texto (`hidrometros_rota.txt`) para definir quais hidrômetros devem ser processados.
-   **Desduplicação Inteligente:** Compara timestamps (`DataAlteracao`) para garantir que apenas a leitura mais recente de cada hidrômetro seja salva.
-   **Exportação para CSV:** Gera um arquivo `.csv` com nome dinâmico (incluindo data e hora) para evitar conflitos e manter um histórico.
-   **Interface Interativa:** Guia o usuário através de um prompt de comando para fornecer o caminho do arquivo a ser processado.
-   **Modo Contínuo:** Após cada processamento, a aplicação pergunta se o usuário deseja processar um novo arquivo, permitindo o uso em sessões contínuas.

---

## 3. Pré-requisitos

Para compilar e executar este projeto, você precisará ter o seguinte software instalado:

-   [.NET 8 SDK (ou superior)](https://dotnet.microsoft.com/download)

---

## 4. Estrutura de Arquivos Esperada

Para que a aplicação funcione corretamente, ela espera encontrar alguns arquivos em um diretório base (configurado no código para `C:\arquivos`).

1.  **`hidrometros_rota.txt`**
    -   **Objetivo:** Listar os números dos hidrômetros que você deseja filtrar.
    -   **Formato:** Um número de hidrômetro por linha. Espaços em branco antes ou depois do número são ignorados.
    -   **Exemplo:**
        ```
        549570
        083115
        214030
        ```

2.  **Arquivo de Log (`.txt`)**
    -   **Objetivo:** O arquivo de log bruto contendo os dados a serem processados.
    -   **Formato:** Cada linha pode conter um prefixo de log seguido por um objeto JSON. A aplicação foi projetada para extrair o JSON de cada linha.
    -   **Exemplo de linha:**
        ```
        [JsonsAPI] 09:52:37 : {"Rotas":[{"dadosLeitura":[{"IdentificadorPenaDagua":4137,"Hidrometro":"549570",...}]}]}
        ```

---

## 5. Como Executar

1.  **Clone o Repositório**
    ```bash
    git clone <URL_DO_SEU_REPOSITORIO>
    cd <NOME_DA_PASTA_DO_PROJETO>
    ```

2.  **Execute a Aplicação**
    Abra um terminal na pasta do projeto e execute o seguinte comando:
    ```bash
    dotnet run
    ```

3.  **Siga as Instruções no Console**
    -   A aplicação solicitará que você insira o caminho completo para o arquivo de log que deseja processar.
    -   Exemplo: `C:\Users\seu_usuario\Downloads\log_de_hoje.txt`
    -   Após o processamento, um arquivo `.csv` será gerado na pasta `C:\arquivos` (ou no diretório configurado).
    -   Em seguida, a aplicação perguntará se você deseja processar outro arquivo (`S/N`).

---

## 6. Como o Código Pode Ser Adaptado

Esta aplicação foi construída de forma modular, permitindo que seja facilmente adaptada para outros fins.

### Para um Novo Formato de JSON

-   **O que mudar?** Se a estrutura do JSON no seu log mudar (novos campos, nomes diferentes, mais níveis de aninhamento), você só precisa atualizar as classes de modelo (Models).
-   **Onde mudar?** No topo do arquivo `Leitura.cs`, ajuste as classes `PayloadRotas`, `Rota` e `LeituraHidrometro` para que espelhem a nova estrutura do JSON. Use os atributos `[JsonPropertyName("...")]` para mapear os campos corretamente.

### Para um Novo Formato de Arquivo de Log

-   **O que mudar?** Se o arquivo de log não for mais um JSON por linha ou se o prefixo mudar.
-   **Onde mudar?** A lógica de extração do JSON está no início do laço `await foreach`.
    ```csharp
    int indiceInicioJson = linha.IndexOf('{');
    string jsonString = linha.Substring(indiceInicioJson);
    ```
    Adapte este trecho para a nova forma como os dados estão dispostos no seu arquivo.

### Para Mudar a Regra de Negócio

-   **O que mudar?** Se, por exemplo, você precisar da leitura **mais antiga** em vez da mais recente, ou se quiser filtrar por um critério diferente.
-   **Onde mudar?** A lógica principal de negócio está dentro dos `if`s no laço de processamento. Para mudar a regra de "mais recente", basta inverter a comparação de datas:
    ```csharp
    // Para pegar a mais ANTIGA, mude de > para <
    if (dataAtual < dataArmazenada) 
    ```

### Para Outro Formato de Saída

-   **O que mudar?** Se você precisar gerar um XML, um arquivo de texto com outro delimitador, ou inserir os dados em um banco de dados.
-   **Onde mudar?** A mudança se concentra na etapa final, após a coleta dos dados. Em vez de escrever em um `StreamWriter` formatando para CSV, você implementaria a nova lógica de escrita.
    ```csharp
    // Substituir este bloco pela nova lógica de saída
    using (var streamCsv = new StreamWriter(arquivoCsvSaida))
    {
        // ...
    }
    ```

---

## 7. Autor

-   **Victor Samuel Oliveira**
