// =============================================================================
//  Tela.cs
//  Classe BASE de todas as telas do jogo (menu, ranking, partida...).
//
//  "abstract" significa que não dá para criar uma "Tela" genérica: só as
//  classes filhas (TelaMenu, TelaJogo, ...) que completam o que falta.
//  Isso é HERANÇA + POLIMORFISMO: o Game1 trata todas como "Tela" e chama
//  Atualizar/Desenhar sem saber qual tela específica está ativa.
// =============================================================================

using System;
using JogoPeixe.Jogo;
using JogoPeixe.Nucleo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Telas;

public abstract class Tela
{
    // Cores usadas em várias telas ("protected": visíveis para as classes filhas).
    protected static readonly Color CorTitulo = new Color(255, 220, 60);
    protected static readonly Color CorTexto = Color.White;
    protected static readonly Color CorSuave = new Color(170, 225, 255);
    protected static readonly Color CorSelecao = new Color(255, 220, 60);
    protected static readonly Color CorErro = new Color(255, 110, 100);

    /// <summary>Acesso a recursos, sons, teclado, jogadores...</summary>
    protected Contexto Ctx { get; }

    /// <summary>Atalho para a fonte.</summary>
    protected FonteBitmap Fonte => Ctx.Recursos.Fonte;

    /// <summary>Segundos desde que a tela abriu (para animações simples).</summary>
    protected float TempoNaTela { get; private set; }

    protected Tela(Contexto ctx)
    {
        Ctx = ctx;
    }

    /// <summary>Chamado uma vez, quando a tela passa a ser a ativa.
    /// "virtual": as filhas PODEM substituir (override), mas não são obrigadas.</summary>
    public virtual void Entrar()
    {
    }

    /// <summary>Chamado todo quadro pelo Game1.</summary>
    public void Atualizar(float dt)
    {
        TempoNaTela += dt;
        AoAtualizar(dt);
    }

    /// <summary>Lógica específica de cada tela. "abstract": as filhas SÃO obrigadas a implementar.</summary>
    protected abstract void AoAtualizar(float dt);

    /// <summary>Desenho específico de cada tela.</summary>
    public abstract void Desenhar();

    // ==================================================================
    //  Ajudantes usados pelas telas de menu
    // ==================================================================

    /// <summary>Faz o cenário de fundo andar devagar atrás dos menus.</summary>
    protected void AtualizarFundo(float dt)
    {
        Ctx.Cenario.Atualizar(dt, 90f);
    }

    /// <summary>Desenha o cenário e escurece um pouco, para o menu ficar legível.</summary>
    protected void DesenharFundo(SpriteBatch lote, float escurecer = 0.35f)
    {
        Ctx.Cenario.DesenharFundo(lote);
        Ctx.Cenario.DesenharSuperficie(lote);
        Desenho.Escurecer(lote, escurecer);
    }

    /// <summary>Lista de opções com a selecionada destacada em amarelo e uma seta.</summary>
    protected void DesenharOpcoes(SpriteBatch lote, string[] opcoes, int selecionada,
                                  float centroX, float yInicial, float tamanho, float espacamento)
    {
        for (int i = 0; i < opcoes.Length; i++)
        {
            float y = yInicial + i * espacamento;
            bool ativa = i == selecionada;

            Fonte.DesenharCentralizado(lote, opcoes[i], centroX, y, ativa ? CorSelecao : CorTexto, tamanho);

            if (ativa)
            {
                // A seta balança para os lados (seno) para chamar atenção.
                float largura = Fonte.MedirLargura(opcoes[i], tamanho);
                float balanco = (float)Math.Sin(TempoNaTela * 8f) * 4f;
                Fonte.DesenharComContorno(lote, ">",
                    new Vector2(centroX - largura / 2f - tamanho * 1.6f + balanco, y), CorSelecao, tamanho);
            }
        }
    }

    /// <summary>Texto de ajuda no rodapé da tela (teclas disponíveis).</summary>
    protected void DesenharRodape(SpriteBatch lote, string texto)
    {
        Fonte.DesenharCentralizado(lote, texto, Config.LarguraTela / 2f, Config.AlturaTela - 40f, CorSuave, 14f);
    }

    /// <summary>Move a seleção para cima/baixo com as setas, dando a volta nas pontas.</summary>
    protected int NavegarLista(int selecionada, int quantidade)
    {
        if (quantidade == 0)
            return 0;

        if (Ctx.Entrada.ApertouCima)
        {
            Ctx.Audio.Tocar("menu_mover");
            return (selecionada - 1 + quantidade) % quantidade;   // do primeiro vai para o último
        }

        if (Ctx.Entrada.ApertouBaixo)
        {
            Ctx.Audio.Tocar("menu_mover");
            return (selecionada + 1) % quantidade;                // do último volta ao primeiro
        }

        return selecionada;
    }
}
