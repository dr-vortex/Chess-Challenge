using Raylib_cs;
using System.Numerics;
using System;
using System.IO;

namespace ChessChallenge.Application
{
    public static class MenuUI
    {
        static bool selectFirst = true;
        static ChallengeController.PlayerType firstType;

        public static void DrawButtons(ChallengeController controller)
        {
            Vector2 buttonPos = UIHelper.Scale(new Vector2(200, 100));
            Vector2 buttonSize = UIHelper.Scale(new Vector2(380, 55));
            float spacing = buttonSize.Y * 1.2f;
            float breakSpacing = spacing * 0.6f;

            Option(ChallengeController.PlayerType.Human);
            Option(ChallengeController.PlayerType.MyBot);

            buttonPos.Y += breakSpacing;

            // Game Buttons
            Option(ChallengeController.PlayerType.EvilBot);
            Option(ChallengeController.PlayerType.MiniMax);
            Option(ChallengeController.PlayerType.AlphaBeta);
            Option(ChallengeController.PlayerType.Negamax, elo: 1556);
            Option(ChallengeController.PlayerType.Quiescence, elo: 1717);

            buttonPos.Y += breakSpacing;

            Option(ChallengeController.PlayerType.Stockfish12, elo: StockfishSettings.getElo());
            Option(ChallengeController.PlayerType.Ernestoyaquello, elo: 1683);
            Option(ChallengeController.PlayerType.Outer);

            void Option(ChallengeController.PlayerType playerType, string name = "", int elo = -1)
            {
                if (name == "") name = playerType.ToString();
                string eloString;
                if (elo == -1) eloString = "";
                else eloString = $"({elo})";

                if (NextButtonInRow(name, ref buttonPos, spacing, buttonSize, eloString))
                {
                    if (selectFirst)
                    {
                        selectFirst = false;
                        firstType = playerType;
                    }
                    else
                    {
                        controller.StartNewBotMatch(firstType, playerType);
                        selectFirst = true;
                    }
                }
            }

            bool NextButtonInRow(string name, ref Vector2 pos, float spacingY, Vector2 size, string subtext = "")
            {
                bool pressed = UIHelper.Button(name, pos, size, subtext);
                pos.Y += spacingY;
                return pressed;
            }
        }
    }
}