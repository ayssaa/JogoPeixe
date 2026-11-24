// =============================================================================
//  FitaSprites.cs
//  Uma "fita" é uma imagem com vários quadros (frames) lado a lado, todos do
//  mesmo tamanho. Ex.: peixe_nadando.png tem 5 quadros na horizontal.
//
//      +------+------+------+------+------+
//      |  0   |  1   |  2   |  3   |  4   |
//      +------+------+------+------+------+
//
//  Esta classe sabe recortar o quadro certo e desenhá-lo na tela.
// =============================================================================

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Nucleo;

public class FitaSprites
{
    /// <summary>A imagem completa, com todos os quadros.</summary>
    public Texture2D Textura { get; }

    /// <summary>Quantidade de quadros na fita.</summary>
    public int Quadros { get; }

    /// <summary>Largura de UM quadro (largura total ÷ quantidade de quadros).</summary>
    public int LarguraQuadro { get; }

    /// <summary>Altura de um quadro (igual à altura da imagem).</summary>
    public int AlturaQuadro { get; }

    /// <summary>Ponto central de um quadro. Útil como "origem" para girar o sprite
    /// em torno do próprio centro.</summary>
    public Vector2 Centro => new Vector2(LarguraQuadro / 2f, AlturaQuadro / 2f);

    /// <summary>Ponto no meio da base do quadro. Usado para objetos "apoiados"
    /// (corais no fundo do mar, barco na água).</summary>
    public Vector2 BaseCentral => new Vector2(LarguraQuadro / 2f, AlturaQuadro);

    public FitaSprites(Texture2D textura, int quadros)
    {
        Textura = textura;
        Quadros = quadros;
        LarguraQuadro = textura.Width / quadros;
        AlturaQuadro = textura.Height;
    }

    /// <summary>Retângulo, dentro da textura, que corresponde ao quadro pedido.</summary>
    public Rectangle Recorte(int quadro)
    {
        // O operador % (resto da divisão) garante que o índice nunca passe do limite.
        int indice = ((quadro % Quadros) + Quadros) % Quadros;
        return new Rectangle(indice * LarguraQuadro, 0, LarguraQuadro, AlturaQuadro);
    }

    /// <summary>Desenha um quadro da fita.</summary>
    /// <param name="lote">SpriteBatch já iniciado (Begin) por quem chamou.</param>
    /// <param name="quadro">Índice do quadro a desenhar.</param>
    /// <param name="posicao">Onde o ponto de "origem" ficará na tela.</param>
    /// <param name="origem">Ponto do quadro que fica sobre a posição (ex.: Centro).</param>
    /// <param name="escala">1 = tamanho original.</param>
    /// <param name="cor">Cor multiplicada (Color.White = sem alteração).</param>
    /// <param name="rotacao">Rotação em radianos.</param>
    /// <param name="efeito">Permite espelhar a imagem.</param>
    public void Desenhar(SpriteBatch lote, int quadro, Vector2 posicao, Vector2 origem,
                         float escala, Color cor, float rotacao = 0f,
                         SpriteEffects efeito = SpriteEffects.None)
    {
        lote.Draw(Textura, posicao, Recorte(quadro), cor, rotacao, origem,
                  escala, efeito, 0f);
    }
}
