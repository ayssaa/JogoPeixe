// =============================================================================
//  Program.cs
//  Ponto de entrada do programa: é a primeira coisa que roda.
//  Cria o jogo (Game1) e inicia o loop principal do MonoGame.
//  "using var" garante que os recursos do jogo sejam liberados ao fechar.
// =============================================================================

using var game = new JogoPeixe.Game1();
game.Run();
