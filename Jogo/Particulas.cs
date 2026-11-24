// =============================================================================
//  Particulas.cs
//  Pequenos efeitos visuais: bolhas saindo do peixe, bolhas quando o anzol
//  cai na água, etc. Cada partícula vive pouco tempo e depois some.
// =============================================================================

using System;
using System.Collections.Generic;
using JogoPeixe.Nucleo;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JogoPeixe.Jogo;

public class Particulas
{
    /// <summary>
    /// Uma partícula. É uma "struct" (tipo por valor): leve e ideal para dados
    /// pequenos que são criados e descartados com frequência.
    /// </summary>
    private struct Particula
    {
        public Vector2 Posicao;
        public Vector2 Velocidade;
        public float Vida;          // segundos restantes
        public float VidaTotal;     // vida inicial (para calcular a transparência)
        public float Escala;
        public int Quadro;          // qual quadro da fita de bolhas usar
    }

    private readonly List<Particula> _particulas = new List<Particula>();
    private readonly FitaSprites _bolhas;
    private readonly Random _sorteio = new Random();

    public Particulas(Recursos recursos)
    {
        _bolhas = recursos.Bolhas;
    }

    /// <summary>Cria "quantidade" bolhas perto de "origem".</summary>
    /// <param name="forca">Quanto as bolhas se espalham (1 = normal).</param>
    public void EmitirBolhas(Vector2 origem, int quantidade, float forca = 1f)
    {
        for (int i = 0; i < quantidade; i++)
        {
            float vida = 0.6f + (float)_sorteio.NextDouble() * 0.8f;

            _particulas.Add(new Particula
            {
                Posicao = origem + new Vector2(Sortear(-10f, 10f), Sortear(-8f, 8f)),
                Velocidade = new Vector2(Sortear(-60f, 30f) * forca, Sortear(-90f, -40f) * forca),  // sobem (Y negativo)
                Vida = vida,
                VidaTotal = vida,
                Escala = Sortear(0.18f, 0.35f) * (0.8f + forca * 0.2f),
                Quadro = _sorteio.Next(_bolhas.Quadros)
            });
        }
    }

    /// <summary>Move as partículas e remove as que "morreram".</summary>
    /// <param name="velocidadeMundo">As bolhas são levadas para trás junto com o cenário.</param>
    public void Atualizar(float dt, float velocidadeMundo)
    {
        // Percorremos de trás para frente porque vamos remover itens da lista:
        // removendo do fim para o começo, os índices que ainda faltam não mudam.
        for (int i = _particulas.Count - 1; i >= 0; i--)
        {
            Particula p = _particulas[i];
            p.Vida -= dt;

            if (p.Vida <= 0f || p.Posicao.Y < Config.LimiteSuperiorPeixe - 30f)
            {
                _particulas.RemoveAt(i);
                continue;
            }

            p.Posicao += p.Velocidade * dt;
            p.Posicao.X -= velocidadeMundo * 0.5f * dt;
            _particulas[i] = p;
        }
    }

    public void Desenhar(SpriteBatch lote)
    {
        foreach (Particula p in _particulas)
        {
            // Some aos poucos: transparência proporcional à vida restante.
            float opacidade = MathHelper.Clamp(p.Vida / p.VidaTotal, 0f, 1f);
            _bolhas.Desenhar(lote, p.Quadro, p.Posicao, _bolhas.Centro, p.Escala, Color.White * opacidade);
        }
    }

    private float Sortear(float minimo, float maximo) =>
        minimo + (float)_sorteio.NextDouble() * (maximo - minimo);
}
