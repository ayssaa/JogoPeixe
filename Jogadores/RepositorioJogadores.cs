// =============================================================================
//  RepositorioJogadores.cs
//  Guarda a lista de jogadores e cuida da persistência em arquivo JSON.
//
//  Implementa o CRUD pedido no trabalho:
//    C (Create) -> Cadastrar()
//    R (Read)   -> Ranking(), Todos, Buscar()
//    U (Update) -> AtualizarRecorde()
//    D (Delete) -> Excluir()
//
//  Toda alteração já é salva no disco na hora, então os dados continuam
//  lá mesmo se o jogo for fechado de repente.
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using JogoPeixe.Jogo;

namespace JogoPeixe.Jogadores;

public class RepositorioJogadores
{
    // Lista em memória com todos os jogadores. List<T> é uma lista que cresce sozinha.
    private readonly List<Jogador> _jogadores = new List<Jogador>();

    // Opções do JSON:
    //  - WriteIndented: grava o arquivo "bonito", com quebras de linha (fácil de ler).
    //  - UnsafeRelaxedJsonEscaping: mantém acentos legíveis ("João" em vez de "Jo\u00E3o").
    private static readonly JsonSerializerOptions OpcoesJson = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>Caminho completo do arquivo JSON (mostrado na tela de ranking).</summary>
    public string CaminhoArquivo { get; }

    /// <summary>Todos os jogadores, somente leitura (quem está fora não consegue
    /// adicionar/remover direto na lista, só pelos métodos desta classe).</summary>
    public IReadOnlyList<Jogador> Todos => _jogadores;

    /// <summary>Cria o repositório e já carrega o arquivo.</summary>
    /// <param name="caminhoArquivo">Se for null, usa a pasta padrão de dados do usuário.</param>
    public RepositorioJogadores(string caminhoArquivo = null)
    {
        CaminhoArquivo = caminhoArquivo ?? CaminhoPadrao();
        Carregar();
    }

    /// <summary>
    /// Pasta padrão onde salvar os dados:
    ///   Windows: C:\Users\SeuNome\AppData\Local\JogoPeixe\jogadores.json
    ///   Linux:   ~/.local/share/JogoPeixe/jogadores.json
    ///   macOS:   ~/Library/Application Support/JogoPeixe/jogadores.json (ou similar)
    /// Usar essa pasta (em vez da pasta do .exe) evita perder os dados ao recompilar
    /// e evita erro de permissão quando o jogo é instalado em "Arquivos de Programas".
    /// </summary>
    public static string CaminhoPadrao()
    {
        string pastaUsuario = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(pastaUsuario, Config.PastaDados, Config.ArquivoJogadores);
    }

    // ==================================================================
    //  Persistência (ler e gravar o arquivo)
    // ==================================================================

    /// <summary>Lê o arquivo JSON e preenche a lista. Chamado ao iniciar o jogo.</summary>
    public void Carregar()
    {
        _jogadores.Clear();

        // Primeira vez jogando: ainda não existe arquivo. Começamos com a lista vazia.
        if (!File.Exists(CaminhoArquivo))
            return;

        try
        {
            string json = File.ReadAllText(CaminhoArquivo);
            List<Jogador> lidos = JsonSerializer.Deserialize<List<Jogador>>(json, OpcoesJson);

            if (lidos != null)
            {
                // Ignora registros inválidos (sem nome) que alguém possa ter editado à mão.
                foreach (Jogador jogador in lidos)
                {
                    if (!string.IsNullOrWhiteSpace(jogador.Nome))
                        _jogadores.Add(jogador);
                }
            }
        }
        catch (Exception erro) when (erro is JsonException || erro is IOException)
        {
            // O arquivo existe mas está corrompido (ex.: editado errado à mão).
            // Em vez de travar o jogo, guardamos uma cópia do arquivo ruim e
            // seguimos com a lista vazia. Assim nada é perdido de vez.
            Console.WriteLine($"[Jogadores] Erro ao ler {CaminhoArquivo}: {erro.Message}");

            string copia = CaminhoArquivo + ".corrompido";
            File.Copy(CaminhoArquivo, copia, overwrite: true);
        }
    }

