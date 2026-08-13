namespace Cunt.WordGame;

public class GameContext(ulong userId) {
  public ulong UserId { get; } = userId;
  public List<GuessResult[]> GuessLog { get; } = new();
}

