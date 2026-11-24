// =============================================================================
//  TelaSelecionarJogador.cs
//  Lista os perfis cadastrados para escolher com qual jogar.
//  Também permite EXCLUIR um perfil (o "D" do CRUD), com confirmação.
// =============================================================================

using System;
using System.Collections.Generic;
using JogoPeixe.Jogadores;
using JogoPeixe.Jogo;
using JogoPeixe.Nucleo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JogoPeixe.Telas;

public class TelaSelecionarJogador : Tela
{
    // Quantas linhas cabem no painel. Se houver mais jogadores, a lista "rola".
    private const int LinhasVisiveis = 7;

    private readonly bool _irParaJogoDepois;
    private List<Jogador> _jogadores;
    private int _selecionado;
    private int _primeiraLinha;      // índice do primeiro jogador visível (rolagem)

    // Quando não é null, estamos perguntando "Excluir fulano?".
    private Jogador _confirmandoExclusao;

    public TelaSelecionarJogador(Contexto ctx, bool irParaJogoDepois) : base(ctx)
    {
        _irParaJogoDepois = irParaJogoDepois;
        RecarregarLista();

        // Começa com o jogador atual já selecionado, se houver.
        if (ctx.JogadorAtual != null)
            _selecionado = Math.Max(0, _jogadores.IndexOf(ctx.JogadorAtual));
    }

    private void RecarregarLista()
    {
        _jogadores = Ctx.Repositorio.EmOrdemAlfabetica();
        _selecionado = Math.Clamp(_selecionado, 0, Math.Max(0, _jogadores.Count - 1));
    }

    protected override void AoAtualizar(float dt)
    {
        AtualizarFundo(dt);

        if (_confirmandoExclusao != null)
        {
            AtualizarConfirmacao();
            return;
        }

        _selecionado = NavegarLista(_selecionado, _jogadores.Count);
        AjustarRolagem();

        if (Ctx.Entrada.ApertouConfirmar && _jogadores.Count > 0)
        {
            Selecionar(_jogadores[_selecionado]);
        }
        else if (Ctx.Entrada.ApertouAlguma(Keys.Delete, Keys.X) && _jogadores.Count > 0)
        {
            _confirmandoExclusao = _jogadores[_selecionado];
            Ctx.Audio.Tocar("alerta", 0.6f);
        }
        else if (Ctx.Entrada.Apertou(Keys.N))
        {
            Ctx.Audio.Tocar("menu_confirmar");
            Ctx.TrocarTela(new TelaNovoJogador(Ctx, _irParaJogoDepois));
        }
        else if (Ctx.Entrada.ApertouVoltar)
        {
            Ctx.Audio.Tocar("menu_voltar");
            Ctx.TrocarTela(new TelaMenu(Ctx));
        }
    }

    /// <summary>Responde à pergunta "Excluir?": S/Enter = sim, N/Esc = não.</summary>
    private void AtualizarConfirmacao()
    {
        if (Ctx.Entrada.ApertouAlguma(Keys.S, Keys.Enter))
        {
            Jogador excluido = _confirmandoExclusao;
            Ctx.Repositorio.Excluir(excluido);

            // Se excluiu o jogador que estava selecionado no jogo, ninguém fica selecionado.
            if (Ctx.JogadorAtual == excluido)
                Ctx.JogadorAtual = null;

            _confirmandoExclusao = null;
            Ctx.Audio.Tocar("splash", 0.6f);
            RecarregarLista();

            // Excluiu o último? Não há mais o que listar: volta ao menu.
            if (_jogadores.Count == 0)
                Ctx.TrocarTela(new TelaMenu(Ctx));
        }
        else if (Ctx.Entrada.ApertouAlguma(Keys.N, Keys.Escape))
        {
            _confirmandoExclusao = null;
            Ctx.Audio.Tocar("menu_voltar");
        }
    }

    /// <summary>Garante que o item selecionado esteja dentro da parte visível da lista.</summary>
    private void AjustarRolagem()
    {
        if (_selecionado < _primeiraLinha)
            _primeiraLinha = _selecionado;
        else if (_selecionado >= _primeiraLinha + LinhasVisiveis)
            _primeiraLinha = _selecionado - LinhasVisiveis + 1;
    }

