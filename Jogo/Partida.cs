// =============================================================================
//  Partida.cs
//  Junta as peças de UMA partida: cenário, obstáculos, partículas,
//  vidas, pontuação e dificuldade.
//
//  A Partida não sabe nada de menus: ela só roda o jogo. Quem mostra
//  contagem, pausa e Game Over é a TelaJogo (pasta Telas).
//
//  >>> AINDA FALTA <<<
//  O PEIXE (personagem) e o PESCADOR ainda não existem. Os pontos onde eles
//  entram estão marcados com "TODO Peixe" e "TODO Pescador". Enquanto isso,
//  nada tira vida e a partida não termina sozinha (saia pelo Esc / pausa).
// =============================================================================

using System;
using System.Collections.Generic;
using JogoPeixe.Nucleo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Jogo;

public class Partida
{
    private readonly Contexto _ctx;
    private readonly GeradorObstaculos _gerador;
    private readonly Particulas _particulas;
    private readonly List<Obstaculo> _obstaculos = new List<Obstaculo>();

    private float _distancia;          // pixels percorridos desde o início
    private int _ultimoMarco;          // último múltiplo de PontosPorMarco já comemorado

    // TODO Peixe: public Peixe Peixe { get; }
    // TODO Pescador: public Pescador Pescador { get; }

    /// <summary>Vidas restantes.</summary>
    public int Vidas { get; private set; } = Config.VidasIniciais;

    /// <summary>Pontuação atual: distância percorrida dividida por PixelsPorPonto.</summary>
    public int Pontos => (int)(_distancia / Config.PixelsPorPonto);

    /// <summary>Segundos desde o início da partida.</summary>
    public float Tempo { get; private set; }

    /// <summary>0 (fácil) até 1 (máximo). Cresce com o tempo.</summary>
    public float Dificuldade => MathHelper.Clamp(Tempo / Config.TempoAteDificuldadeMaxima, 0f, 1f);

    /// <summary>Velocidade atual do mundo (px/s).</summary>
    public float VelocidadeMundo { get; private set; } = Config.VelocidadeMundoInicial;

    /// <summary>true quando as vidas acabam.</summary>
    public bool Terminou { get; private set; }

    /// <summary>Intensidade do tremor de tela (diminui sozinho). A TelaJogo usa para sacudir a câmera.</summary>
    public float Tremor { get; private set; }

    /// <summary>Tempo restante do "brilho" do placar ao passar de um marco de pontos.</summary>
    public float BrilhoMarco { get; private set; }

    /// <summary>Se true, desenha as caixas de colisão (tecla F3). Ajuda a entender e ajustar o jogo.</summary>
    public bool MostrarHitboxes { get; set; }

    public Partida(Contexto ctx)
    {
        _ctx = ctx;
        _gerador = new GeradorObstaculos(ctx.Recursos);
        _particulas = new Particulas(ctx.Recursos);

        // TODO Peixe: criar o peixe aqui.
        // TODO Pescador: criar o pescador e se inscrever nos eventos dele
        //               (ex.: quando fisgar o peixe -> PerderVida()).
    }

    // ==================================================================
    //  Atualização
    // ==================================================================

    public void Atualizar(float dt)
    {
        Tremor = Math.Max(0f, Tremor - dt);
        BrilhoMarco = Math.Max(0f, BrilhoMarco - dt);

        if (Terminou)
        {
            // Depois do fim, o mundo vai parando aos poucos.
            VelocidadeMundo = MathHelper.Lerp(VelocidadeMundo, 0f, 1f - (float)Math.Exp(-2f * dt));
        }
        else
        {
            Tempo += dt;
            VelocidadeMundo = MathHelper.Lerp(Config.VelocidadeMundoInicial, Config.VelocidadeMundoMaxima, Dificuldade);
            _distancia += VelocidadeMundo * dt;
            VerificarMarco();
        }

        _ctx.Cenario.Atualizar(dt, VelocidadeMundo);

        // TODO Peixe: atualizar o peixe.
        // TODO Pescador: atualizar o pescador.

        if (!Terminou)
            _gerador.Atualizar(dt, Dificuldade, _obstaculos);

        // Atualiza obstáculos e remove os que saíram da tela.
        // RemoveAll recebe uma condição (lambda) e apaga todos que a satisfazem.
        foreach (Obstaculo obstaculo in _obstaculos)
            obstaculo.Atualizar(dt, VelocidadeMundo);
        _obstaculos.RemoveAll(o => !o.Ativo);

        // TODO Peixe: verificar colisão do peixe com os obstáculos
        //             (obstaculo.Hitbox.Intersects(...)) e chamar PerderVida().

        _particulas.Atualizar(dt, VelocidadeMundo);
    }

    /// <summary>Toca um som e faz o placar brilhar a cada X pontos.</summary>
    private void VerificarMarco()
    {
        int marcoAtual = Pontos / Config.PontosPorMarco;   // divisão inteira
        if (marcoAtual > _ultimoMarco)
        {
            _ultimoMarco = marcoAtual;
            BrilhoMarco = 1f;
            _ctx.Audio.Tocar("marco", 0.7f);
        }
    }

    /// <summary>Tira uma vida e sacode a tela. Se acabarem, termina a partida.
    /// Ainda não é chamado por ninguém: use quando o peixe bater ou for fisgado.</summary>
    public void PerderVida()
    {
        Vidas--;
        Tremor = 0.35f;

        if (Vidas <= 0)
        {
            Vidas = 0;
            Terminou = true;
        }
    }

    // ==================================================================
    //  Desenho (a ordem importa: o que é desenhado depois fica na frente)
    // ==================================================================

    public void Desenhar(SpriteBatch lote)
    {
        _ctx.Cenario.DesenharFundo(lote);        // 1. céu, fundo do mar, algas
        // TODO Pescador: desenhar a marcação na água
        foreach (Obstaculo obstaculo in _obstaculos)
            obstaculo.Desenhar(lote);            // 2. obstáculos
        _particulas.Desenhar(lote);              // 3. bolhas
        // TODO Peixe: desenhar o peixe
        _ctx.Cenario.DesenharSuperficie(lote);   // 4. ondas por cima
        // TODO Pescador: desenhar o barco, a linha e o anzol

        if (MostrarHitboxes)
            DesenharHitboxes(lote);
    }

    /// <summary>Modo de depuração: mostra as áreas de colisão.</summary>
    private void DesenharHitboxes(SpriteBatch lote)
    {
        foreach (Obstaculo obstaculo in _obstaculos)
            Desenho.Contorno(lote, obstaculo.Hitbox, Color.Red, 2);
    }
}
