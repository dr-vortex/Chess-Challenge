using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class Ernestoyaquello : IChessBot
{
    // TODO Simplify these further to reduce tokens?
    private static readonly Dictionary<PieceType, int[]> _pieceValues = new Dictionary<PieceType, int[]>
    {
        {
            PieceType.Pawn,
            new int[]
            {
                // Intrinsic value
                100,

                // Position values (white at the bottom, black at the top)
                  0,  0,  0,  0,  0,  0,  0,  0,
                 50, 50, 50, 50, 50, 50, 50, 50,
                 10, 10, 20, 30, 30, 20, 10, 10,
                  5,  5, 10, 25, 25, 10,  5,  5,
                  0,  0,  0, 20, 20,  0,  0,  0,
                  5, -5,-10,  0,  0,-10, -5,  5,
                  5, 10, 10,-20,-20, 10, 10,  5,
                  0,  0,  0,  0,  0,  0,  0,  0,
             }
        },

        {
            PieceType.Knight,
            new int[]
            {
                // Intrinsic value
                320,

                // Position values (white at the bottom, black at the top)
                -50,-40,-30,-30,-30,-30,-40,-50,
                -40,-20,  0,  0,  0,  0,-20,-40,
                -30,  0, 10, 15, 15, 10,  0,-30,
                -30,  5, 15, 20, 20, 15,  5,-30,
                -30,  0, 15, 20, 20, 15,  0,-30,
                -30,  5, 10, 15, 15, 10,  5,-30,
                -40,-20,  0,  5,  5,  0,-20,-40,
                -50,-40,-30,-30,-30,-30,-40,-50,
             }
        },

        {
            PieceType.Bishop,
            new int[]
            {
                // Intrinsic value
                333,

                // Position values (white at the bottom, black at the top)
                -20,-10,-10,-10,-10,-10,-10,-20,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -10,  0,  5, 10, 10,  5,  0,-10,
                -10,  5,  5, 10, 10,  5,  5,-10,
                -10,  0, 10, 10, 10, 10,  0,-10,
                -10, 10, 10, 10, 10, 10, 10,-10,
                -10,  5,  0,  0,  0,  0,  5,-10,
                -20,-10,-10,-10,-10,-10,-10,-20,
             }
        },

        {
            PieceType.Rook,
            new int[]
            {
                // Intrinsic value
                510,

                // Position values (white at the bottom, black at the top)
                  0,  0,  0,  0,  0,  0,  0,  0,
                  5, 10, 10, 10, 10, 10, 10,  5,
                 -5,  0,  0,  0,  0,  0,  0, -5,
                 -5,  0,  0,  0,  0,  0,  0, -5,
                 -5,  0,  0,  0,  0,  0,  0, -5,
                 -5,  0,  0,  0,  0,  0,  0, -5,
                 -5,  0,  0,  0,  0,  0,  0, -5,
                  0,  0,  0,  5,  5,  0,  0,  0,
            }
        },

        {
            PieceType.Queen,
            new int[]
            {
                // Intrinsic value
                880,

                // Position values (white at the bottom, black at the top)
                -20,-10,-10, -5, -5,-10,-10,-20,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -10,  0,  5,  5,  5,  5,  0,-10,
                 -5,  0,  5,  5,  5,  5,  0, -5,
                  0,  0,  5,  5,  5,  5,  0, -5,
                -10,  5,  5,  5,  5,  5,  0,-10,
                -10,  0,  5,  0,  0,  0,  0,-10,
                -20,-10,-10, -5, -5,-10,-10,-20,
             }
        },
    };

    private struct ScoredMove
    {
        public ScoredMove(Move move, int potentialScore = int.MinValue) =>
            (Move, PotentialScore, GameScore) = (move, potentialScore, int.MinValue);

        public Move Move { get; }
        public int PotentialScore { get; } // Based only on the piece position change and the potential capture
        public int GameScore { get; set; } // To be set later by the function CalculateBestMove(...)
    }

    public Move Think(Board board, Timer timer) => CalculateBestMove(board, timer).Move;

    private ScoredMove CalculateBestMove(Board board, Timer timer, int previousBestMoveScore = int.MinValue, int depth = 1)
    {
        // Moves scored by their potential and sorted from the highest to the lowest to cut off branchs early
        var scoredMoves = board
            .GetLegalMoves()
            .Select(m => new ScoredMove(m, CalculateMovePotentialScore(m, board.IsWhiteToMove)))
            .OrderByDescending(m => m.PotentialScore);

        var bestMove = scoredMoves.FirstOrDefault(new ScoredMove(Move.NullMove));

        using var scoredMovesEnumerator = scoredMoves.GetEnumerator();
        while (scoredMovesEnumerator.MoveNext())
        {
            var candidateMove = scoredMovesEnumerator.Current;
            board.MakeMove(candidateMove.Move);

            if (board.IsInCheckmate())
                candidateMove.GameScore = int.MaxValue;
            else
            {
                // Change maximum depth depending on how much time we have and how many pieces there are left
                var numberOfPiecesLeft = BitboardHelper.GetNumberOfSetBits(board.AllPiecesBitboard);
                var maxDepth = (numberOfPiecesLeft <= 12 || timer.MillisecondsRemaining > 25000)
                    ? 5
                    : (timer.MillisecondsRemaining > 5000 ? 4 : 3);

                // Calculate the best move for the opponent after our move, then take the inverse as our score.
                // If we cannot go deeper in the search tree, then we calculate the score using heuristics.
                candidateMove.GameScore = (depth < maxDepth)
                    ? -CalculateBestMove(board, timer, bestMove.GameScore, depth + 1).GameScore
                    : CalculateHeuristicScore(board, numberOfPiecesLeft);
            }

            board.UndoMove(candidateMove.Move);

            bestMove = candidateMove.GameScore > bestMove.GameScore ? candidateMove : bestMove;

            // No need to continue with the search in this branch if one of these is true:
            // * The ideal move (a checkmate) has already been found.
            // * The parent node has already found a move that would make us perform worse in this turn than we are performing now.
            //   Thus, they will choose that move and not the one that would lead to the hypothetical situation we are analysing here.
            if (bestMove.GameScore == int.MaxValue || previousBestMoveScore >= -bestMove.GameScore)
                break;
        }

        return bestMove;
    }

    private int CalculateHeuristicScore(Board board, int numberOfPiecesLeft)
    {
        var heuristicScore = 0;
        var numberOfPiecesFound = 0;

        // Calculate how well we are doing looking at the piece scores, both ours (positive) and the opponent's (negative)
        var squareIndex = 0;
        while (numberOfPiecesFound < numberOfPiecesLeft)
        {
            var square = new Square(squareIndex++);
            var pieceInSquare = board.GetPiece(square);
            if (!pieceInSquare.IsNull)
            {
                // To know if a piece is ours, we just have to remember that we are here because we just moved (it is the opponent's turn)
                heuristicScore += (pieceInSquare.IsWhite != board.IsWhiteToMove ? 1 : -1) * CalculatePieceScore(pieceInSquare.PieceType, square, pieceInSquare.IsWhite);
                numberOfPiecesFound++;
            }
        }

        return heuristicScore;
    }

    // Calculations: the intrinsic value of the piece + its position value
    private int CalculatePieceScore(PieceType piece, Square position, bool isWhite) => (piece == PieceType.None || piece == PieceType.King)
        ? 0
        : _pieceValues[piece][0] + (_pieceValues[piece][1 + ((isWhite ? (7 - position.Rank) : position.Rank) * 8) + (isWhite ? position.File : (7 - position.File))]);

    // Calculations: score of the piece after moving - score of the piece before moving + score of the captured piece
    private int CalculateMovePotentialScore(Move move, bool isWhite) => 
        CalculatePieceScore((move.IsPromotion ? move.PromotionPieceType : move.MovePieceType), move.TargetSquare, isWhite)
        - CalculatePieceScore(move.MovePieceType, move.StartSquare, isWhite)
        + CalculatePieceScore(move.CapturePieceType, move.TargetSquare, !isWhite);
}