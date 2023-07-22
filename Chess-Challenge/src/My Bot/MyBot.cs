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

	//private List<Move> moves = new List<Move>();

	private string _lastMoveText;
	private string _bestMoveText;

	private int moveNum = 0;

	private static int LogLevel = 2;

	private int maxDepth = 5;

	private static string[] ShortNames = { "", "P", "N", "B", "R", "Q", "K" };

	public Move Think(Board board, Timer timer)
	{
		Print(1, $"[#{++moveNum}] Evaluating move...");
		Move[] moves = board.GetLegalMoves();
		Move bestMove = moves[0];
		int bestScore = int.MinValue;
		int movesEvaluated = 0;
		foreach (Move move in moves)
		{
			int score = EvaluateMove(move, board);
			string movesEvaledString = (++movesEvaluated).ToString("D" + moves.Length.ToString().Length);
			Print(1, $"[#{moveNum}] Evaluated move {movesEvaledString}/{moves.Length}: {_lastMoveText}");
			if (score > bestScore)
			{
				bestScore = score;
				bestMove = move;
				_bestMoveText = _lastMoveText;
			}
		}

		Print(2, $"[#{moveNum}] Best move: {_bestMoveText}");
		//moves.Add(bestMove);
		return bestMove;
	}

	private int EvaluateMove(Move move, Board board)
	{

		board.MakeMove(move);
		if (board.IsInCheckmate())
		{
			board.UndoMove(move);
			return int.MaxValue;
		}
		if (board.IsDraw())
		{
			board.UndoMove(move);
			return int.MinValue;
		}
		board.UndoMove(move);

		int score = 0;
		Square startSquare = move.StartSquare;
		Piece movingPiece = board.GetPiece(startSquare);

		// Evaluating captures
		int captureScore = 0;
		if (move.IsCapture)
		{
			score += captureScore = PieceValues[(int)move.CapturePieceType] * 10;
		}

		// Evaluating promotions
		int promotionScore = 0;
		if (move.IsPromotion)
		{
			// Score the promotion piece higher than a pawn.
			score += promotionScore = PieceValues[(int)move.PromotionPieceType] - PieceValues[(int)PieceType.Pawn];
		}

		// Evaluate piece mobility
		int mobilityScore = CalculateMobility(board, board.IsWhiteToMove);
		score += mobilityScore;

		// Evaluate positional advantage
		int positionalScore = 0;
		// Adjust the positional score based on the average positional advantage of the bot's pieces.
		score += positionalScore = GetSquareValue(move, board.IsWhiteToMove);

		// Evaluate king safety
		if (board.IsInCheck())
		{
			// Penalize moves that put the king in check.
			score -= 100;
		}

		// Check for checkmate
		if (board.IsInCheckmate())
		{
			// Assign a very high score for a checkmate move.
			return int.MaxValue - 1;
		}

		_lastMoveText = $"{ShortNames[(int)move.MovePieceType]} {startSquare.Name} -> {move.TargetSquare.Name}	score:{score}	capture:{captureScore}	promotion:{promotionScore}	mobility:{mobilityScore}	position:{positionalScore}";

		return score;
	}

	private int GetSquareValue(Move move, bool isWhite)
	{
		int rank = isWhite ? move.TargetSquare.Rank : 7 - move.TargetSquare.Rank;
		int file = isWhite ? move.TargetSquare.File : 7 - move.TargetSquare.File;
		return SquareTables[(int)move.MovePieceType - 1, rank, file];
	}

	private int CalculateMobility(Board board, bool isWhite)
	{
		int mobility = 0;
		Move[] moves = board.GetLegalMoves();
		foreach (Move move in moves)
		{
			int moveMobility = 0;
			Piece piece = board.GetPiece(move.StartSquare);
			int pieceValue = PieceValues[(int)piece.PieceType];
			int pieceMobility = PieceMobility[(int)piece.PieceType] * pieceValue * (piece.IsWhite == isWhite ? 1 : -1);
			moveMobility += pieceMobility;

			int capturedPieceValue = 0;
			if (move.IsCapture)
			{
				capturedPieceValue = PieceValues[(int)piece.PieceType];
				moveMobility += capturedPieceValue * pieceValue; // Encourage capturing valuable pieces
			}

			int attackedSquareValue = GetSquareValue(move, isWhite);
			moveMobility += attackedSquareValue;

			Print(0, $"	Mobility for move {ShortNames[(int)move.MovePieceType]} {move.StartSquare.Name} -> {move.TargetSquare.Name}	piece:{pieceMobility}	capture:{capturedPieceValue}	attack:{attackedSquareValue}");
			mobility += moveMobility;
		}
		return mobility;
	}

	private static void Print(int logLevel, string message)
	{
		if (logLevel >= LogLevel)
		{
			System.Console.WriteLine(message);
		}
	}

}