    /// <summary>Grava a lista inteira no arquivo JSON.</summary>
    public void Salvar()
    {
        // Cria a pasta, se ainda não existir.
        string pasta = Path.GetDirectoryName(CaminhoArquivo);
        if (!string.IsNullOrEmpty(pasta))
            Directory.CreateDirectory(pasta);

        string json = JsonSerializer.Serialize(_jogadores, OpcoesJson);

        // Gravação segura: primeiro escrevemos num arquivo temporário e só depois
        // trocamos pelo verdadeiro. Se o jogo fechar no meio da gravação,
        // o arquivo original continua inteiro.
        string temporario = CaminhoArquivo + ".tmp";
        File.WriteAllText(temporario, json);
        File.Move(temporario, CaminhoArquivo, overwrite: true);
    }

    // ==================================================================
    //  CRUD
    // ==================================================================

    /// <summary>
    /// CREATE: cadastra um jogador novo com recorde 0.
    /// Retorna true se deu certo. Se der errado, "erro" explica o motivo.
    /// </summary>
    public bool Cadastrar(string nome, out Jogador jogador, out string erro)
    {
        jogador = null;
        nome = (nome ?? "").Trim();   // remove espaços no começo e no fim

        // Validações: cada "if" verifica uma regra e sai cedo se ela falhar.
        if (nome.Length == 0)
        {
            erro = "Digite um nome.";
            return false;
        }

        if (nome.Length > Config.TamanhoMaximoNome)
        {
            erro = $"Máximo de {Config.TamanhoMaximoNome} letras.";
            return false;
        }

        if (Buscar(nome) != null)
        {
            erro = "Esse nome já existe.";
            return false;
        }

        jogador = new Jogador(nome);
        _jogadores.Add(jogador);
        Salvar();

        erro = null;
        return true;
    }

    /// <summary>READ: procura um jogador pelo nome, sem diferenciar maiúsculas/minúsculas.
    /// Retorna null se não encontrar.</summary>
    public Jogador Buscar(string nome)
    {
        // FirstOrDefault (LINQ) devolve o primeiro que satisfaz a condição, ou null.
        return _jogadores.FirstOrDefault(j =>
            string.Equals(j.Nome, nome?.Trim(), StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>READ: lista ordenada do maior recorde para o menor.
    /// Em caso de empate, ordena pelo nome (ordem alfabética).</summary>
    public List<Jogador> Ranking(int quantidade = Config.TamanhoRanking)
    {
        return _jogadores
            .OrderByDescending(j => j.Recorde)
            .ThenBy(j => j.Nome, StringComparer.CurrentCultureIgnoreCase)
            .Take(quantidade)
            .ToList();
    }

    /// <summary>Lista em ordem alfabética (usada na tela de seleção de jogador).</summary>
    public List<Jogador> EmOrdemAlfabetica()
    {
        return _jogadores
            .OrderBy(j => j.Nome, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>Posição do jogador no ranking geral (1 = primeiro lugar). 0 se não existir.</summary>
    public int Posicao(Jogador jogador)
    {
        List<Jogador> todos = Ranking(int.MaxValue);
        return todos.IndexOf(jogador) + 1;   // IndexOf devolve -1 se não achar -> 0
    }

    /// <summary>
    /// UPDATE: compara a pontuação da partida com o recorde atual.
    /// Se for maior, atualiza e salva. Retorna true quando é um NOVO recorde.
    /// </summary>
    public bool AtualizarRecorde(Jogador jogador, int pontuacao)
    {
        if (jogador == null || pontuacao <= jogador.Recorde)
            return false;

        jogador.Recorde = pontuacao;
        Salvar();
        return true;
    }

    /// <summary>DELETE: remove o jogador e salva. Retorna false se ele não estava na lista.</summary>
    public bool Excluir(Jogador jogador)
    {
        bool removeu = _jogadores.Remove(jogador);

        if (removeu)
            Salvar();

        return removeu;
    }
}
