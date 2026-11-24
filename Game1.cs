// =============================================================================
//  Game1.cs
//  Classe principal do MonoGame. O MonoGame chama, em loop, ~60 vezes por segundo:
//      Update(gameTime)  -> atualiza a lógica (movimento, colisões, menus)
//      Draw(gameTime)    -> desenha a tela
//
//  Aqui ficam só as coisas GLOBAIS: janela, tela cheia, som ligado/desligado
//  e a troca entre telas. Cada tela (menu, jogo, ranking...) cuida do resto.
//
//  RESOLUÇÃO: o jogo é sempre desenhado em uma "tela virtual" de 1280x720
//  (um RenderTarget). No final, essa imagem é esticada para o tamanho da
//  janela, com faixas pretas se a proporção for diferente. Assim o jogo
//  funciona igual em qualquer tamanho de janela ou monitor.
// =============================================================================

using System;
using JogoPeixe.Jogadores;
using JogoPeixe.Jogo;
using JogoPeixe.Nucleo;
using JogoPeixe.Telas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JogoPeixe;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graficos;
    private SpriteBatch _lote;
    private RenderTarget2D _telaVirtual;

    private Entrada _entrada;
    private Contexto _contexto;
    private Tela _telaAtual;

    // Aviso rápido no topo da tela (ex.: "SOM DESLIGADO").
    private string _aviso;
    private float _tempoAviso;

    public Game1()
    {
        _graficos = new GraphicsDeviceManager(this);

        // Tamanho inicial da janela (igual à tela virtual).
        _graficos.PreferredBackBufferWidth = Config.LarguraTela;
        _graficos.PreferredBackBufferHeight = Config.AlturaTela;

        // Tela cheia "sem borda": troca mais rápido e sem piscar o monitor.
        _graficos.HardwareModeSwitch = false;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.Title = Config.TituloJanela;
    }

    protected override void Initialize()
    {
        _entrada = new Entrada();

        // Toda vez que uma letra for digitada na janela, ela vai para a fila da Entrada.
        // "(remetente, args) => ..." é uma função anônima (lambda) usada como tratador do evento.
        Window.TextInput += (remetente, args) => _entrada.RegistrarCaractere(args.Character);

        base.Initialize();   // o MonoGame chama LoadContent() aqui dentro
    }

    protected override void LoadContent()
    {
        _lote = new SpriteBatch(GraphicsDevice);
        _telaVirtual = new RenderTarget2D(GraphicsDevice, Config.LarguraTela, Config.AlturaTela);

        var recursos = new Recursos(GraphicsDevice);
        var audio = new Audio();
        var repositorio = new RepositorioJogadores();   // já carrega o JSON

        _contexto = new Contexto(this, _lote, recursos, audio, _entrada, repositorio);

        _telaAtual = new TelaMenu(_contexto);
        _telaAtual.Entrar();
    }

    protected override void Update(GameTime gameTime)
    {
        // dt = segundos desde o último quadro (normalmente ~0,016 s a 60 FPS).
        // Limitamos a 0,05 s: se o jogo travar (ex.: arrastando a janela),
        // os objetos não "teleportam" um pedaço enorme de uma vez.
        float dt = Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 0.05f);

        _entrada.Atualizar();

        // ---------------- Teclas globais (funcionam em qualquer tela) ----------------
        if (_entrada.Apertou(Keys.M))
        {
            _contexto.Audio.AlternarMudo();
            MostrarAviso(_contexto.Audio.Mudo ? "SOM DESLIGADO" : "SOM LIGADO");
        }

        if (_entrada.Apertou(Keys.F11) ||
            (_entrada.Apertou(Keys.Enter) && (_entrada.Segurando(Keys.LeftAlt) || _entrada.Segurando(Keys.RightAlt))))
        {
            _graficos.ToggleFullScreen();
        }

        _tempoAviso = Math.Max(0f, _tempoAviso - dt);

        // ---------------- Tela atual ----------------
        _telaAtual.Atualizar(dt);

        // Se a tela pediu para trocar, troca agora (no fim do quadro).
        if (_contexto.ProximaTela != null)
        {
            _telaAtual = _contexto.ProximaTela;
            _contexto.LimparProximaTela();
            _telaAtual.Entrar();
        }

        base.Update(gameTime);
    }

    private void MostrarAviso(string texto)
    {
        _aviso = texto;
        _tempoAviso = 1.5f;
    }

    protected override void Draw(GameTime gameTime)
    {
        // 1) Desenha o jogo na tela virtual de 1280x720.
        GraphicsDevice.SetRenderTarget(_telaVirtual);
        GraphicsDevice.Clear(new Color(10, 40, 90));

        _telaAtual.Desenhar();

        if (_tempoAviso > 0f)
        {
            SpriteBatch lote = _contexto.IniciarDesenho();
            _contexto.Recursos.Fonte.DesenharCentralizado(lote, _aviso, Config.LarguraTela / 2f, 130f,
                                                          Color.White * Math.Min(1f, _tempoAviso * 2f), 18f);
            _contexto.FinalizarDesenho();
        }

        // 2) Volta a desenhar na janela de verdade e copia a tela virtual esticada.
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);   // faixas pretas, se sobrar espaço

        _lote.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp);
        _lote.Draw(_telaVirtual, CalcularAreaDestino(), Color.White);
        _lote.End();

        base.Draw(gameTime);
    }

    /// <summary>
    /// Maior retângulo com proporção 16:9 que cabe na janela, centralizado.
    /// Ex.: janela 1920x1200 -> imagem 1920x1080 com faixas de 60 px em cima e embaixo.
    /// </summary>
    private Rectangle CalcularAreaDestino()
    {
        int larguraJanela = GraphicsDevice.PresentationParameters.BackBufferWidth;
        int alturaJanela = GraphicsDevice.PresentationParameters.BackBufferHeight;

        float escala = Math.Min((float)larguraJanela / Config.LarguraTela,
                                (float)alturaJanela / Config.AlturaTela);

        int largura = (int)(Config.LarguraTela * escala);
        int altura = (int)(Config.AlturaTela * escala);

        return new Rectangle((larguraJanela - largura) / 2, (alturaJanela - altura) / 2, largura, altura);
    }
}
