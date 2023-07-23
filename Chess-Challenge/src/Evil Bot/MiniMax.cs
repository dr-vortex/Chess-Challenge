using ChessChallenge.API;
using System;

namespace Frederox.MiniMax
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class MiniMax : IChessBot
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
            if (moves.Length < 10) depthToSearch = 4;
            else depthToSearch = 2;

            for (int i = 0; i < moves.Length; i++)
            {
                int score = Minimax(board, moves[i], depthToSearch);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = moves[i];
                }
            }

            return bestMove;
        }

        public int Minimax(Board board, Move move, int depth)
        {
            positionsEvaluated++;

            board.MakeMove(move);

            // We should always evaluate from the perspective of the bot's color
            // The problem was if you cause the bot to evaluate for the other color, then it will minimize for that colo, which means it will pick the worst move instead of the best move for the other color
            int heuristic = EvaluateBoard(board, botIsWhite);

            if (depth == 0)
            {
                board.UndoMove(move);

                return heuristic;
            }

            Move[] legalResponses = board.GetLegalMoves();
            int bestLegalResponseValue;

            // We also no longer need to pass if we are maximizing, since we can always tell by the color of our bot and the color of the board
            // if it is our turn to move, we maximize score
            if (botIsWhite == board.IsWhiteToMove)
            {
                bestLegalResponseValue = int.MinValue;
                for (int i = 0; i < legalResponses.Length; i++)
                {
                    int value = Minimax(board, legalResponses[i], depth - 1);
                    bestLegalResponseValue = Math.Max(value, bestLegalResponseValue);
                }
            }
            // if it is not our turn to move, we minimize OUR score, which is the same as maximizing the opponent's score
            else
            {
                bestLegalResponseValue = int.MaxValue;
                for (int i = 0; i < legalResponses.Length; i++)
                {
                    int value = Minimax(board, legalResponses[i], depth - 1);
                    bestLegalResponseValue = Math.Min(value, bestLegalResponseValue);
                }
            }

            board.UndoMove(move);

            return bestLegalResponseValue;
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

            return (asWhite ? 1 : -1) * (whiteScore - blackScore);
        }
    }
}