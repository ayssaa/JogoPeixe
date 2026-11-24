// =============================================================================
//  Obstaculo.cs
//  Qualquer coisa no mar que tira vida do peixe ao encostar.
//  Os obstáculos nascem à direita da tela e andam para a esquerda.
//
//  Tipos:
//    - Coral:    preso no fundo do mar.
//    - Lixo:     lata, garrafa, sacola ou pneu boiando e girando.
//    - AguaViva: animada, sobe e desce devagar.
//    - Rede:     grande, aparece quando o jogo fica mais difícil.
// =============================================================================

using System;
using JogoPeixe.Nucleo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Jogo;

/// <summary>"enum" lista as opções possíveis de um valor (aqui, o tipo do obstáculo).</summary>
public enum TipoObstaculo
{
    Coral,
    Lixo,
    AguaViva,
    Rede
}

public class Obstaculo
{
    private readonly FitaSprites _fita;
    private readonly Animacao _animacao;          // só a água-viva tem animação (os outros: null)
    private readonly float _escala;
    private readonly float _fatorVelocidade;       // 1 = anda junto com o mundo; >1 = mais rápido
    private readonly float _velocidadeRotacao;     // radianos por segundo (lixo girando)
    private readonly float _alturaBase;            // Y "central" em torno do qual oscila
    private readonly float _amplitude;             // quanto sobe/desce (px)
    private readonly float _fase;                  // desencontra o balanço de cada obstáculo
    private readonly Vector2 _origem;              // ponto de referência do desenho

    // Margens (em % do tamanho do desenho) que são retiradas da caixa de colisão.
    // Ex.: a sacola tem "alças" finas que não devem contar como colisão.
    private readonly float _margemX;
    private readonly float _margemY;

    private float _rotacao;
    private float _tempo;

    public TipoObstaculo Tipo { get; }

    /// <summary>Posição do ponto de origem (centro, ou base para corais).</summary>
    public Vector2 Posicao { get; private set; }

    /// <summary>Quando sai pela esquerda da tela, fica false e é removido da lista.</summary>
    public bool Ativo { get; private set; } = true;

    /// <summary>
    /// Construtor com muitos parâmetros: quem decide os valores é o GeradorObstaculos.
    /// Os parâmetros com "=" são opcionais.
    /// </summary>
    public Obstaculo(TipoObstaculo tipo, FitaSprites fita, Vector2 posicao, float escala,
                     float margemX, float margemY, float fatorVelocidade = 1f,
                     float velocidadeRotacao = 0f, float amplitude = 0f, float fase = 0f,
                     bool apoiadoNoFundo = false)
    {
        Tipo = tipo;
        _fita = fita;
        Posicao = posicao;
        _escala = escala;
        _margemX = margemX;
        _margemY = margemY;
        _fatorVelocidade = fatorVelocidade;
        _velocidadeRotacao = velocidadeRotacao;
        _alturaBase = posicao.Y;
        _amplitude = amplitude;
        _fase = fase;

        // Corais "apoiam" a base no chão; o resto usa o centro como referência.
        _origem = apoiadoNoFundo ? fita.BaseCentral : fita.Centro;

        if (fita.Quadros > 1)
            _animacao = new Animacao(fita, 6f);
    }

    /// <summary>Largura e altura desenhadas na tela.</summary>
    public float Largura => _fita.LarguraQuadro * _escala;
    public float Altura => _fita.AlturaQuadro * _escala;

    /// <summary>Caixa de colisão (menor que o desenho, de acordo com as margens).</summary>
    public Rectangle Hitbox
    {
        get
        {
            // Canto superior esquerdo do desenho (sem rotação).
            float esquerda = Posicao.X - _origem.X * _escala;
            float topo = Posicao.Y - _origem.Y * _escala;

            float cortaX = Largura * _margemX;
            float cortaY = Altura * _margemY;

            return new Rectangle(
                (int)(esquerda + cortaX),
                (int)(topo + cortaY),
                (int)(Largura - 2 * cortaX),
                (int)(Altura - 2 * cortaY));
        }
    }

    public void Atualizar(float dt, float velocidadeMundo)
    {
        _tempo += dt;

        // Anda para a esquerda.
        float x = Posicao.X - velocidadeMundo * _fatorVelocidade * dt;

        // Sobe e desce em onda (seno). Se amplitude = 0, fica na mesma altura.
        float y = _alturaBase + (float)Math.Sin(_tempo * 1.8f + _fase) * _amplitude;

        Posicao = new Vector2(x, y);
        _rotacao += _velocidadeRotacao * dt;
        _animacao?.Atualizar(dt);

        // Já saiu totalmente pela esquerda? Pode ser descartado.
        if (x + Largura < -20f)
            Ativo = false;
    }

    public void Desenhar(SpriteBatch lote)
    {
        int quadro = _animacao?.QuadroAtual ?? 0;   // "??": se não tem animação, usa o quadro 0
        _fita.Desenhar(lote, quadro, Posicao, _origem, _escala, Color.White, _rotacao);
    }
}
