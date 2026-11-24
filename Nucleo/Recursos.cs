// =============================================================================
//  Recursos.cs
//  Carrega TODAS as imagens do jogo uma única vez, no início.
//  Depois, qualquer parte do código pega a imagem já pronta daqui.
//
//  As imagens são PNGs comuns na pasta Assets (copiada junto com o .exe).
//  Usamos Texture2D.FromStream em vez do Content Pipeline do MonoGame
//  para deixar o projeto mais simples: basta trocar o PNG e rodar de novo.
// =============================================================================

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Nucleo;

public class Recursos
{
    // Pasta "Assets" ao lado do executável.
    // AppContext.BaseDirectory = pasta onde o .exe está rodando.
    public static readonly string PastaAssets = Path.Combine(AppContext.BaseDirectory, "Assets");

    private readonly GraphicsDevice _dispositivo;

    // ---------------- Personagens ----------------
    public FitaSprites PeixeNadando { get; }   // 5 quadros
    public FitaSprites PeixeDano { get; }      // 3 quadros (normal, vermelho, vermelho com brilho)
    public FitaSprites Pescador { get; }       // 5 quadros (parado, preparando, lançando, puxando, com peixe)

    // ---------------- Obstáculos ----------------
    public FitaSprites[] Corais { get; }       // 5 corais/pedras diferentes (1 quadro cada)
    public FitaSprites AguaViva { get; }       // 4 quadros
    public FitaSprites Lata { get; }
    public FitaSprites Garrafa { get; }
    public FitaSprites Pneu { get; }
    public FitaSprites Sacola { get; }
    public FitaSprites Rede { get; }

    // ---------------- Efeitos e interface ----------------
    public FitaSprites Marcacao { get; }       // 3 quadros: alvo sólido, tracejado, tracejado fraco
    public FitaSprites Vida { get; }           // 4 quadros: coração "batendo" (do menor ao maior)
    public FitaSprites Bolhas { get; }         // 4 quadros de grupos de bolhas
    public FitaSprites Alga { get; }           // alga decorativa do cenário
    public FitaSprites Logo { get; }           // logo do menu

    // ---------------- Cenário ----------------
    public Texture2D Ceu { get; }
    public Texture2D Superficie { get; }
    public Texture2D FundoMar { get; }

    // ---------------- Texto ----------------
    public FonteBitmap Fonte { get; }

    public Recursos(GraphicsDevice dispositivo)
    {
        _dispositivo = dispositivo;

        // Textura de 1x1 pixel branco, usada para desenhar formas (ver Desenho.cs).
        var pixel = new Texture2D(dispositivo, 1, 1);
        pixel.SetData(new[] { Color.White });
        Desenho.Pixel = pixel;

        // O segundo número de cada chamada é a quantidade de quadros da imagem.
        PeixeNadando = Fita("Sprites/peixe_nadando.png", 5);
        PeixeDano    = Fita("Sprites/peixe_dano.png", 3);
        Pescador     = Fita("Sprites/pescador.png", 5);

        Corais = new FitaSprites[5];
        for (int i = 0; i < Corais.Length; i++)
            Corais[i] = Fita($"Sprites/coral{i + 1}.png", 1);   // coral1.png ... coral5.png

        AguaViva = Fita("Sprites/agua_viva.png", 4);
        Lata     = Fita("Sprites/lata.png", 1);
        Garrafa  = Fita("Sprites/garrafa.png", 1);
        Pneu     = Fita("Sprites/pneu.png", 1);
        Sacola   = Fita("Sprites/sacola.png", 1);
        Rede     = Fita("Sprites/rede.png", 1);

        Marcacao = Fita("Sprites/marcacao.png", 3);
        Vida     = Fita("Sprites/vida.png", 4);
        Bolhas   = Fita("Sprites/bolhas.png", 4);
        Alga     = Fita("Sprites/alga.png", 1);
        Logo     = Fita("Sprites/logo.png", 1);

        Ceu        = CarregarTextura("Cenario/ceu.png");
        Superficie = CarregarTextura("Cenario/superficie.png");
        FundoMar   = CarregarTextura("Cenario/fundo_mar.png");

        // A ordem das letras na imagem está escrita no arquivo fonte.txt.
        string letras = File.ReadAllText(Path.Combine(PastaAssets, "Fonte/fonte.txt"));
        Fonte = new FonteBitmap(CarregarTextura("Fonte/fonte.png"), letras);
    }

    /// <summary>Carrega um PNG e já cria a FitaSprites correspondente.</summary>
    private FitaSprites Fita(string caminhoRelativo, int quadros)
    {
        return new FitaSprites(CarregarTextura(caminhoRelativo), quadros);
    }

    /// <summary>Lê um PNG do disco e transforma em Texture2D.</summary>
    private Texture2D CarregarTextura(string caminhoRelativo)
    {
        string caminho = Path.Combine(PastaAssets, caminhoRelativo);

        if (!File.Exists(caminho))
            throw new FileNotFoundException($"Imagem não encontrada: {caminho}");

        // "using" fecha o arquivo automaticamente quando o bloco termina.
        using FileStream arquivo = File.OpenRead(caminho);
        Texture2D textura = Texture2D.FromStream(_dispositivo, arquivo);

        PremultiplicarAlfa(textura);
        return textura;
    }

    /// <summary>
    /// O MonoGame desenha (BlendState.AlphaBlend) esperando cores "pré-multiplicadas":
    /// cada canal de cor já vem multiplicado pela transparência do pixel.
    /// PNGs comuns não vêm assim, então fazemos a conta aqui uma única vez.
    /// Sem isso, as bordas dos sprites ficariam com um contorno claro estranho.
    /// </summary>
    private static void PremultiplicarAlfa(Texture2D textura)
    {
        var pixels = new Color[textura.Width * textura.Height];
        textura.GetData(pixels);

        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            pixels[i] = new Color(
                (byte)(c.R * c.A / 255),
                (byte)(c.G * c.A / 255),
                (byte)(c.B * c.A / 255),
                c.A);
        }

        textura.SetData(pixels);
    }
}
