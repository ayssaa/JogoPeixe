// =============================================================================
//  Audio.cs
//  Carrega e toca os sons (efeitos curtos) e as músicas (em loop).
//
//  Todos os sons são arquivos .wav na pasta Assets/Sons, gerados em estilo
//  8-bit (chiptune) para combinar com o visual em pixel art.
//  Arquivos cujo nome começa com "musica_" são tratados como música de fundo.
//
//  Se o computador não tiver placa/dispositivo de som, o jogo continua
//  funcionando normalmente, só que mudo (ver o try/catch no construtor).
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace JogoPeixe.Nucleo;

public class Audio
{
    // Efeitos sonoros por nome (ex.: "splash" -> splash.wav).
    private readonly Dictionary<string, SoundEffect> _efeitos = new Dictionary<string, SoundEffect>();

    // Músicas: guardamos uma "instância" de cada, porque instâncias podem
    // tocar em loop, pausar e continuar.
    private readonly Dictionary<string, SoundEffectInstance> _musicas = new Dictionary<string, SoundEffectInstance>();

    // Música tocando agora (null se nenhuma).
    private SoundEffectInstance _musicaAtual;
    private string _nomeMusicaAtual;

    // false se não foi possível iniciar o áudio.
    private readonly bool _disponivel = true;

    /// <summary>Volume geral das músicas (0 a 1).</summary>
    public float VolumeMusica { get; set; } = 0.45f;

    /// <summary>Volume geral dos efeitos (0 a 1).</summary>
    public float VolumeEfeitos { get; set; } = 0.8f;

    /// <summary>Se true, nenhum som é tocado (tecla M alterna).</summary>
    public bool Mudo { get; private set; }

    public Audio()
    {
        string pasta = Path.Combine(Recursos.PastaAssets, "Sons");

        try
        {
            foreach (string arquivo in Directory.GetFiles(pasta, "*.wav"))
            {
                string nome = Path.GetFileNameWithoutExtension(arquivo);

                using FileStream fluxo = File.OpenRead(arquivo);
                SoundEffect som = SoundEffect.FromStream(fluxo);

                if (nome.StartsWith("musica_"))
                {
                    SoundEffectInstance instancia = som.CreateInstance();
                    instancia.IsLooped = true;   // música recomeça sozinha ao acabar
                    _musicas[nome] = instancia;
                }
                else
                {
                    _efeitos[nome] = som;
                }
            }
        }
        catch (Exception erro)
        {
            // Sem som não é motivo para fechar o jogo: apenas avisamos no console.
            Console.WriteLine($"[Audio] Som desativado: {erro.Message}");
            _disponivel = false;
        }
    }

    /// <summary>Toca um efeito sonoro pelo nome do arquivo (sem ".wav").</summary>
    /// <param name="volume">0 a 1.</param>
    /// <param name="variacaoTom">-1 (mais grave) a 1 (mais agudo). Pequenas variações
    /// evitam que sons repetidos fiquem cansativos.</param>
    public void Tocar(string nome, float volume = 1f, float variacaoTom = 0f)
    {
        if (!_disponivel || Mudo)
            return;

        if (_efeitos.TryGetValue(nome, out SoundEffect som))
            som.Play(volume * VolumeEfeitos, variacaoTom, 0f);
        else
            Console.WriteLine($"[Audio] Som não encontrado: {nome}");
    }

    /// <summary>Começa a tocar uma música em loop (para a anterior, se for outra).</summary>
    public void TocarMusica(string nome)
    {
        if (!_disponivel)
            return;

        // Já está tocando essa? Não faz nada (evita recomeçar do zero).
        if (_nomeMusicaAtual == nome && _musicaAtual != null)
            return;

        PararMusica();

        if (_musicas.TryGetValue(nome, out SoundEffectInstance musica))
        {
            _musicaAtual = musica;
            _nomeMusicaAtual = nome;
            _musicaAtual.Volume = Mudo ? 0f : VolumeMusica;
            _musicaAtual.Play();
        }
    }

    /// <summary>Para a música atual.</summary>
    public void PararMusica()
    {
        _musicaAtual?.Stop();   // "?." só chama Stop se não for null
        _musicaAtual = null;
        _nomeMusicaAtual = null;
    }

    /// <summary>Liga/desliga todo o som.</summary>
    public void AlternarMudo()
    {
        Mudo = !Mudo;

        if (_musicaAtual != null)
            _musicaAtual.Volume = Mudo ? 0f : VolumeMusica;
    }
}
