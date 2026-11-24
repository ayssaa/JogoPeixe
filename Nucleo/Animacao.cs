// =============================================================================
//  Animacao.cs
//  Controla QUAL quadro de uma FitaSprites deve aparecer em cada momento.
//  Ela só conta o tempo; quem desenha é a FitaSprites.
// =============================================================================

namespace JogoPeixe.Nucleo;

public class Animacao
{
    /// <summary>A fita cujos quadros serão animados.</summary>
    public FitaSprites Fita { get; }

    /// <summary>Quadros por segundo (velocidade da animação). Pode mudar durante o jogo.</summary>
    public float QuadrosPorSegundo { get; set; }

    /// <summary>Se true, volta ao primeiro quadro ao terminar. Se false, para no último.</summary>
    public bool Repetir { get; set; }

    /// <summary>Índice do quadro que deve ser desenhado agora.</summary>
    public int QuadroAtual { get; private set; }

    /// <summary>Para animações sem repetição: indica que o último quadro já foi alcançado.</summary>
    public bool Terminou { get; private set; }

    // Tempo acumulado desde a última troca de quadro.
    private float _acumulado;

    public Animacao(FitaSprites fita, float quadrosPorSegundo, bool repetir = true)
    {
        Fita = fita;
        QuadrosPorSegundo = quadrosPorSegundo;
        Repetir = repetir;
    }

    /// <summary>Avança a animação. "dt" é o tempo (em segundos) desde o último quadro do jogo.</summary>
    public void Atualizar(float dt)
    {
        if (Terminou || QuadrosPorSegundo <= 0f)
            return;

        _acumulado += dt;
        float duracaoQuadro = 1f / QuadrosPorSegundo;

        // "while" (e não "if") porque, se o jogo travar um pouco, pode ser preciso
        // pular mais de um quadro de uma vez para a animação não ficar atrasada.
        while (_acumulado >= duracaoQuadro)
        {
            _acumulado -= duracaoQuadro;
            QuadroAtual++;

            if (QuadroAtual >= Fita.Quadros)
            {
                if (Repetir)
                {
                    QuadroAtual = 0;
                }
                else
                {
                    QuadroAtual = Fita.Quadros - 1;
                    Terminou = true;
                    break;
                }
            }
        }
    }

    /// <summary>Volta a animação para o começo.</summary>
    public void Reiniciar()
    {
        QuadroAtual = 0;
        _acumulado = 0f;
        Terminou = false;
    }
}
