using System;
using ChessChallenge.API;
using Stockfish.NET;

public static class StockfishSettings
{
	public static int stockfishLevel = 3;
    static int[] eloLookup = { 1347, 1490, 1597, 1694, 1785, 1871, 1954, 2035, 2113, 2189, 2264, 2337, 2409, 2480, 2550, 2619, 2686, 2754, 2820, 2886 };

	public static int getElo()
	{
		return eloLookup[stockfishLevel];
	}
}

public class StockfishBot : IChessBot
{
	IStockfish mStockFish;

	public StockfishBot()
	{
		Stockfish.NET.Models.Settings stockfishSettings = new Stockfish.NET.Models.Settings();
		stockfishSettings.SkillLevel = StockfishSettings.stockfishLevel;

		mStockFish = new Stockfish.NET.Stockfish("C:\\Users\\blake\\Desktop\\Chess-Challenge\\Chess-Challenge\\resources\\stockfish\\stockfish12.exe", 2, stockfishSettings);
	}

	public Move Think(Board board, Timer timer)
	{
		string fen = board.GetFenString();
		mStockFish.SetFenPosition(fen);

		string bestMove = mStockFish.GetBestMoveTime(GetTime(board,timer));

		return new Move(bestMove, board);
	}

	// Basic time management
	public int GetTime(Board board, Timer timer)
	{
		return Math.Min(1000, timer.MillisecondsRemaining / 100);
	}
}