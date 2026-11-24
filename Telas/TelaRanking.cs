// =============================================================================
//  TelaRanking.cs
//  Mostra os melhores recordes (o "R" do CRUD), do maior para o menor.
// =============================================================================

using System.Collections.Generic;
using JogoPeixe.Jogadores;
using JogoPeixe.Jogo;
using JogoPeixe.Nucleo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Telas;

public class TelaRanking : Tela
{
    // Cores de ouro, prata e bronze para os 3 primeiros lugares.
    private static readonly Color[] CoresPodio =
    {
        new Color(255, 215, 60),
        new Color(210, 220, 235),
        new Color(225, 150, 90)
    };

    private readonly List<Jogador> _ranking;

    public TelaRanking(Contexto ctx) : base(ctx)
    {
        _ranking = ctx.Repositorio.Ranking();
    }

    protected override void AoAtualizar(float dt)
    {
        AtualizarFundo(dt);

        if (Ctx.Entrada.ApertouVoltar || Ctx.Entrada.ApertouConfirmar)
        {
            Ctx.Audio.Tocar("menu_voltar");
            Ctx.TrocarTela(new TelaMenu(Ctx));
        }
    }

    public override void Desenhar()
    {
        SpriteBatch lote = Ctx.IniciarDesenho();
        float centro = Config.LarguraTela / 2f;

        DesenharFundo(lote, 0.45f);
        var painel = new Rectangle((int)centro - 420, 40, 840, 600);
        Desenho.Painel(lote, painel);

        Fonte.DesenharCentralizado(lote, "RANKING", centro, 70f, CorTitulo, 40f);

        if (_ranking.Count == 0)
        {
            Fonte.DesenharCentralizado(lote, "NENHUM JOGADOR AINDA.", centro, 300f, CorTexto, 22f);
            Fonte.DesenharCentralizado(lote, "CADASTRE-SE NO MENU!", centro, 340f, CorSuave, 18f);
        }
        else
        {
            float colunaPosicao = painel.X + 70f;
            float colunaNome = painel.X + 170f;
            float colunaPontosDireita = painel.Right - 70f;

            for (int i = 0; i < _ranking.Count; i++)
            {
                Jogador jogador = _ranking[i];
                float y = 140f + i * 46f;

                // Destaque de fundo para o jogador atual.
                if (jogador == Ctx.JogadorAtual)
                    Desenho.Retangulo(lote, new Rectangle(painel.X + 30, (int)y - 8, painel.Width - 60, 40), new Color(120, 200, 255) * 0.2f);

                // 1º, 2º e 3º com cores de medalha; o resto em branco.
                Color cor = i < CoresPodio.Length ? CoresPodio[i] : CorTexto;

                Fonte.DesenharComContorno(lote, $"{i + 1}º", new Vector2(colunaPosicao, y), cor, 24f);
                Fonte.DesenharComContorno(lote, jogador.Nome, new Vector2(colunaNome, y), cor, 24f);

                string pontos = jogador.Recorde.ToString("D6");
                Fonte.DesenharComContorno(lote, pontos, new Vector2(colunaPontosDireita - Fonte.MedirLargura(pontos, 24f), y), cor, 24f);
            }
        }

        // Mostra onde os dados ficam salvos (útil para apresentar a persistência em JSON).
        Fonte.DesenharCentralizado(lote, "DADOS SALVOS EM:", centro, 596f, CorSuave, 12f);
        Fonte.DesenharCentralizado(lote, Encurtar(Ctx.Repositorio.CaminhoArquivo, 66), centro, 616f, CorSuave, 12f);

        DesenharRodape(lote, "ESC OU ENTER: VOLTAR");
        Ctx.FinalizarDesenho();
    }

    /// <summary>Se o texto for longo demais, mostra só o final com "..." no começo.</summary>
    private static string Encurtar(string texto, int maximo)
    {
        if (texto.Length <= maximo)
            return texto;
        return "..." + texto.Substring(texto.Length - (maximo - 3));
    }
}
