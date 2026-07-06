namespace Cunt.WordGame;

public class GameContext(ulong user) {
  public readonly ulong UserID = user;

  public readonly List<GuessResult[]> GuessLog = new();
}