    private void Selecionar(Jogador jogador)
    {
        Ctx.JogadorAtual = jogador;
        Ctx.Audio.Tocar("menu_confirmar");

        if (_irParaJogoDepois)
            Ctx.TrocarTela(new TelaJogo(Ctx));
        else
            Ctx.TrocarTela(new TelaMenu(Ctx));
    }

    public override void Desenhar()
    {
        SpriteBatch lote = Ctx.IniciarDesenho();
        float centro = Config.LarguraTela / 2f;

        DesenharFundo(lote, 0.45f);
        var painel = new Rectangle((int)centro - 400, 80, 800, 530);
        Desenho.Painel(lote, painel);

        Fonte.DesenharCentralizado(lote, "ESCOLHA O JOGADOR", centro, 115f, CorTitulo, 32f);

        // Cabeçalho da tabela.
        float colunaNome = painel.X + 90f;
        float colunaRecordeDireita = painel.Right - 60f;
        Fonte.Desenhar(lote, "NOME", new Vector2(colunaNome, 180f), CorSuave, 16f);
        Fonte.Desenhar(lote, "RECORDE", new Vector2(colunaRecordeDireita - Fonte.MedirLargura("RECORDE", 16f), 180f), CorSuave, 16f);

        // Linhas visíveis da lista.
        int ultima = Math.Min(_jogadores.Count, _primeiraLinha + LinhasVisiveis);
        for (int i = _primeiraLinha; i < ultima; i++)
        {
            Jogador jogador = _jogadores[i];
            float y = 220f + (i - _primeiraLinha) * 48f;
            bool ativo = i == _selecionado;

            if (ativo)
                Desenho.Retangulo(lote, new Rectangle(painel.X + 30, (int)y - 10, painel.Width - 60, 44), new Color(255, 220, 60) * 0.18f);

            Color cor = ativo ? CorSelecao : CorTexto;
            if (ativo)
                Fonte.DesenharComContorno(lote, ">", new Vector2(painel.X + 50f, y), cor, 24f);

            Fonte.DesenharComContorno(lote, jogador.Nome, new Vector2(colunaNome, y), cor, 24f);

            // Marca o jogador que já está em uso.
            if (jogador == Ctx.JogadorAtual)
                Fonte.Desenhar(lote, "(ATUAL)", new Vector2(colunaNome + Fonte.MedirLargura(jogador.Nome, 24f) + 16f, y + 6f), CorSuave, 12f);

            string recorde = jogador.Recorde.ToString();
            Fonte.DesenharComContorno(lote, recorde, new Vector2(colunaRecordeDireita - Fonte.MedirLargura(recorde, 24f), y), cor, 24f);
        }

        // Setinhas indicando que há mais itens acima/abaixo.
        if (_primeiraLinha > 0)
            Fonte.DesenharCentralizado(lote, "^", centro, 196f, CorSuave, 16f);
        if (ultima < _jogadores.Count)
            Fonte.DesenharCentralizado(lote, "v", centro, 560f, CorSuave, 16f);

        DesenharRodape(lote, "ENTER: ESCOLHER   N: NOVO   DEL/X: EXCLUIR   ESC: VOLTAR");

        if (_confirmandoExclusao != null)
            DesenharConfirmacao(lote, centro);

        Ctx.FinalizarDesenho();
    }

    /// <summary>Caixa de pergunta por cima da lista.</summary>
    private void DesenharConfirmacao(SpriteBatch lote, float centro)
    {
        Desenho.Escurecer(lote, 0.5f);
        Desenho.Painel(lote, new Rectangle((int)centro - 330, 250, 660, 200));

        Fonte.DesenharCentralizado(lote, "EXCLUIR JOGADOR?", centro, 285f, CorErro, 28f);
        Fonte.DesenharCentralizado(lote, _confirmandoExclusao.Nome, centro, 335f, CorTexto, 24f);
        Fonte.DesenharCentralizado(lote, "S: SIM      N: NÃO", centro, 395f, CorSuave, 18f);
    }
}
