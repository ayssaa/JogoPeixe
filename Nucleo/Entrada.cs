// =============================================================================
//  Entrada.cs
//  Centraliza a leitura do teclado.
//
//  O MonoGame só diz se uma tecla está apertada AGORA. Para saber se ela
//  "acabou de ser apertada" (um único toque), guardamos o estado do quadro
//  anterior e comparamos os dois.
// =============================================================================

using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace JogoPeixe.Nucleo;

public class Entrada
{
    // Estado do teclado neste quadro e no quadro anterior.
    private KeyboardState _atual;
    private KeyboardState _anterior;

    // Fila com os caracteres digitados (usada na tela de cadastro de nome).
    // Uma fila (Queue) devolve os itens na mesma ordem em que entraram.
    private readonly Queue<char> _caracteresDigitados = new Queue<char>();

    /// <summary>Deve ser chamado UMA vez por quadro, antes de qualquer leitura.</summary>
    public void Atualizar()
    {
        _anterior = _atual;            // o que era "agora" vira "antes"
        _atual = Keyboard.GetState();  // lê o teclado de novo
    }

    /// <summary>A tecla está sendo segurada neste quadro?</summary>
    public bool Segurando(Keys tecla) => _atual.IsKeyDown(tecla);

    /// <summary>A tecla foi apertada exatamente neste quadro (não estava antes)?</summary>
    public bool Apertou(Keys tecla) => _atual.IsKeyDown(tecla) && !_anterior.IsKeyDown(tecla);

    /// <summary>Alguma das teclas informadas foi apertada neste quadro?
    /// "params" permite chamar assim: ApertouAlguma(Keys.A, Keys.B, Keys.C).</summary>
    public bool ApertouAlguma(params Keys[] teclas)
    {
        foreach (Keys tecla in teclas)
        {
            if (Apertou(tecla))
                return true;
        }
        return false;
    }

    // ------------------------------------------------------------------
    //  Atalhos para os comandos usados no jogo (setas OU WASD)
    // ------------------------------------------------------------------

    public bool SegurandoCima => Segurando(Keys.Up) || Segurando(Keys.W);
    public bool SegurandoBaixo => Segurando(Keys.Down) || Segurando(Keys.S);
    public bool SegurandoEsquerda => Segurando(Keys.Left) || Segurando(Keys.A);
    public bool SegurandoDireita => Segurando(Keys.Right) || Segurando(Keys.D);

    public bool ApertouCima => ApertouAlguma(Keys.Up, Keys.W);
    public bool ApertouBaixo => ApertouAlguma(Keys.Down, Keys.S);

    /// <summary>Confirmar: Enter ou Espaço.</summary>
    public bool ApertouConfirmar => ApertouAlguma(Keys.Enter, Keys.Space);

    /// <summary>Voltar/cancelar: Esc.</summary>
    public bool ApertouVoltar => Apertou(Keys.Escape);

    // ------------------------------------------------------------------
    //  Texto digitado
    // ------------------------------------------------------------------

    /// <summary>Chamado pelo Game1 sempre que a janela recebe um caractere digitado.
    /// Usar o evento de texto da janela (em vez de teclas) garante acentos,
    /// maiúsculas e o layout correto do teclado (ABNT2, por exemplo).</summary>
    public void RegistrarCaractere(char caractere)
    {
        _caracteresDigitados.Enqueue(caractere);
    }

    /// <summary>Tenta retirar o próximo caractere digitado da fila.
    /// Retorna false quando não há mais caracteres. O "out" devolve o caractere.</summary>
    public bool TentarLerCaractere(out char caractere)
    {
        return _caracteresDigitados.TryDequeue(out caractere);
    }

    /// <summary>Descarta caracteres antigos (ex.: ao entrar na tela de nome,
    /// para não aproveitar letras digitadas durante o jogo).</summary>
    public void LimparTexto()
    {
        _caracteresDigitados.Clear();
    }
}
