// =============================================================================
//  Hud.cs
//  HUD = "heads-up display": as informações desenhadas por cima do jogo
//  (vidas, pontuação, recorde, nome do jogador).
// =============================================================================

using JogoPeixe.Jogadores;
using JogoPeixe.Nucleo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Jogo;

public static class Hud
{
    // Cores reaproveitadas.
    private static readonly Color Amarelo = new Color(255, 220, 60);
    private static readonly Color Branco = Color.White;
    private static readonly Color AzulClaro = new Color(170, 225, 255);

    // Relógio interno para o coração "batendo".
    private static float _tempo;

    public static void Atualizar(float dt) => _tempo += dt;

    public static void Desenhar(SpriteBatch lote, Recursos recursos, Partida partida, Jogador jogador)
    {
        FonteBitmap fonte = recursos.Fonte;

        // ---------------- Vidas (canto superior esquerdo) ----------------
        FitaSprites coracao = recursos.Vida;
        const float escalaCoracao = 0.55f;
        float espacamento = coracao.LarguraQuadro * escalaCoracao + 6f;

        for (int i = 0; i < Config.VidasIniciais; i++)
        {
            Vector2 posicao = new Vector2(30f + i * espacamento + coracao.LarguraQuadro * escalaCoracao / 2f, 42f);
            bool temVida = i < partida.Vidas;

            if (temVida)
            {
                // Na última vida, o coração "bate" (alterna entre os 4 tamanhos) para alertar.
                int quadro = 3;
                if (partida.Vidas == 1)
                    quadro = (int)(_tempo * 8f) % coracao.Quadros;

                coracao.Desenhar(lote, quadro, posicao, coracao.Centro, escalaCoracao, Color.White);
            }
            else
            {
                // Vida perdida: coração apagado (cinza e transparente).
                coracao.Desenhar(lote, 3, posicao, coracao.Centro, escalaCoracao, new Color(40, 40, 60) * 0.6f);
            }
        }

        // Nome do jogador logo abaixo dos corações.
        if (jogador != null)
            fonte.DesenharComContorno(lote, jogador.Nome, new Vector2(24f, 78f), AzulClaro, 16f);

        // ---------------- Pontuação (canto superior direito) ----------------
        int recordeAnterior = jogador?.Recorde ?? 0;
        bool batendoRecorde = jogador != null && partida.Pontos > recordeAnterior && recordeAnterior > 0;

        // Formato "D6": sempre 6 dígitos, com zeros à esquerda (000123).
        string pontos = partida.Pontos.ToString("D6");

        // Brilho ao passar de um marco: o número cresce e fica amarelo por um instante.
        float tamanhoPontos = 32f + partida.BrilhoMarco * 8f;
        Color corPontos = partida.BrilhoMarco > 0f ? Color.Lerp(Branco, Amarelo, partida.BrilhoMarco) : Branco;

        float direita = Config.LarguraTela - 24f;
        fonte.DesenharComContorno(lote, "PONTOS", new Vector2(direita - fonte.MedirLargura("PONTOS", 16f), 18f), AzulClaro, 16f);
        fonte.DesenharComContorno(lote, pontos, new Vector2(direita - fonte.MedirLargura(pontos, tamanhoPontos), 40f), corPontos, tamanhoPontos);

        string textoRecorde = batendoRecorde ? "NOVO RECORDE!" : $"RECORDE {recordeAnterior}";
        Color corRecorde = batendoRecorde ? Amarelo : AzulClaro;
        fonte.DesenharComContorno(lote, textoRecorde,
            new Vector2(direita - fonte.MedirLargura(textoRecorde, 16f), 82f), corRecorde, 16f);
    }
}
