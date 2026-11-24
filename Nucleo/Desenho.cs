// =============================================================================
//  Desenho.cs
//  Funções de ajuda para desenhar formas simples (retângulos, linhas, painéis)
//  usando uma textura de 1x1 pixel branco esticada e colorida.
// =============================================================================

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Nucleo;

public static class Desenho
{
    /// <summary>Textura de 1 pixel branco. É criada pela classe Recursos ao carregar o jogo.</summary>
    public static Texture2D Pixel { get; set; }

    /// <summary>Retângulo preenchido.</summary>
    public static void Retangulo(SpriteBatch lote, Rectangle area, Color cor)
    {
        lote.Draw(Pixel, area, cor);
    }

    /// <summary>Apenas a borda de um retângulo.</summary>
    public static void Contorno(SpriteBatch lote, Rectangle area, Color cor, int espessura = 2)
    {
        Retangulo(lote, new Rectangle(area.X, area.Y, area.Width, espessura), cor);                          // cima
        Retangulo(lote, new Rectangle(area.X, area.Bottom - espessura, area.Width, espessura), cor);         // baixo
        Retangulo(lote, new Rectangle(area.X, area.Y, espessura, area.Height), cor);                         // esquerda
        Retangulo(lote, new Rectangle(area.Right - espessura, area.Y, espessura, area.Height), cor);         // direita
    }

    /// <summary>Painel semitransparente com borda, usado como fundo dos menus.</summary>
    public static void Painel(SpriteBatch lote, Rectangle area)
    {
        Retangulo(lote, area, new Color(8, 28, 60) * 0.94f);             // fundo azul-escuro (quase opaco)
        Contorno(lote, area, new Color(120, 200, 255), 3);               // borda azul-clara
        var interno = new Rectangle(area.X + 6, area.Y + 6, area.Width - 12, area.Height - 12);
        Contorno(lote, interno, new Color(60, 120, 190) * 0.8f, 2);      // borda interna
    }

    /// <summary>Linha reta entre dois pontos (usada para a linha de pesca).
    /// Truque: esticamos o pixel até o comprimento da linha e giramos no ângulo certo.</summary>
    public static void Linha(SpriteBatch lote, Vector2 inicio, Vector2 fim, Color cor, float espessura = 2f)
    {
        Vector2 diferenca = fim - inicio;
        float comprimento = diferenca.Length();
        float angulo = (float)Math.Atan2(diferenca.Y, diferenca.X);

        lote.Draw(Pixel, inicio, null, cor, angulo,
                  new Vector2(0f, 0.5f),                       // origem: meio da borda esquerda
                  new Vector2(comprimento, espessura),          // escala X = comprimento, Y = espessura
                  SpriteEffects.None, 0f);
    }

    /// <summary>Escurece a tela inteira (usado atrás de menus e na pausa).</summary>
    public static void Escurecer(SpriteBatch lote, float intensidade)
    {
        Retangulo(lote, new Rectangle(0, 0, Jogo.Config.LarguraTela, Jogo.Config.AlturaTela),
                  Color.Black * intensidade);
    }
}
