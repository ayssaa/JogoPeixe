// =============================================================================
//  TelaJogo.cs
//  A tela da partida. Cuida de tudo que está "em volta" do jogo em si:
//    - contagem regressiva (3, 2, 1, VAI!)
//    - pausa
//    - fim de jogo: atualiza o recorde (o "U" do CRUD) e mostra o resultado
//
//  O jogo propriamente dito (peixe, pescador, obstáculos) fica na classe Partida.
// =============================================================================

using System;
using JogoPeixe.Jogo;
using JogoPeixe.Nucleo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JogoPeixe.Telas;

public class TelaJogo : Tela
{
    /// <summary>Etapas da tela de jogo.</summary>
    private enum Etapa
    {
        Contagem,   // 3, 2, 1...
        Jogando,
        Pausado,
        Fim         // game over
    }

    private const float DuracaoContagem = 3f;

    private readonly Partida _partida;
    private readonly Random _sorteio = new Random();

    private Etapa _etapa = Etapa.Contagem;
    private float _tempoNaEtapa;

    // Resultado guardado no fim da partida.
    private bool _resultadoRegistrado;
    private bool _novoRecorde;
    private int _recordeAntes;
    private int _posicaoRanking;

    // Para tocar o som de "novo recorde" um pouco depois do som de game over.
    private bool _somRecordeTocado;

    public TelaJogo(Contexto ctx) : base(ctx)
    {
        _partida = new Partida(ctx);
        _recordeAntes = ctx.JogadorAtual?.Recorde ?? 0;
    }

    public override void Entrar()
    {
        Ctx.Audio.TocarMusica("musica_jogo");
    }

    private void MudarEtapa(Etapa nova)
    {
        _etapa = nova;
        _tempoNaEtapa = 0f;
    }

    protected override void AoAtualizar(float dt)
    {
        _tempoNaEtapa += dt;
        Hud.Atualizar(dt);

        // F3 liga/desliga a visualização das caixas de colisão (modo depuração).
        if (Ctx.Entrada.Apertou(Keys.F3))
            _partida.MostrarHitboxes = !_partida.MostrarHitboxes;

        switch (_etapa)
        {
            case Etapa.Contagem:
                AtualizarContagem(dt);
                break;
            case Etapa.Jogando:
                AtualizarJogando(dt);
                break;
            case Etapa.Pausado:
                AtualizarPausado();
                break;
            case Etapa.Fim:
                AtualizarFim(dt);
                break;
        }
    }

    private void AtualizarContagem(float dt)
    {
        // Durante a contagem, só o cenário anda (o peixe ainda não se move).
        Ctx.Cenario.Atualizar(dt, Config.VelocidadeMundoInicial * 0.5f);

        // Toca um "bip" a cada número da contagem.
        int antes = (int)(_tempoNaEtapa - dt);
        int agora = (int)_tempoNaEtapa;
        if (agora != antes && agora < DuracaoContagem)
            Ctx.Audio.Tocar("menu_mover", 0.8f);

        if (_tempoNaEtapa >= DuracaoContagem)
        {
            Ctx.Audio.Tocar("iniciar");
            MudarEtapa(Etapa.Jogando);
        }
    }

    private void AtualizarJogando(float dt)
    {
        // P ou Esc pausam. Também pausa sozinho se a janela perder o foco.
        if (Ctx.Entrada.ApertouAlguma(Keys.P, Keys.Escape) || !Ctx.JanelaAtiva)
        {
            Ctx.Audio.Tocar("menu_mover");
            MudarEtapa(Etapa.Pausado);
            return;
        }

        _partida.Atualizar(dt);

        if (_partida.Terminou)
        {
            RegistrarResultado();
            Ctx.Audio.PararMusica();
            Ctx.Audio.Tocar("gameover");
            MudarEtapa(Etapa.Fim);
        }
    }

    private void AtualizarPausado()
    {
        if (Ctx.Entrada.ApertouAlguma(Keys.P, Keys.Enter, Keys.Space))
        {
            Ctx.Audio.Tocar("menu_confirmar");
            MudarEtapa(Etapa.Jogando);
        }
        else if (Ctx.Entrada.Apertou(Keys.Escape))
        {
            // Sair no meio da partida também conta: a pontuação feita até aqui é registrada.
            RegistrarResultado();
            Ctx.Audio.Tocar("menu_voltar");
            Ctx.TrocarTela(new TelaMenu(Ctx));
        }
    }

    private void AtualizarFim(float dt)
    {
        // A partida continua rodando (o peixe boia, o mundo desacelera) por trás do painel.
        _partida.Atualizar(dt);

        if (_novoRecorde && !_somRecordeTocado && _tempoNaEtapa > 1.4f)
        {
            Ctx.Audio.Tocar("recorde");
            _somRecordeTocado = true;
        }

        // Espera um pouco antes de aceitar teclas (para não pular o resultado sem querer).
        if (_tempoNaEtapa < 1.2f)
            return;

        if (Ctx.Entrada.ApertouConfirmar)
        {
            Ctx.Audio.Tocar("menu_confirmar");
            Ctx.TrocarTela(new TelaJogo(Ctx));       // nova partida
        }
        else if (Ctx.Entrada.Apertou(Keys.R))
        {
            Ctx.Audio.Tocar("menu_confirmar");
            Ctx.TrocarTela(new TelaRanking(Ctx));
        }
        else if (Ctx.Entrada.ApertouVoltar)
        {
            Ctx.Audio.Tocar("menu_voltar");
            Ctx.TrocarTela(new TelaMenu(Ctx));
        }
    }

