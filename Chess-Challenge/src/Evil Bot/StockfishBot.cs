using System;
using ChessChallenge.API;
using Stockfish.NET;

public class StockfishBot : IChessBot
{
	const int STOCKFISH_LEVEL = 2;

	IStockfish mStockFish;

	public StockfishBot()
	{
		Stockfish.NET.Models.Settings stockfishSettings = new Stockfish.NET.Models.Settings();
		stockfishSettings.SkillLevel = STOCKFISH_LEVEL;

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