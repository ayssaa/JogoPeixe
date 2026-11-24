// =============================================================================
//  Contexto.cs
//  "Caixa" com tudo o que é compartilhado entre as telas: imagens, sons,
//  teclado, lista de jogadores, cenário e o jogador escolhido.
//
//  Em vez de passar 6 parâmetros para cada tela, passamos só o Contexto.
// =============================================================================

using JogoPeixe.Jogadores;
using JogoPeixe.Jogo;
using JogoPeixe.Telas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Nucleo;

public class Contexto
{
    private readonly Game1 _jogo;
    private readonly SpriteBatch _lote;

    public Recursos Recursos { get; }
    public Audio Audio { get; }
    public Entrada Entrada { get; }
    public RepositorioJogadores Repositorio { get; }

    /// <summary>Cenário compartilhado: o fundo continua "andando" ao trocar de tela.</summary>
    public Cenario Cenario { get; }

    /// <summary>Jogador escolhido no momento (null = nenhum).</summary>
    public Jogador JogadorAtual { get; set; }

    /// <summary>Tela que deve entrar no lugar da atual (aplicada pelo Game1 no fim do quadro).</summary>
    public Tela ProximaTela { get; private set; }

    public Contexto(Game1 jogo, SpriteBatch lote, Recursos recursos, Audio audio,
                    Entrada entrada, RepositorioJogadores repositorio)
    {
        _jogo = jogo;
        _lote = lote;
        Recursos = recursos;
        Audio = audio;
        Entrada = entrada;
        Repositorio = repositorio;
        Cenario = new Cenario(recursos);
    }

    /// <summary>Pede a troca de tela. A troca acontece no fim do quadro atual,
    /// para não trocar a tela "no meio" do Update dela.</summary>
    public void TrocarTela(Tela novaTela)
    {
        ProximaTela = novaTela;
    }

    /// <summary>Usado pelo Game1 depois de aplicar a troca.</summary>
    public void LimparProximaTela() => ProximaTela = null;

    /// <summary>Fecha o jogo.</summary>
    public void Sair() => _jogo.Exit();

    /// <summary>A janela está em foco? (usado para pausar quando o jogador troca de janela)</summary>
    public bool JanelaAtiva => _jogo.IsActive;

    /// <summary>
    /// Inicia o desenho de um "lote" de sprites com as configurações padrão do jogo.
    /// "transformacao" permite deslocar tudo (ex.: tremor de tela).
    /// Sempre que chamar Iniciar, chame Finalizar depois.
    /// </summary>
    public SpriteBatch IniciarDesenho(Matrix? transformacao = null)
    {
        _lote.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    null, null, null, transformacao);
        return _lote;
    }

    public void FinalizarDesenho() => _lote.End();
}