    /// <summary>
    /// Compara a pontuação com o recorde e salva no JSON se for maior.
    /// O "_resultadoRegistrado" garante que isso aconteça uma única vez.
    /// </summary>
    private void RegistrarResultado()
    {
        if (_resultadoRegistrado || Ctx.JogadorAtual == null)
            return;

        _resultadoRegistrado = true;
        _novoRecorde = Ctx.Repositorio.AtualizarRecorde(Ctx.JogadorAtual, _partida.Pontos);
        _posicaoRanking = Ctx.Repositorio.Posicao(Ctx.JogadorAtual);
    }

    // ==================================================================
    //  Desenho
    // ==================================================================

    public override void Desenhar()
    {
        // Tremor de tela: desloca tudo um pouco para um lado aleatório.
        Matrix? tremor = null;
        if (_partida.Tremor > 0f)
        {
            float forca = _partida.Tremor * 22f;
            tremor = Matrix.CreateTranslation(Sortear(-forca, forca), Sortear(-forca, forca), 0f);
        }

        // 1º lote: o mundo (com tremor).
        SpriteBatch lote = Ctx.IniciarDesenho(tremor);
        _partida.Desenhar(lote);
        Ctx.FinalizarDesenho();

        // 2º lote: interface (sem tremor, para continuar legível).
        lote = Ctx.IniciarDesenho();
        Hud.Desenhar(lote, Ctx.Recursos, _partida, Ctx.JogadorAtual);

        switch (_etapa)
        {
            case Etapa.Contagem:
                DesenharContagem(lote);
                break;
            case Etapa.Pausado:
                DesenharPausa(lote);
                break;
            case Etapa.Fim:
                DesenharFim(lote);
                break;
        }

        Ctx.FinalizarDesenho();
    }

    private void DesenharContagem(SpriteBatch lote)
    {
        float centro = Config.LarguraTela / 2f;
        int numero = 3 - (int)_tempoNaEtapa;             // 3, 2, 1
        string texto = numero > 0 ? numero.ToString() : "VAI!";

        // Cada número começa grande e encolhe (efeito "pulo").
        float fracao = _tempoNaEtapa % 1f;               // 0..1 dentro de cada segundo
        float tamanho = MathHelper.Lerp(110f, 70f, fracao);

        Fonte.DesenharCentralizado(lote, texto, centro, 330f - tamanho / 2f, CorTitulo, tamanho);
        Fonte.DesenharCentralizado(lote, "SETAS OU WASD PARA NADAR", centro, 450f, CorTexto, 18f);
        Fonte.DesenharCentralizado(lote, "FUJA DAS MARCAS VERMELHAS E DOS OBSTÁCULOS!", centro, 482f, CorSuave, 14f);
    }

    private void DesenharPausa(SpriteBatch lote)
    {
        float centro = Config.LarguraTela / 2f;
        Desenho.Escurecer(lote, 0.55f);
        Desenho.Painel(lote, new Rectangle((int)centro - 330, 220, 660, 260));

        Fonte.DesenharCentralizado(lote, "PAUSADO", centro, 260f, CorTitulo, 44f);
        Fonte.DesenharCentralizado(lote, "P OU ENTER: CONTINUAR", centro, 350f, CorTexto, 18f);
        Fonte.DesenharCentralizado(lote, "ESC: SAIR PARA O MENU", centro, 390f, CorTexto, 18f);
        Fonte.DesenharCentralizado(lote, "(SEUS PONTOS SERÃO SALVOS)", centro, 428f, CorSuave, 12f);
    }

    private void DesenharFim(SpriteBatch lote)
    {
        // O painel aparece aos poucos (fade) depois de 0,6 s.
        float aparecer = MathHelper.Clamp((_tempoNaEtapa - 0.6f) / 0.5f, 0f, 1f);
        if (aparecer <= 0f)
            return;

        float centro = Config.LarguraTela / 2f;
        Desenho.Escurecer(lote, 0.5f * aparecer);
        Desenho.Painel(lote, new Rectangle((int)centro - 380, 150, 760, 430));

        Fonte.DesenharCentralizado(lote, "FIM DE JOGO", centro, 185f, CorErro, 48f);
        Fonte.DesenharCentralizado(lote, $"PONTOS: {_partida.Pontos}", centro, 270f, CorTexto, 28f);

        if (_novoRecorde)
        {
            // Pisca entre amarelo e branco.
            Color cor = (int)(_tempoNaEtapa * 4f) % 2 == 0 ? CorTitulo : CorTexto;
            Fonte.DesenharCentralizado(lote, "NOVO RECORDE!", centro, 320f, cor, 28f);
            if (_recordeAntes > 0)
                Fonte.DesenharCentralizado(lote, $"RECORDE ANTERIOR: {_recordeAntes}", centro, 362f, CorSuave, 16f);
        }
        else if (Ctx.JogadorAtual != null)
        {
            Fonte.DesenharCentralizado(lote, $"RECORDE: {Ctx.JogadorAtual.Recorde}", centro, 320f, CorSuave, 22f);
        }

        if (_posicaoRanking > 0)
            Fonte.DesenharCentralizado(lote, $"POSIÇÃO NO RANKING: {_posicaoRanking}º", centro, 405f, CorSuave, 18f);

        if (_tempoNaEtapa >= 1.2f)
        {
            Fonte.DesenharCentralizado(lote, "ENTER: JOGAR DE NOVO", centro, 465f, CorSelecao, 20f);
            Fonte.DesenharCentralizado(lote, "R: RANKING     ESC: MENU", centro, 505f, CorTexto, 16f);
        }
    }

    private float Sortear(float minimo, float maximo) =>
        minimo + (float)_sorteio.NextDouble() * (maximo - minimo);
}
