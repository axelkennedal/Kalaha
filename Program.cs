// See https://aka.ms/new-console-template for more information
using System.Text;


namespace Kalaha
{
    public class Simulation
    {
        public static void Main(string[] args)
        {
            var b = new KalahaBoard();
            b.PrintBoard();

            // main game loop
            var currentPlayer = Player.Player2;
            var playable = b.PlayableSlots(currentPlayer);
            int turn = 1;
            while (playable.Count > 0)
            {
                Console.WriteLine("Turn " + turn + " - " + currentPlayer);
                b.PlayTurn(playable.First(), currentPlayer);
                b.PrintBoard();
                turn++;

                currentPlayer = currentPlayer == Player.Player1 ? Player.Player2 : Player.Player1;
                playable = b.PlayableSlots(currentPlayer);
            }

            // collect any remaining marbles
            currentPlayer = currentPlayer == Player.Player1 ? Player.Player2 : Player.Player1;
            playable = b.PlayableSlots(currentPlayer);
            if (playable.Count > 0)
            {
                Console.WriteLine($"Capture remaining - {currentPlayer}");
            }
            foreach (int remainingSlot in playable)
            {
                b.StoreMarbles(remainingSlot, currentPlayer);
            }
            b.PrintBoard();

            Player? winner = null; // null => draw
            var p1Points = b.GetPoints(Player.Player1);
            var p2Points = b.GetPoints(Player.Player2);
            double ratio = 0.5;
            if (p1Points > p2Points)
            {
                winner = Player.Player1;
                ratio = (double)p1Points / b.TotalMarbles;
            }
            else if (p2Points > p1Points)
            {
                winner = Player.Player2;
                ratio = (double)p2Points / b.TotalMarbles;
            }

            Console.WriteLine($"WINNER: {winner}, ratio: {ratio * 100:0.00}%");
        }

        public enum Player
        {
            Player1, // starts bottom left
            Player2 // starts top right
        }

        public class KalahaBoard
        {
            // counter clockwise
            private int[] slots;
            public const int bottomLeft = 0, bottomRight = 5, rightStore = 6, topRight = 7, topLeft = 12, leftStore = 13;

            public KalahaBoard()
            {
                slots = new int[] { 4, 4, 4, 4, 4, 4, 0, 4, 4, 4, 4, 4, 4, 0 };
            }

            public int GetPoints(Player player)
            {
                if (player == Player.Player1) return slots[rightStore];
                else return slots[leftStore];
            }

            public int TotalMarbles => 4 * 6 * 2;

            public void PrintBoard()
            {
                /* string board = "------------------------------------\n";
                board += $"|   | [{slots[12]}] [{slots[11]}] [{slots[10]}] [{slots[9]}] [{slots[8]}] [{slots[7]}] |   |\n";
                board += $"| {slots[13]} |      \t\t      | {slots[6]} |\n";
                board += $"|   | [{slots[0]}] [{slots[1]}] [{slots[2]}] [{slots[3]}] [{slots[4]}] [{slots[5]}] |   |\n";
                board += "------------------------------------\n\n";
                Console.Write(board); */

                int topBonusLength = 0;
                for (int currentPos = topRight; currentPos <= topLeft; currentPos++)
                {
                    if (slots[currentPos] > 9) topBonusLength++;
                }

                int bottomBonusLength = 0;
                for (int currentPos = bottomLeft; currentPos <= bottomRight; currentPos++)
                {
                    if (slots[currentPos] > 9) bottomBonusLength++;
                }

                int bonusLength = Math.Max(topBonusLength, bottomBonusLength);
                string bonusString = "";
                for (int i = 0; i < bonusLength; i++) bonusString += "-";

                Console.Write(bonusString + "--------------------------------------------------\n");
                var lines = new List<string[]>();
                lines.Add(new[] { "|   ", $"[{slots[12]}]", $"[{slots[11]}]", $"[{slots[10]}]", $"[{slots[9]}]", $"[{slots[8]}]", $"[{slots[7]}]", "", "|" });
                lines.Add(new[] { $"|   {slots[13]}", "", "", "", "", "", "", $" {slots[6]}", "|" });
                lines.Add(new[] { "|   ", $"[{slots[0]}]", $"[{slots[1]}]", $"[{slots[2]}]", $"[{slots[3]}]", $"[{slots[4]}]", $"[{slots[5]}]", "", "|" });
                Console.Write(PadElementsInLines(lines, 3) + "\n");
                Console.Write(bonusString + "--------------------------------------------------\n\n");
            }

            // yoinked from https://stackoverflow.com/questions/4449021/how-can-i-align-text-in-columns-using-console-writeline
            public static string PadElementsInLines(List<string[]> lines, int padding = 1)
            {
                // Calculate maximum numbers for each element accross all lines
                var numElements = lines[0].Length;
                var maxValues = new int[numElements];
                for (int i = 0; i < numElements; i++)
                {
                    maxValues[i] = lines.Max(x => x[i].Length) + padding;
                }
                var sb = new StringBuilder();
                // Build the output
                bool isFirst = true;
                foreach (var line in lines)
                {
                    if (!isFirst)
                    {
                        sb.AppendLine();
                    }
                    isFirst = false;
                    for (int i = 0; i < line.Length; i++)
                    {
                        var value = line[i];
                        // Append the value with padding of the maximum length of any value for this element
                        sb.Append(value.PadRight(maxValues[i]));
                    }
                }
                return sb.ToString();
            }

            public List<int> PlayableSlots(Player player)
            {
                int startPos, endPos;
                if (player == Player.Player1)
                {
                    startPos = bottomLeft; endPos = bottomRight;
                }
                else
                {
                    startPos = topRight; endPos = topLeft;
                }

                var playableSlots = new List<int>();

                for (int currentPos = startPos; currentPos <= endPos; currentPos++)
                {
                    if (slots[currentPos] > 0)
                    {
                        playableSlots.Add(currentPos);
                    }
                }
                return playableSlots;
            }

            public void StoreMarbles(int fromSlot, Player toPlayerStore)
            {
                if (slots[fromSlot] == 0) return;

                int toSlot = toPlayerStore == Player.Player1 ? rightStore : leftStore;

                slots[toSlot] += slots[fromSlot];
                slots[fromSlot] = 0;
            }
            public int PlayTurn(int fromSlot, Player player)
            {
                if (slots[fromSlot] == 0) return 0;

                int marblesInHand = slots[fromSlot];
                slots[fromSlot] = 0;

                int pointsGained = 0;

                for (int currentPos = (fromSlot + 1) % slots.Length; marblesInHand > 0; currentPos = (currentPos + 1) % slots.Length)
                {
                    // don't put marbles in the enemy store
                    if ((player == Player.Player1 && currentPos == leftStore) || (player == Player.Player2 && currentPos == rightStore)) continue;

                    // count points gained
                    if ((player == Player.Player1 && currentPos == rightStore) || (player == Player.Player2 && currentPos == leftStore)) pointsGained++;

                    slots[currentPos]++;
                    marblesInHand--;
                }

                return pointsGained;
            }
        }
    }
}