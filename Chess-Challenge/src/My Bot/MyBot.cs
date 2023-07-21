using ChessChallenge.API;

public class MyBot : IChessBot
{

	private static readonly int[] PieceValues = { 0, 1, 3, 3, 5, 9, 100 }; // Values for Pawn, Knight, Bishop, Rook, Queen, and King
	private static readonly int[] PieceMobility = { 0, 1, 3, 3, 5, 9, 0 }; // Mobility values for each piece type

	private static readonly int[,,] SquareTables = {
	{
		{ 0,  0,  0,  0,  0,  0,  0,  0 },
		{ 50, 50, 50, 50, 50, 50, 50, 50 },
		{ 10, 10, 20, 30, 30, 20, 10, 10 },
		{ 5,  5, 10, 25, 25, 10,  5,  5 },
		{ 0,  0,  0, 20, 20,  0,  0,  0 },
		{ 5, -5,-10,  0,  0,-10, -5,  5 },
		{ 5, 10, 10,-20,-20, 10, 10,  5 },
		{ 0,  0,  0,  0,  0,  0,  0,  0 }
	},
	{
		{-50,-40,-30,-30,-30,-30,-40,-50},
		{-40,-20,  0,  0,  0,  0,-20,-40},
		{-30,  0, 10, 15, 15, 10,  0,-30},
		{-30,  5, 15, 20, 20, 15,  5,-30},
		{-30,  0, 15, 20, 20, 15,  0,-30},
		{-30,  5, 10, 15, 15, 10,  5,-30},
		{-40,-20,  0,  5,  5,  0,-20,-40},
		{-50,-40,-30,-30,-30,-30,-40,-50}
	},
	{
		{-20,-10,-10,-10,-10,-10,-10,-20},
		{-10,  0,  0,  0,  0,  0,  0,-10},
		{-10,  0,  5, 10, 10,  5,  0,-10},
		{-10,  5,  5, 10, 10,  5,  5,-10},
		{-10,  0, 10, 10, 10, 10,  0,-10},
		{-10, 10, 10, 10, 10, 10, 10,-10},
		{-10,  5,  0,  0,  0,  0,  5,-10},
		{-20,-10,-10,-10,-10,-10,-10,-20}
	},
	{
		{ 0,  0,  0,  0,  0,  0,  0,  0},
		{ 5, 10, 10, 10, 10, 10, 10,  5},
		{-5,  0,  0,  0,  0,  0,  0, -5},
		{-5,  0,  0,  0,  0,  0,  0, -5},
		{-5,  0,  0,  0,  0,  0,  0, -5},
		{-5,  0,  0,  0,  0,  0,  0, -5},
		{-5,  0,  0,  0,  0,  0,  0, -5},
		{ 0,  0,  0,  5,  5,  0,  0,  0}
	},
	{
		{-20,-10,-10, -5, -5,-10,-10,-20},
		{-10,  0,  0,  0,  0,  0,  0,-10},
		{-10,  0,  5,  5,  5,  5,  0,-10},
		{ -5,  0,  5,  5,  5,  5,  0, -5},
		{  0,  0,  5,  5,  5,  5,  0, -5},
		{-10,  5,  5,  5,  5,  5,  0,-10},
		{-10,  0,  5,  0,  0,  0,  0,-10},
		{-20,-10,-10, -5, -5,-10,-10,-20}
	},
	{
		{ 20, 30, 10,  0,  0, 10, 30, 20},
		{ 20, 20,  0,  0,  0,  0, 20, 20},
		{-10,-20,-20,-20,-20,-20,-20,-10},
		{-20,-30,-30,-40,-40,-30,-30,-20},
		{-30,-40,-40,-50,-50,-40,-40,-30},
		{-30,-40,-40,-50,-50,-40,-40,-30},
		{-30,-40,-40,-50,-50,-40,-40,-30},
		{-30,-40,-40,-50,-50,-40,-40,-30}
	}
};

	private string _lastMoveText;
	private bool debugMode = true;
	private string _bestMoveText;

	private int moveNum = 0;

	public Move Think(Board board, Timer timer)
	{
		debugPrint($"[#{++moveNum}] Evaluating move...");
		Move[] moves = board.GetLegalMoves();
		Move bestMove = moves[0];
		int bestScore = int.MinValue;

		foreach (Move move in moves)
		{
			int score = EvaluateMove(move, board);
			if (score > bestScore)
			{
				bestScore = score;
				bestMove = move;
				_bestMoveText = _lastMoveText;
			}
		}

		debugPrint($"[#{moveNum}] Best move: {_bestMoveText}");
		return bestMove;
	}

	private int EvaluateMove(Move move, Board board)
	{
		int score = 0;
		Square startSquare = move.StartSquare;
		Piece movingPiece = board.GetPiece(startSquare);

		// Evaluating capture moves.
		int captureScore = 0;
		if (move.IsCapture)
		{
			score += captureScore = PieceValues[(int)move.CapturePieceType] * 10;
		}

		// Evaluating promotion moves.
		int promotionScore = 0;
		if (move.IsPromotion)
		{
			score += promotionScore = PieceValues[(int)move.PromotionPieceType] - PieceValues[(int)PieceType.Pawn]; // Score the promotion piece higher than a pawn.
		}

		// Evaluate piece mobility
		int mobilityScore = CalculateMobility(board, board.IsWhiteToMove);
		score += mobilityScore;

		// Evaluate positional advantage
		int positionalScore = 0;
		score += positionalScore = SquareTables[(int)movingPiece.PieceType - 1, move.TargetSquare.Rank, move.TargetSquare.File]; // Adjust the positional score based on the average positional advantage of the bot's pieces.

		// Evaluate king safety.
		if (board.IsInCheck())
		{
			// Penalize moves that put the king in check.
			score -= 100;
		}

		// Check for checkmate.
		if (board.IsInCheckmate())
		{
			// Assign a very high score for a checkmate move.
			return int.MaxValue - 1;
		}

		_lastMoveText = $"{move.MovePieceType} {startSquare.Name} -> {move.TargetSquare.Name}	scores: total:{score}	capture:{captureScore}	promotion:{promotionScore}	mobility:{mobilityScore}	position:{positionalScore}";
		debugPrint($"[#{moveNum}] Evaluated move: {_lastMoveText}");

		return score;
	}

	private int CalculateMobility(Board board, bool isWhite)
	{
		int mobility = 0;
		Move[] moves = board.GetLegalMoves();
		foreach (Move move in moves)
		{
			//int pieceValue = PieceValues[(int)board.GetPiece(move.StartSquare).PieceType];
			int pieceMobility = PieceMobility[(int)board.GetPiece(move.StartSquare).PieceType];
			mobility += pieceMobility * (board.GetPiece(move.StartSquare).IsWhite == isWhite ? 1 : -1);
		}
		return mobility;
	}

	private void debugPrint(string message)
	{
		if (debugMode)
		{
			System.Console.WriteLine(message);
		}
	}

}