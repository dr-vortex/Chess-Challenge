using ChessChallenge.API;
using Frederox.AlphaBeta;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    // 256 * 1024 * 1024 = 268435456 bits
    // 268435456 / 112 (Sizeof TEntry) = 2396745
    TEntry[] transpositions = new TEntry[2396745];

    int positionsEvaluated;

    int negativeInfinity = -100000;
    Move bestMove;
    byte mDepth = 5;

    public Move Think(Board board, Timer timer)
    {
        positionsEvaluated = 0;
        Negamax(board, mDepth, negativeInfinity, -negativeInfinity, board.IsWhiteToMove);
        Console.WriteLine($"Depth: {mDepth}, Evaluated: {positionsEvaluated}, {bestMove}");

        return bestMove;
    }

    int Negamax(Board board, byte depth, int alpha, int beta, bool asWhite)
    {
        int alphaOriginal = alpha;
        TEntry? ttEntry = getTransposition(board.ZobristKey);

        // (* Transposition Table Lookup; board.ZobristKey is the lookup key for ttEntry *)
        if (ttEntry != null && ttEntry.Depth >= depth)
        {
            // if ttEntry.flag = EXACT then
            if (ttEntry.Flag == 2) return ttEntry.Value;

            // else if ttEntry.flag = LOWERBOUND then
            else if (ttEntry.Flag == 0) alpha = Math.Max(alpha, ttEntry.Value);

            // else if ttEntry.flag = UPPERBOUND then
            else beta = Math.Min(beta, ttEntry.Value);

            // if α ≥ β then
            if (alpha >= beta) return ttEntry.Value;
        }

        positionsEvaluated++;

        // if depth = 0 or node is a terminal node then
        if (depth == 0 || board.IsDraw() || board.IsInCheckmate())
        {
            if (board.IsInCheckmate()) return negativeInfinity;
            // return color × the heuristic value of node
            return EvaluateHeuristicValue(board, asWhite);
        }

        var moves = board.GetLegalMoves()
            .OrderByDescending(move => ScoreMovePotential(move));

        int value = negativeInfinity;

        //for (int i = 0; i < moves.Length; i++)
        foreach(Move move in moves)
        {
            board.MakeMove(move);
            int moveScore = Math.Max(value, -Negamax(board, (byte)(depth - 1), -beta, -alpha, !asWhite));
            board.UndoMove(move);

            if (value < moveScore)
            {
                value = moveScore;
                if (depth == mDepth) bestMove = move;
            }

            alpha = Math.Max(alpha, value);
            if (alpha >= beta) break;
        }

        // (* Transposition Table Store; board.ZobristKey is the lookup key for ttEntry *)

        // ttEntry.flag := UPPERBOUND
        byte flag = 2;

        // ttEntry.flag := UPPERBOUND
        if (value <= alphaOriginal) flag = 1;

        // ttEntry.flag := UPPERBOUND
        else if (value >= beta) flag = 0;

        setTransposition(board.ZobristKey, depth, value, flag);
        return value;
    }

    int ScoreMovePotential(Move move)
    {
        int scoreGuess = 0;

        // Prioritise taking high-value pieces with the lowest-value piece
        if (move.IsCapture)
            scoreGuess += 10 * GetPieceValue(move.CapturePieceType) - GetPieceValue(move.MovePieceType);

        // Promoting a pawn
        if (move.IsPromotion) 
            scoreGuess += GetPieceValue(move.PromotionPieceType);

        return scoreGuess;
    }

    int GetPieceValue(PieceType type)
    {
        int[] values = { 0, 100, 300, 300, 500, 900, 10000 };
        return values[(int)type];
    }

    int EvaluateHeuristicValue(Board board, bool asWhite) 
        => EvaluateSide(board, asWhite) - EvaluateSide(board, !asWhite);

    int EvaluateSide(Board board, bool asWhite)
        => board.GetPieceList(PieceType.Pawn, asWhite).Count * 100 +
            board.GetPieceList(PieceType.Knight, asWhite).Count * 300 +
            board.GetPieceList(PieceType.Bishop, asWhite).Count * 300 +
            board.GetPieceList(PieceType.Rook, asWhite).Count * 500 +
            board.GetPieceList(PieceType.Queen, asWhite).Count * 900 +
            board.GetPieceList(PieceType.King, asWhite).Count * 100000 +
            // Mobility (the number of legal moves)
            10 * board.GetLegalMoves().Length;

    TEntry? getTransposition(ulong zobristKey)
    {
        TEntry? entry = transpositions[(int)(zobristKey % 2080895)];
        if (entry != null && entry.ZobristKey == zobristKey) return entry;
        return null;
    }

    void setTransposition(ulong zobristKey, byte depth, int value, byte flag)
    {
        transpositions[(int)(zobristKey % 2396745)] = new TEntry
        {
            ZobristKey = zobristKey,
            Depth = depth,
            Value = value,
            Flag = flag
        };
    }
}

// Size 112 bits
class TEntry
{
    public ulong ZobristKey;
    public int Value;
    public byte Depth;
    /**
     * 0 Lowerbound, 1 Upperbound, 2 Exact
     */
    public byte Flag;
}