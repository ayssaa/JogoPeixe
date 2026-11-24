// =============================================================================
//  Config.cs
//  Reúne TODAS as constantes de ajuste do jogo em um só lugar.
//  Quer deixar o jogo mais fácil ou mais difícil? Mude os números aqui,
//  sem precisar procurar valores "mágicos" espalhados pelo código.
// =============================================================================

namespace JogoPeixe.Jogo;

/// <summary>
/// Classe estática: não precisa (nem pode) ser instanciada.
/// Usamos "const" para valores fixos conhecidos em tempo de compilação.
/// </summary>
public static class Config
{
    // ------------------------------------------------------------------
    //  Tela
    // ------------------------------------------------------------------

    /// <summary>Largura da "tela virtual". Todo o jogo é desenhado nesse tamanho
    /// e depois esticado para caber na janela (mantendo a proporção).</summary>
    public const int LarguraTela = 1280;

    /// <summary>Altura da tela virtual.</summary>
    public const int AlturaTela = 720;

    /// <summary>Título que aparece na barra da janela.</summary>
    public const string TituloJanela = "Jogo do Peixe";

    // ------------------------------------------------------------------
    //  Cenário (posições verticais das camadas, em pixels da tela virtual)
    // ------------------------------------------------------------------

    /// <summary>Altura em que a imagem do céu termina (o céu é esticado de 0 até aqui).</summary>
    public const float AlturaCeu = 190f;

    /// <summary>Onde começa (topo) a faixa de ondas da superfície.</summary>
    public const float TopoSuperficie = 168f;

    /// <summary>Escala da faixa de ondas da superfície.</summary>
    public const float EscalaSuperficie = 1.5f;

    /// <summary>Onde começa a imagem do fundo do mar.</summary>
    public const float TopoFundoMar = 222f;

    /// <summary>Linha d'água: altura (Y) em que o barco "flutua".</summary>
    public const float LinhaDagua = 196f;

    /// <summary>Velocidade das camadas em relação à velocidade do mundo
    /// (efeito parallax: o que está longe se move mais devagar).</summary>
    public const float ParallaxFundo = 0.25f;
    public const float ParallaxSuperficie = 0.6f;
    public const float ParallaxAlgas = 0.75f;

    // ------------------------------------------------------------------
    //  Área em que o peixe pode nadar
    // ------------------------------------------------------------------

    /// <summary>Limite superior: o peixe não pode subir acima disso (não sai da água
    /// e não fica escondido atrás das ondas da superfície).</summary>
    public const float LimiteSuperiorPeixe = 292f;

    /// <summary>Limite inferior (fundo do mar).</summary>
    public const float LimiteInferiorPeixe = 690f;

    /// <summary>Limite esquerdo da tela para o peixe.</summary>
    public const float LimiteEsquerdoPeixe = 50f;

    /// <summary>O peixe não pode ir além de 70% da largura da tela, para o jogador
    /// sempre ter tempo de ver os obstáculos chegando pela direita.</summary>
    public const float LimiteDireitoPeixe = LarguraTela * 0.70f;

    // ------------------------------------------------------------------
    //  Peixe (jogador)
    // ------------------------------------------------------------------

    /// <summary>Velocidade máxima do peixe, em pixels por segundo.</summary>
    public const float VelocidadePeixe = 380f;

    /// <summary>Quão rápido o peixe atinge a velocidade desejada (maior = mais "duro").</summary>
    public const float AceleracaoPeixe = 9f;

    /// <summary>Tamanho do peixe na tela em relação ao sprite original.</summary>
    public const float EscalaPeixe = 0.75f;

    /// <summary>Tempo (s) em que o peixe fica piscando e imune depois de levar dano.</summary>
    public const float TempoInvulneravel = 1.8f;

    /// <summary>Quantidade de vidas no começo da partida.</summary>
    public const int VidasIniciais = 3;

    // ------------------------------------------------------------------
    //  Mundo / dificuldade
    // ------------------------------------------------------------------

    /// <summary>Velocidade com que o cenário e os obstáculos andam no início (px/s).</summary>
    public const float VelocidadeMundoInicial = 260f;

    /// <summary>Velocidade máxima do mundo (px/s).</summary>
    public const float VelocidadeMundoMaxima = 640f;

    /// <summary>Segundos de partida até a dificuldade chegar ao máximo (1.0).</summary>
    public const float TempoAteDificuldadeMaxima = 150f;

    /// <summary>Distância (px) que vale 1 ponto. Pontos = distância percorrida / esse valor.</summary>
    public const float PixelsPorPonto = 10f;

    /// <summary>A cada quantos pontos toca o som de "marco" (comemoração).</summary>
    public const int PontosPorMarco = 500;

    // ------------------------------------------------------------------
    //  Obstáculos
    // ------------------------------------------------------------------

    /// <summary>Intervalo (s) entre obstáculos no começo e no fim da curva de dificuldade.</summary>
    public const float IntervaloObstaculoInicial = 1.7f;
    public const float IntervaloObstaculoFinal = 0.65f;

    // ------------------------------------------------------------------
    //  Pescador
    // ------------------------------------------------------------------

    /// <summary>Escala do barco do pescador.</summary>
    public const float EscalaPescador = 0.8f;

    /// <summary>Tempo (s) entre o fim de uma pescaria e o início do próximo aviso.</summary>
    public const float IntervaloPescaInicial = 3.8f;
    public const float IntervaloPescaFinal = 1.5f;

    /// <summary>Tempo (s) que a marcação fica na água antes do anzol cair.</summary>
    public const float TempoAvisoInicial = 1.7f;
    public const float TempoAvisoFinal = 0.85f;

    /// <summary>Tempo (s) que o anzol leva para chegar até a marcação.</summary>
    public const float TempoLancamento = 0.22f;

    /// <summary>Raio (px) da área de captura.</summary>
    public const float RaioPescaInicial = 62f;
    public const float RaioPescaFinal = 76f;

    /// <summary>Faixa horizontal em que o barco pode ficar. O mínimo evita que o barco
    /// fique por trás dos corações (HUD) no canto superior esquerdo.</summary>
    public const float BarcoXMinimo = 320f;
    public const float BarcoXMaximo = 900f;

    /// <summary>Velocidade máxima com que o barco persegue o peixe (px/s).</summary>
    public const float VelocidadeBarcoInicial = 150f;
    public const float VelocidadeBarcoFinal = 300f;

    // ------------------------------------------------------------------
    //  Jogadores
    // ------------------------------------------------------------------

    /// <summary>Tamanho máximo do nome de um jogador.</summary>
    public const int TamanhoMaximoNome = 12;

    /// <summary>Quantos jogadores aparecem no ranking.</summary>
    public const int TamanhoRanking = 10;

    /// <summary>Nome do arquivo JSON onde os jogadores são salvos.</summary>
    public const string ArquivoJogadores = "jogadores.json";

    /// <summary>Pasta (dentro da pasta de dados do usuário) onde o JSON fica.</summary>
    public const string PastaDados = "JogoPeixe";
}
