// =============================================================================
//  TelaMenu.cs
//  Menu principal: Jogar, Novo jogador, Trocar jogador, Ranking, Sair.
// =============================================================================

using System;
using JogoPeixe.Jogo;
using JogoPeixe.Nucleo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Telas;

public class TelaMenu : Tela
{
    // Os índices abaixo precisam bater com a ordem do array _opcoes.
    private const int OpcaoJogar = 0;
    private const int OpcaoNovoJogador = 1;
    private const int OpcaoTrocarJogador = 2;
    private const int OpcaoRanking = 3;
    private const int OpcaoSair = 4;

    private readonly string[] _opcoes = { "JOGAR", "NOVO JOGADOR", "TROCAR JOGADOR", "RANKING", "SAIR" };
    private int _selecionada;

    // Mensagem temporária (ex.: "Nenhum jogador cadastrado").
    private string _mensagem;
    private float _tempoMensagem;

    // Peixinho decorativo que atravessa a tela.
    private readonly Animacao _peixeDecorativo;
    private float _xPeixe = -100f;

    public TelaMenu(Contexto ctx) : base(ctx)   // ": base(ctx)" chama o construtor da classe Tela
    {
        _peixeDecorativo = new Animacao(ctx.Recursos.PeixeNadando, 9f);
    }

    public override void Entrar()
    {
        Ctx.Audio.TocarMusica("musica_menu");
    }

    protected override void AoAtualizar(float dt)
    {
        AtualizarFundo(dt);
        _peixeDecorativo.Atualizar(dt);
        _xPeixe += 140f * dt;
        if (_xPeixe > Config.LarguraTela + 100f)
            _xPeixe = -100f;

        _tempoMensagem = Math.Max(0f, _tempoMensagem - dt);
        _selecionada = NavegarLista(_selecionada, _opcoes.Length);

        // Esc no menu principal só leva a seleção até "SAIR" (evita fechar sem querer).
        if (Ctx.Entrada.ApertouVoltar)
        {
            _selecionada = OpcaoSair;
            Ctx.Audio.Tocar("menu_mover");
        }

        if (Ctx.Entrada.ApertouConfirmar)
            Escolher();
    }

    /// <summary>Executa a opção selecionada.</summary>
    private void Escolher()
    {
        bool temJogadores = Ctx.Repositorio.Todos.Count > 0;

        switch (_selecionada)
        {
            case OpcaoJogar:
                Ctx.Audio.Tocar("menu_confirmar");
                if (Ctx.JogadorAtual != null)
                    Ctx.TrocarTela(new TelaJogo(Ctx));
                else if (temJogadores)
                    Ctx.TrocarTela(new TelaSelecionarJogador(Ctx, irParaJogoDepois: true));   // escolhe e já joga
                else
                    Ctx.TrocarTela(new TelaNovoJogador(Ctx, irParaJogoDepois: true));         // cadastra e já joga
                break;

            case OpcaoNovoJogador:
                Ctx.Audio.Tocar("menu_confirmar");
                Ctx.TrocarTela(new TelaNovoJogador(Ctx, irParaJogoDepois: false));
                break;

            case OpcaoTrocarJogador:
                if (!temJogadores)
                {
                    MostrarMensagem("Nenhum jogador cadastrado ainda.");
                    Ctx.Audio.Tocar("erro");
                    break;
                }
                Ctx.Audio.Tocar("menu_confirmar");
                Ctx.TrocarTela(new TelaSelecionarJogador(Ctx, irParaJogoDepois: false));
                break;

            case OpcaoRanking:
                Ctx.Audio.Tocar("menu_confirmar");
                Ctx.TrocarTela(new TelaRanking(Ctx));
                break;

            case OpcaoSair:
                Ctx.Sair();
                break;
        }
    }

    private void MostrarMensagem(string texto)
    {
        _mensagem = texto;
        _tempoMensagem = 2.5f;
    }

    public override void Desenhar()
    {
        SpriteBatch lote = Ctx.IniciarDesenho();
        float centro = Config.LarguraTela / 2f;

        DesenharFundo(lote, 0.25f);

        // Peixinho decorativo nadando lá embaixo.
        FitaSprites fita = _peixeDecorativo.Fita;
        float ondaY = (float)Math.Sin(TempoNaTela * 2f) * 12f;
        fita.Desenhar(lote, _peixeDecorativo.QuadroAtual, new Vector2(_xPeixe, 640f + ondaY),
                      fita.Centro, 0.6f, Color.White);

        // Logo flutuando (sobe e desce devagar).
        FitaSprites logo = Ctx.Recursos.Logo;
        float flutuar = (float)Math.Sin(TempoNaTela * 1.5f) * 6f;
        logo.Desenhar(lote, 0, new Vector2(centro, 150f + flutuar), logo.Centro, 0.82f, Color.White);

        // Painel com as opções.
        Desenho.Painel(lote, new Rectangle((int)centro - 270, 290, 540, 280));
        DesenharOpcoes(lote, _opcoes, _selecionada, centro, 320f, 26f, 50f);

        // Jogador atual.
        string textoJogador = Ctx.JogadorAtual != null
            ? $"JOGADOR: {Ctx.JogadorAtual.Nome}  |  RECORDE: {Ctx.JogadorAtual.Recorde}"
            : "NENHUM JOGADOR SELECIONADO";
        Fonte.DesenharCentralizado(lote, textoJogador, centro, 592f, CorSuave, 16f);

        if (_tempoMensagem > 0f)
            Fonte.DesenharCentralizado(lote, _mensagem, centro, 620f, CorErro, 16f);

        DesenharRodape(lote, "SETAS: ESCOLHER   ENTER: CONFIRMAR   M: SOM   F11: TELA CHEIA");

        Ctx.FinalizarDesenho();
    }
}
