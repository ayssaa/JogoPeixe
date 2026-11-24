// =============================================================================
//  FonteBitmap.cs
//  Desenha texto usando uma imagem com todas as letras (fonte "bitmap").
//
//  Por que não usar o SpriteFont do MonoGame? O SpriteFont precisa do
//  "Content Pipeline" e de a fonte estar instalada no computador de quem
//  compila. Com uma imagem pronta, o projeto funciona em qualquer máquina.
//
//  A fonte usada é a "Press Start 2P" (licença SIL Open Font License),
//  em estilo pixel art, que combina com o visual do jogo.
//
//  Organização da imagem fonte.png:
//    - Cada letra ocupa uma célula de 32x32 pixels.
//    - São 16 células por linha.
//    - A ordem das letras está no arquivo fonte.txt.
//  Como a fonte é "monoespaçada", toda letra tem a mesma largura, o que
//  simplifica muito o cálculo da largura do texto.
// =============================================================================

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Nucleo;

public class FonteBitmap
{
    // Tamanho (em pixels) de cada célula da imagem.
    private const int TamanhoCelula = 32;

    // Quantas células existem por linha na imagem.
    private const int Colunas = 16;

    // Imagem com todas as letras.
    private readonly Texture2D _textura;

    // Dicionário: letra -> posição (índice) dela na imagem.
    // Dictionary faz essa busca de forma muito rápida.
    private readonly Dictionary<char, int> _indices = new Dictionary<char, int>();

    public FonteBitmap(Texture2D textura, string caracteresNaOrdem)
    {
        _textura = textura;

        for (int i = 0; i < caracteresNaOrdem.Length; i++)
            _indices[caracteresNaOrdem[i]] = i;
    }

    /// <summary>A fonte tem desenho para esse caractere?</summary>
    public bool Suporta(char caractere) => _indices.ContainsKey(caractere);

    /// <summary>Largura (px) que o texto ocupará na tela.
    /// "tamanho" é a altura das letras em pixels (ex.: 16, 24, 32).</summary>
    public float MedirLargura(string texto, float tamanho)
    {
        // Fonte monoespaçada: cada letra avança exatamente "tamanho" pixels.
        return texto.Length * tamanho;
    }

    /// <summary>Desenha o texto a partir do canto superior esquerdo "posicao".</summary>
    public void Desenhar(SpriteBatch lote, string texto, Vector2 posicao, Color cor, float tamanho)
    {
        float escala = tamanho / TamanhoCelula;
        Vector2 cursor = posicao;

        foreach (char caractere in texto)
        {
            // Letras que a fonte não conhece viram "?" para não quebrar o jogo.
            if (!_indices.TryGetValue(caractere, out int indice))
                _indices.TryGetValue('?', out indice);

            // Espaço não precisa ser desenhado, só avança o cursor.
            if (caractere != ' ')
            {
                var recorte = new Rectangle(
                    (indice % Colunas) * TamanhoCelula,   // coluna
                    (indice / Colunas) * TamanhoCelula,   // linha
                    TamanhoCelula, TamanhoCelula);

                lote.Draw(_textura, cursor, recorte, cor, 0f, Vector2.Zero, escala,
                          SpriteEffects.None, 0f);
            }

            cursor.X += tamanho;
        }
    }

    /// <summary>Desenha o texto com um contorno escuro ao redor.
    /// O contorno deixa o texto legível sobre qualquer fundo (céu, água, etc.).</summary>
    public void DesenharComContorno(SpriteBatch lote, string texto, Vector2 posicao,
                                    Color cor, float tamanho, Color? corContorno = null)
    {
        // "Color?" significa que o parâmetro pode ser nulo; "??" usa o valor padrão.
        Color contorno = corContorno ?? new Color(10, 20, 45);

        // Espessura do contorno = 1 "pixel" do desenho original da fonte (que é 8x8).
        float e = tamanho / 8f;

        // Desenha o texto 8 vezes deslocado em volta (formando a borda)...
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                Desenhar(lote, texto, posicao + new Vector2(dx * e, dy * e), contorno, tamanho);
            }
        }

        // ...e por último o texto na cor principal, por cima.
        Desenhar(lote, texto, posicao, cor, tamanho);
    }

    /// <summary>Desenha o texto centralizado horizontalmente em "centroX".</summary>
    public void DesenharCentralizado(SpriteBatch lote, string texto, float centroX, float y,
                                     Color cor, float tamanho, bool contorno = true)
    {
        float x = centroX - MedirLargura(texto, tamanho) / 2f;
        var posicao = new Vector2(x, y);

        if (contorno)
            DesenharComContorno(lote, texto, posicao, cor, tamanho);
        else
            Desenhar(lote, texto, posicao, cor, tamanho);
    }
}
