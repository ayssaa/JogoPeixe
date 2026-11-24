// =============================================================================
//  Cenario.cs
//  Desenha o fundo do jogo em camadas que se movem em velocidades diferentes
//  (efeito "parallax"): o que está longe anda devagar, o que está perto anda
//  rápido. Isso dá sensação de profundidade.
//
//  Camadas (de trás para frente):
//    1. Céu (parado, com farol e ilha)
//    2. Fundo do mar (lento)
//    3. Algas decorativas (médio, escurecidas para não parecerem obstáculos)
//    4. Ondas da superfície (desenhadas por último, por cima do peixe)
//
//  Como fazer uma imagem "infinita"? Repetimos a imagem lado a lado. Para não
//  aparecer um corte entre uma cópia e outra, as cópias de índice ímpar são
//  desenhadas espelhadas: assim a borda direita de uma encosta na borda
//  direita (igual) da próxima, e a emenda some.
// =============================================================================

using System;
using System.Collections.Generic;
using JogoPeixe.Nucleo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Jogo;

public class Cenario
{
    private readonly Recursos _recursos;

    // Quanto cada camada já "andou" (em pixels). Só cresce.
    private float _deslocamentoFundo;
    private float _deslocamentoSuperficie;

    // Algas decorativas: posição X e tamanho de cada uma.
    private readonly List<Vector2> _algas = new List<Vector2>();  // X = posição, Y = escala
    private readonly Random _sorteio = new Random();

    // Tempo total, usado para pequenas animações (balanço das algas).
    private float _tempo;

    public Cenario(Recursos recursos)
    {
        _recursos = recursos;

        // Espalha algumas algas pelo fundo, com distâncias e tamanhos sorteados.
        float x = 0f;
        while (x < Config.LarguraTela + 400f)
        {
            _algas.Add(new Vector2(x, 0.55f + (float)_sorteio.NextDouble() * 0.45f));
            x += 160f + (float)_sorteio.NextDouble() * 260f;
        }
    }

    /// <summary>Move as camadas. "velocidadeMundo" é a velocidade (px/s) da camada mais próxima.</summary>
    public void Atualizar(float dt, float velocidadeMundo)
    {
        _tempo += dt;
        _deslocamentoFundo += velocidadeMundo * Config.ParallaxFundo * dt;
        _deslocamentoSuperficie += velocidadeMundo * Config.ParallaxSuperficie * dt;

        float distanciaAlgas = velocidadeMundo * Config.ParallaxAlgas * dt;

        for (int i = 0; i < _algas.Count; i++)
        {
            Vector2 alga = _algas[i];
            alga.X -= distanciaAlgas;

            // Saiu pela esquerda? Volta lá para a direita, depois da última alga.
            if (alga.X < -150f)
            {
                float maisDireita = 0f;
                foreach (Vector2 outra in _algas)
                    maisDireita = Math.Max(maisDireita, outra.X);

                alga.X = maisDireita + 160f + (float)_sorteio.NextDouble() * 260f;
                alga.Y = 0.55f + (float)_sorteio.NextDouble() * 0.45f;
            }

            _algas[i] = alga;   // Vector2 é "struct" (cópia), então precisamos gravar de volta
        }
    }

    /// <summary>Desenha céu, fundo do mar e algas (tudo que fica ATRÁS do peixe).</summary>
    public void DesenharFundo(SpriteBatch lote)
    {
        // 1) Céu: esticado na largura toda, parado.
        lote.Draw(_recursos.Ceu,
                  new Rectangle(0, 0, Config.LarguraTela, (int)Config.AlturaCeu + 10),
                  Color.White);

        // 2) Fundo do mar: escalado para ocupar da superfície até o fim da tela.
        float alturaFundo = Config.AlturaTela - Config.TopoFundoMar;
        float escalaFundo = alturaFundo / _recursos.FundoMar.Height;
        DesenharRepetido(lote, _recursos.FundoMar, Config.TopoFundoMar, escalaFundo, _deslocamentoFundo, Color.White);

        // 3) Algas decorativas, escurecidas e azuladas (parecem mais distantes
        //    e o jogador não confunde com os obstáculos de verdade).
        Color corAlga = new Color(70, 110, 150) * 0.75f;
        foreach (Vector2 alga in _algas)
        {
            // Pequeno balanço usando seno: cada alga balança num ritmo diferente.
            float balanco = (float)Math.Sin(_tempo * 1.6f + alga.X * 0.01f) * 0.06f;
            _recursos.Alga.Desenhar(lote, 0, new Vector2(alga.X, Config.AlturaTela + 6f),
                                    _recursos.Alga.BaseCentral, alga.Y, corAlga, balanco);
        }
    }

    /// <summary>Desenha as ondas da superfície (fica NA FRENTE do peixe e atrás do barco).</summary>
    public void DesenharSuperficie(SpriteBatch lote)
    {
        DesenharRepetido(lote, _recursos.Superficie, Config.TopoSuperficie,
                         Config.EscalaSuperficie, _deslocamentoSuperficie, Color.White);
    }

    /// <summary>
    /// Desenha uma textura repetida na horizontal, cobrindo a tela inteira,
    /// deslocada para a esquerda conforme "deslocamento".
    /// </summary>
    private static void DesenharRepetido(SpriteBatch lote, Texture2D textura, float y,
                                         float escala, float deslocamento, Color cor)
    {
        float larguraPeca = textura.Width * escala;

        // Qual é a primeira peça visível? (divisão inteira do deslocamento)
        int primeira = (int)Math.Floor(deslocamento / larguraPeca);

        // Desenha peças até cobrir a tela toda (+1 de sobra).
        int quantidade = (int)Math.Ceiling(Config.LarguraTela / larguraPeca) + 1;

        for (int i = primeira; i <= primeira + quantidade; i++)
        {
            float x = i * larguraPeca - deslocamento;

            // Peças ímpares espelhadas: esconde a emenda entre uma cópia e outra.
            SpriteEffects espelho = (i % 2 != 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Math.Floor no X evita uma linha fina de 1 pixel entre as peças.
            lote.Draw(textura, new Vector2((float)Math.Floor(x), y), null, cor, 0f, Vector2.Zero,
                      new Vector2((larguraPeca + 1f) / textura.Width, escala), espelho, 0f);
        }
    }
}
