using System;
using ChessChallenge.API;

public class Flow : IChessBot
{
    string name = "FlowBotV2²";
    int[,] UniversalTable = new int[8, 8]
    {
            { -50,-40,-30,-30,-30,-30,-40,-50},
            { -40,-20,  0,  0,  0,  0,-20,-40},
            { -30,  0, 10, 15, 15, 10,  0,-30},
            { -30,  5, 15, 25, 25, 15,  5,-30},
            { -30,  0, 15, 25, 25, 15,  0,-30},
            { -30,  5, 10, 15, 15, 10,  5,-30},
            { -40,-20,  0,  5,  5,  0,-20,-40},
            { -50,-40,-30,-30,-30,-30,-40,-50}
    };
    int evaluatedMoves = 0;
    int depth = 4;

    public int RAZORING_THRESHOLD = 1;

    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    bool MoveIsDraw(Board board, Move move)
    {
        board.MakeMove(move);
        bool isDraw = board.IsDraw();
        board.UndoMove(move);
        return isDraw;
    }

    private int GetMVVLVAValue(Board board, Move move)
    {
        // Calculate the MVV-LVA value of the move
        Piece attacker = board.GetPiece(move.StartSquare);
        Piece victim = board.GetPiece(move.TargetSquare);
        int attackerValue = GetPieceValue(attacker.PieceType);
        int victimValue = GetPieceValue(victim.PieceType);
        int value = victimValue * 10 - attackerValue;

        // Add a bonus for capturing a piece with a higher value
        if (victimValue > attackerValue)
        {
            value += 50;
        }

        return value;
    }

    public int EvaluateBoard(Board board)
    {
        int TotalEvaluation = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int ii = 0; ii < 8; ii++)
            {
                Piece piece = board.GetPiece(new Square(i, ii));
                int pieceValue = GetPieceValue(piece.PieceType);
                TotalEvaluation += piece.IsWhite ? (pieceValue + UniversalTable[ii, i]) : (-pieceValue - UniversalTable[7 - ii, i]);
            }
        }
        return TotalEvaluation;
    }

    private int GetPieceValue(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.Pawn:
                return 100;
            case PieceType.Knight:
                return 300;
            case PieceType.Bishop:
                return 330;
            case PieceType.Rook:
                return 500;
            case PieceType.Queen:
                return 1000;
            case PieceType.King:
                return 20000;
            default:
                return 0;
        }
    }

    public Move Think(Board board, Timer timer)
    {
        if (timer.MillisecondsRemaining < 15000 && depth > 3)
        {
            depth--;
            Console.WriteLine($"Depth decreased to {depth}.");
        }

        evaluatedMoves = 0;

        Move[] moves = board.GetLegalMoves();

        Array.Sort(moves, (move1, move2) =>
        {
            if (move1.IsCapture && !move2.IsCapture)
            {
                return -1;
            }
            else if (!move1.IsCapture && move2.IsCapture)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        });

        Move bestMove = moves[0];
        int bestMoveEvaluation = board.IsWhiteToMove ? int.MinValue + 1 : int.MaxValue - 1;

        foreach (Move move in moves)
        {

            if (MoveIsDraw(board, move))
            {
                Console.WriteLine($"Avoiding draw: {move}");
                continue;
            }


            board.MakeMove(move);

            int evaluation = Minimax(board, depth, int.MinValue + 1, int.MaxValue - 1);
            board.UndoMove(move);

            if (bestMove == null || (board.IsWhiteToMove && evaluation > bestMoveEvaluation) || (!board.IsWhiteToMove && evaluation < bestMoveEvaluation))
            {
                bestMove = move;
                bestMoveEvaluation = evaluation;
            }
        }

        Console.WriteLine($"{name}: Best move: {bestMove} with evaluation {bestMoveEvaluation} and depth {depth}. Evaluated moves: {evaluatedMoves}");
        return bestMove;
    }

    private int GetRandomValue()
    {
        Random random = new Random();
        return random.Next(-10, 10);
    }

    private int QuiescenceSearch(Board board, int depth, int alpha, int beta)
    {
        if (depth == 0)
        {
            return EvaluateBoard(board);
        }

        Move[] moves = board.GetLegalMoves();

        int evaluation = EvaluateBoard(board);
        alpha = Math.Max(alpha, evaluation);
        if (beta <= alpha)
        {
            return evaluation;
        }

        foreach (Move move in moves)
        {
            if (move.IsCapture)
            {
                board.MakeMove(move);
                int newEvaluation = QuiescenceSearch(board, depth - 1, alpha, beta);
                board.UndoMove(move);

                evaluation = Math.Max(evaluation, newEvaluation);
                alpha = Math.Max(alpha, evaluation);
                if (beta <= alpha)
                {
                    return evaluation;
                }
            }
        }

        return evaluation;
    }

    private int Minimax(Board board, int depth, int alpha, int beta)
    {
        if (depth == 0)
        {
            return QuiescenceSearch(board, 1, alpha, beta);
        }

        Move[] moves = board.GetLegalMoves();

        if (depth <= RAZORING_THRESHOLD)
        {
            int razorValue = EvaluateBoard(board);
            if (board.IsWhiteToMove)
            {
                alpha = Math.Max(alpha, razorValue);
            }
            else
            {
                beta = Math.Min(beta, razorValue);
            }
            if (beta <= alpha)
            {
                return (board.IsWhiteToMove) ? alpha : beta;
            }
        }

        Array.Sort(moves, (move1, move2) =>
        {
            if (move1.IsCapture && !move2.IsCapture)
            {
                return -1;
            }
            else if (!move1.IsCapture && move2.IsCapture)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        });

        if (board.IsWhiteToMove)
        {
            int maxEvaluation = int.MinValue + 1;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int evaluation = Minimax(board, depth - 1, alpha, beta);
                board.UndoMove(move);
                evaluatedMoves++;
                maxEvaluation = Math.Max(maxEvaluation, evaluation);
                alpha = Math.Max(alpha, evaluation);
                if (beta <= alpha)
                {
                    break;
                }
            }
            return maxEvaluation + GetRandomValue();
        }
        else
        {
            int minEvaluation = int.MaxValue - 1;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int evaluation = Minimax(board, depth - 1, alpha, beta);
                board.UndoMove(move);
                evaluatedMoves++;
                minEvaluation = Math.Min(minEvaluation, evaluation);
                beta = Math.Min(beta, evaluation);
                if (beta <= alpha)
                {
                    break;
                }
            }
            // Add some randomness to the evaluation to avoid the bot playing the same game over and over again
            return minEvaluation + GetRandomValue();
        }
    }
}