using Raylib_cs;
using System.Numerics;
using System;
using System.IO;

namespace ChessChallenge.Application
{
    public static class MenuUI
    {
        public static void DrawButtons(ChallengeController controller)
        {
            Vector2 buttonPos = UIHelper.Scale(new Vector2(200, 100));
            Vector2 buttonSize = UIHelper.Scale(new Vector2(380, 55));
            float spacing = buttonSize.Y * 1.2f;
            float breakSpacing = spacing * 0.6f;

            // Game Buttons
            if (NextButtonInRow("Human vs MyBot", ref buttonPos, spacing, buttonSize))
            {
                var whiteType = controller.HumanWasWhiteLastGame ? ChallengeController.PlayerType.Frederox : ChallengeController.PlayerType.Human;
                var blackType = !controller.HumanWasWhiteLastGame ? ChallengeController.PlayerType.Frederox : ChallengeController.PlayerType.Human;
                controller.StartNewGame(whiteType, blackType);
            }

            if (NextButtonInRow("MyBot vs EvilBot", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.Frederox, ChallengeController.PlayerType.EvilBot);
            }

            if (NextButtonInRow("MyBot vs MiniMax", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.Frederox, ChallengeController.PlayerType.MiniMax);
            }

            if (NextButtonInRow("MyBot vs AlphaBeta", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.Frederox, ChallengeController.PlayerType.AlphaBeta);
            }

            if (NextButtonInRow("MyBot vs Negamax", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.Frederox, ChallengeController.PlayerType.Negamax);
            }

            buttonPos.Y += breakSpacing;

            if (NextButtonInRow("MyBot vs Stockfish 12", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.Frederox, ChallengeController.PlayerType.Stockfish12);
            }

            if (NextButtonInRow("MyBot vs @Flow", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.Frederox, ChallengeController.PlayerType.Flow);
            }

            if (NextButtonInRow("MyBot vs @Lithium", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.Frederox, ChallengeController.PlayerType.Lithium);
            }

            if (NextButtonInRow("MyBot vs @Diamoundz", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.Frederox, ChallengeController.PlayerType.Diamoundz);
            }

            if (NextButtonInRow("MyBot vs @Moonwalker", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.Frederox, ChallengeController.PlayerType.Moonwalker);
            }

            buttonPos.Y += breakSpacing;

            if (NextButtonInRow("MyBot vs @Ernestoyaquello", ref buttonPos, spacing, buttonSize))
            {
                controller.StartNewBotMatch(ChallengeController.PlayerType.Frederox, ChallengeController.PlayerType.Ernestoyaquello);
            }

            // Page buttons
            //buttonPos.Y += breakSpacing;

            //if (NextButtonInRow("Save Games", ref buttonPos, spacing, buttonSize))
            //{
            //    string pgns = controller.AllPGNs;
            //    string directoryPath = Path.Combine(FileHelper.AppDataPath, "Games");
            //    Directory.CreateDirectory(directoryPath);
            //    string fileName = FileHelper.GetUniqueFileName(directoryPath, "games", ".txt");
            //    string fullPath = Path.Combine(directoryPath, fileName);
            //    File.WriteAllText(fullPath, pgns);
            //    System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", fullPath));
            //    ConsoleHelper.Log("Saved games to " + fullPath, false, ConsoleColor.Blue);
            //}
            //if (NextButtonInRow("Rules & Help", ref buttonPos, spacing, buttonSize))
            //{
            //    FileHelper.OpenUrl("https://github.com/SebLague/Chess-Challenge");
            //}
            //if (NextButtonInRow("Documentation", ref buttonPos, spacing, buttonSize))
            //{
            //    FileHelper.OpenUrl("https://seblague.github.io/chess-coding-challenge/documentation/");
            //}
            //if (NextButtonInRow("Submission Page", ref buttonPos, spacing, buttonSize))
            //{
            //    FileHelper.OpenUrl("https://forms.gle/6jjj8jxNQ5Ln53ie6");
            //}

            // Window and quit buttons
            buttonPos.Y += breakSpacing;

            bool isBigWindow = Raylib.GetScreenWidth() > Settings.ScreenSizeSmall.X;
            string windowButtonName = isBigWindow ? "Smaller Window" : "Bigger Window";
            //if (NextButtonInRow(windowButtonName, ref buttonPos, spacing, buttonSize))
            //{
            //    Program.SetWindowSize(isBigWindow ? Settings.ScreenSizeSmall : Settings.ScreenSizeBig);
            //}
            //if (NextButtonInRow("Exit (ESC)", ref buttonPos, spacing, buttonSize))
            //{
            //    Environment.Exit(0);
            //}

            bool NextButtonInRow(string name, ref Vector2 pos, float spacingY, Vector2 size)
            {
                bool pressed = UIHelper.Button(name, pos, size);
                pos.Y += spacingY;
                return pressed;
            }
        }
    }
}