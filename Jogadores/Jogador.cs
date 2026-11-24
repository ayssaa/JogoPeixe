// =============================================================================
//  Jogador.cs
//  Representa um perfil de jogador: um nome e o melhor resultado (recorde).
//
//  É uma classe simples de dados. O System.Text.Json converte objetos dela
//  para JSON e de volta automaticamente, usando as propriedades públicas:
//
//      { "Nome": "Nemo", "Recorde": 12760 }
// =============================================================================

namespace JogoPeixe.Jogadores;

public class Jogador
{
    /// <summary>Nome do jogador (único: não podem existir dois iguais).</summary>
    public string Nome { get; set; }

    /// <summary>Maior pontuação já feita por esse jogador.</summary>
    public int Recorde { get; set; }

    /// <summary>Construtor vazio: OBRIGATÓRIO para o JSON conseguir criar o objeto
    /// ao ler o arquivo (ele cria vazio e depois preenche as propriedades).</summary>
    public Jogador()
    {
    }

    /// <summary>Construtor usado no cadastro: todo jogador novo começa com recorde 0.</summary>
    public Jogador(string nome)
    {
        Nome = nome;
        Recorde = 0;
    }

    /// <summary>Texto usado ao depurar (ex.: no console ou no debugger).</summary>
    public override string ToString() => $"{Nome} ({Recorde} pts)";
}
