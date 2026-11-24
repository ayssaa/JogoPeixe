// =============================================================================
//  TelaNovoJogador.cs
//  Cadastro de um novo perfil (o "C" do CRUD).
//  O jogador digita o nome e aperta Enter.
// =============================================================================

using System;
using System.Text;
using JogoPeixe.Jogadores;
using JogoPeixe.Jogo;
using JogoPeixe.Nucleo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JogoPeixe.Telas;

public class TelaNovoJogador : Tela
{
    // Se true, depois de cadastrar já começa a partida (veio do botão "Jogar").
    private readonly bool _irParaJogoDepois;

    // StringBuilder: texto que pode ser alterado sem criar uma string nova a cada letra.
    private readonly StringBuilder _nome = new StringBuilder();

    private string _erro;

    // Controle para apagar várias letras ao segurar Backspace.
    private float _tempoSegurandoApagar;
    private float _proximaRepeticao;

    public TelaNovoJogador(Contexto ctx, bool irParaJogoDepois) : base(ctx)
    {
        _irParaJogoDepois = irParaJogoDepois;
    }

    public override void Entrar()
    {
        // Descarta letras digitadas antes de abrir esta tela.
        Ctx.Entrada.LimparTexto();
    }

    protected override void AoAtualizar(float dt)
    {
        AtualizarFundo(dt);
        LerLetras();
        TratarApagar(dt);

        if (Ctx.Entrada.Apertou(Keys.Enter))
            Confirmar();
        else if (Ctx.Entrada.ApertouVoltar)
        {
            Ctx.Audio.Tocar("menu_voltar");
            Ctx.TrocarTela(new TelaMenu(Ctx));
        }
    }

    /// <summary>Adiciona ao nome as letras digitadas neste quadro.</summary>
    private void LerLetras()
    {
        while (Ctx.Entrada.TentarLerCaractere(out char letra))
        {
            // Ignora teclas de controle (Enter, Backspace, Tab...) e letras sem desenho na fonte.
            if (char.IsControl(letra) || !Fonte.Suporta(letra))
                continue;

            // Não deixa começar com espaço.
            if (letra == ' ' && _nome.Length == 0)
                continue;

            if (_nome.Length < Config.TamanhoMaximoNome)
            {
                _nome.Append(letra);
                _erro = null;
                Ctx.Audio.Tocar("menu_mover", 0.5f, 0.3f);
            }
        }
    }

    /// <summary>Backspace apaga uma letra; segurando, apaga várias seguidas.</summary>
    private void TratarApagar(float dt)
    {
        if (!Ctx.Entrada.Segurando(Keys.Back))
        {
            _tempoSegurandoApagar = 0f;
            return;
        }

        bool apagar = false;

        if (Ctx.Entrada.Apertou(Keys.Back))
        {
            apagar = true;                       // primeiro toque: apaga na hora
            _proximaRepeticao = 0.4f;            // espera um pouco antes de repetir
        }
        else
        {
            _tempoSegurandoApagar += dt;
            if (_tempoSegurandoApagar >= _proximaRepeticao)
            {
                apagar = true;
                _proximaRepeticao += 0.05f;      // depois repete rápido
            }
        }

        if (apagar && _nome.Length > 0)
            _nome.Remove(_nome.Length - 1, 1);
    }

    private void Confirmar()
    {
        // Cadastrar devolve true/false e, pelos "out", o jogador criado ou a mensagem de erro.
        if (Ctx.Repositorio.Cadastrar(_nome.ToString(), out Jogador novo, out string erro))
        {
            Ctx.JogadorAtual = novo;
            Ctx.Audio.Tocar("menu_confirmar");

            if (_irParaJogoDepois)
                Ctx.TrocarTela(new TelaJogo(Ctx));
            else
                Ctx.TrocarTela(new TelaMenu(Ctx));
        }
        else
        {
            _erro = erro;
            Ctx.Audio.Tocar("erro");
        }
    }

    public override void Desenhar()
    {
        SpriteBatch lote = Ctx.IniciarDesenho();
        float centro = Config.LarguraTela / 2f;

        DesenharFundo(lote, 0.45f);
        Desenho.Painel(lote, new Rectangle((int)centro - 360, 150, 720, 380));

        Fonte.DesenharCentralizado(lote, "NOVO JOGADOR", centro, 190f, CorTitulo, 36f);
        Fonte.DesenharCentralizado(lote, "DIGITE SEU NOME:", centro, 270f, CorSuave, 18f);

        // Caixa de texto.
        const float tamanho = 32f;
        float larguraCaixa = Config.TamanhoMaximoNome * tamanho + 40f;
        var caixa = new Rectangle((int)(centro - larguraCaixa / 2f), 310, (int)larguraCaixa, 64);
        Desenho.Retangulo(lote, caixa, new Color(4, 14, 34) * 0.9f);
        Desenho.Contorno(lote, caixa, CorSelecao, 3);

        // Texto digitado + cursor piscando.
        string texto = _nome.ToString();
        bool cursorVisivel = (int)(TempoNaTela * 2.5f) % 2 == 0;
        Vector2 posicaoTexto = new Vector2(caixa.X + 20f, caixa.Y + 16f);
        Fonte.Desenhar(lote, texto, posicaoTexto, CorTexto, tamanho);

        if (cursorVisivel && _nome.Length < Config.TamanhoMaximoNome)
            Fonte.Desenhar(lote, "_", posicaoTexto + new Vector2(Fonte.MedirLargura(texto, tamanho), 0f), CorSelecao, tamanho);

        // Contador de letras (ex.: 5/12).
        Fonte.DesenharCentralizado(lote, $"{_nome.Length}/{Config.TamanhoMaximoNome}", centro, 392f, CorSuave, 14f);

        if (_erro != null)
            Fonte.DesenharCentralizado(lote, _erro, centro, 440f, CorErro, 18f);

        DesenharRodape(lote, "ENTER: CONFIRMAR   BACKSPACE: APAGAR   ESC: VOLTAR");
        Ctx.FinalizarDesenho();
    }
}
