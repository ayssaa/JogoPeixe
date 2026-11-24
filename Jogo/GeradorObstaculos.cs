// =============================================================================
//  GeradorObstaculos.cs
//  Decide QUANDO e QUAL obstáculo aparece.
//
//  Quanto maior a dificuldade (0 a 1), menor o intervalo entre obstáculos e
//  mais chance de aparecerem os tipos difíceis (água-viva e rede).
// =============================================================================

using System;
using System.Collections.Generic;
using JogoPeixe.Nucleo;
using Microsoft.Xna.Framework;

namespace JogoPeixe.Jogo;

public class GeradorObstaculos
{
    private readonly Recursos _recursos;
    private readonly Random _sorteio = new Random();

    // Segundos até o próximo obstáculo. Começa com uma folga para o jogador se acostumar.
    private float _tempoAteProximo = 1.5f;

    public GeradorObstaculos(Recursos recursos)
    {
        _recursos = recursos;
    }

    /// <summary>Conta o tempo e, quando chega a hora, adiciona um obstáculo na lista.</summary>
    public void Atualizar(float dt, float dificuldade, List<Obstaculo> obstaculos)
    {
        _tempoAteProximo -= dt;
        if (_tempoAteProximo > 0f)
            return;

        obstaculos.Add(Criar(dificuldade));

        // Intervalo base diminui com a dificuldade; o sorteio (±25%) evita ritmo previsível.
        float intervalo = MathHelper.Lerp(Config.IntervaloObstaculoInicial,
                                          Config.IntervaloObstaculoFinal, dificuldade);
        _tempoAteProximo = intervalo * Sortear(0.75f, 1.25f);
    }

    /// <summary>Sorteia o tipo de obstáculo e cria com posição e tamanho adequados.</summary>
    private Obstaculo Criar(float dificuldade)
    {
        // "Pesos" de cada tipo. Água-viva e rede só aparecem depois de um tempo
        // e ficam mais comuns conforme a dificuldade aumenta.
        float pesoCoral = 3f;
        float pesoLixo = 3f;
        float pesoAguaViva = dificuldade > 0.1f ? 1f + dificuldade * 2f : 0f;
        float pesoRede = dificuldade > 0.3f ? dificuldade * 1.5f : 0f;

        float total = pesoCoral + pesoLixo + pesoAguaViva + pesoRede;
        float sorteio = Sortear(0f, total);

        // Para nascer fora da tela, à direita.
        float xInicial = Config.LarguraTela + 120f;

        // Sorteio por "faixas": se cair na faixa do coral, cria coral; senão, testa o próximo...
        if (sorteio < pesoCoral)
        {
            FitaSprites coral = _recursos.Corais[_sorteio.Next(_recursos.Corais.Length)];
            return new Obstaculo(TipoObstaculo.Coral, coral,
                new Vector2(xInicial, Config.AlturaTela + 8f),   // base um pouco abaixo da tela
                escala: Sortear(0.75f, 1.0f),
                margemX: 0.18f, margemY: 0.15f,
                apoiadoNoFundo: true);
        }
        sorteio -= pesoCoral;

        if (sorteio < pesoLixo)
        {
            // Escolhe um dos 4 tipos de lixo usando um array.
            FitaSprites[] lixos = { _recursos.Lata, _recursos.Garrafa, _recursos.Sacola, _recursos.Pneu };
            FitaSprites lixo = lixos[_sorteio.Next(lixos.Length)];

            return new Obstaculo(TipoObstaculo.Lixo, lixo,
                new Vector2(xInicial, Sortear(Config.LimiteSuperiorPeixe + 20f, Config.LimiteInferiorPeixe - 120f)),
                escala: Sortear(0.55f, 0.7f),
                margemX: 0.2f, margemY: 0.2f,
                fatorVelocidade: Sortear(1.0f, 1.3f),            // lixo leve "corre" um pouco mais
                velocidadeRotacao: Sortear(-1.5f, 1.5f),
                amplitude: Sortear(6f, 18f),
                fase: Sortear(0f, MathHelper.TwoPi));
        }
        sorteio -= pesoLixo;

        if (sorteio < pesoAguaViva)
        {
            return new Obstaculo(TipoObstaculo.AguaViva, _recursos.AguaViva,
                new Vector2(xInicial, Sortear(Config.LimiteSuperiorPeixe + 60f, Config.LimiteInferiorPeixe - 160f)),
                escala: Sortear(0.7f, 0.85f),
                margemX: 0.2f, margemY: 0.18f,
                fatorVelocidade: 0.85f,                           // água-viva é mais lenta
                amplitude: Sortear(35f, 60f),                     // mas sobe e desce bastante
                fase: Sortear(0f, MathHelper.TwoPi));
        }

        // Se não caiu em nenhuma faixa anterior, é rede.
        return new Obstaculo(TipoObstaculo.Rede, _recursos.Rede,
            new Vector2(xInicial + 60f, Sortear(Config.LimiteSuperiorPeixe + 40f, Config.LimiteInferiorPeixe - 180f)),
            escala: Sortear(0.9f, 1.1f),
            margemX: 0.12f, margemY: 0.15f,
            amplitude: 10f,
            fase: Sortear(0f, MathHelper.TwoPi));
    }

    /// <summary>Número decimal aleatório entre "minimo" e "maximo".</summary>
    private float Sortear(float minimo, float maximo)
    {
        return minimo + (float)_sorteio.NextDouble() * (maximo - minimo);
    }
}
