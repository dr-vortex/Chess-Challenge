using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    static int[] pieceValues = { 0, 1, 3, 3, 5, 9, 1000 };
    int positionsEvaluated;
    bool botIsWhite;

    public Move Think(Board board, Timer timer)
    {
        botIsWhite = board.IsWhiteToMove;
        positionsEvaluated = 0;

        Move[] moves = board.GetLegalMoves();
        Move bestMove = moves[0];
        int bestScore = int.MinValue;

        int depthToSearch;
        if (moves.Length < 10) depthToSearch = 5;
        else depthToSearch = 3;

        for (int i = 0; i < moves.Length; i++)
        {
            int score = AlphaBeta(board, moves[i], depthToSearch, int.MinValue, int.MaxValue);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = moves[i];
            }
        }

        Console.WriteLine($"Depth: {depthToSearch} evaluated {positionsEvaluated} positions!");
        return bestMove;
    }

    // fail-soft alpha-beta -- https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning
    public int AlphaBeta(Board board, Move move, int depth, int alpha, int beta)
    {
        positionsEvaluated++;
        board.MakeMove(move);

        // Evaluate from the perspective of the bot's color
        int heuristic = EvaluateBoard(board, botIsWhite);

        if (depth == 0)
        {
            board.UndoMove(move);
            return heuristic;
        }

        Move[] legalResponses = board.GetLegalMoves();
        int value;

        // maximize score
        if (botIsWhite == board.IsWhiteToMove)
        {
            value = int.MinValue;
            for (int i = 0; i < legalResponses.Length; i++)
            {
                value = Math.Max(value, AlphaBeta(board, legalResponses[i], depth - 1, alpha, beta));
                alpha = Math.Max(alpha, value);

                if (value >= beta) break;
            }
        }
        else
        {
            value = int.MaxValue;
            for (int i = 0; i < legalResponses.Length; i++)
            {
                value = Math.Min(value, AlphaBeta(board, legalResponses[i], depth - 1, alpha, beta));
                beta = Math.Min(beta, value);

                if (value <= alpha) break;
            }
        }

        board.UndoMove(move);
        return value;
    }

    public int EvaluateBoard(Board board, bool asWhite)
    {
        int whiteScore = 0;
        int blackScore = 0;

        PieceList[] pieces = board.GetAllPieceLists();

        for (int i = 0; i < pieces.Length; i++)
        {
            for (int j = 0; j < pieces[i].Count; j++)
            {
                Piece piece = pieces[i][j];
                int pieceScore = pieceValues[(int)piece.PieceType];

                if (piece.IsWhite) whiteScore += pieceScore;
                else blackScore += pieceScore;
            }
        }

        if (board.IsInCheckmate() || board.IsDraw()) return int.MinValue;

        return (asWhite ? 1 : -1) * (whiteScore - blackScore);
    }
}