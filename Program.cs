// See https://aka.ms/new-console-template for more information

namespace Kalaha
{
    public class Simulation
    {
        public static void Main(string[] args)
        {
            var b = new KalahaBoard();
            b.PrintBoard();
            b.PlayTurn(0, Player.Player1);
            b.PrintBoard();
        }

        public enum Player
        {
            Player1, // starts bottom left
            Player2 // starts top right
        }

        public class KalahaBoard
        {
            // counter clockwise; 0 = bottom left, 6 = right store, 7 = top right, 13 = left store
            private int[] slots;

            public KalahaBoard()
            {
                slots = new int[] { 4, 4, 4, 4, 4, 4, 0, 4, 4, 4, 4, 4, 4, 0 };
            }

            public void PrintBoard()
            {
                string board = "------------------------------------\n";
                board += $"|   | [{slots[12]}] [{slots[11]}] [{slots[10]}] [{slots[9]}] [{slots[8]}] [{slots[7]}] |   |\n";
                board += $"| {slots[13]} |      \t\t      | {slots[6]} |\n";
                board += $"|   | [{slots[0]}] [{slots[1]}] [{slots[2]}] [{slots[3]}] [{slots[4]}] [{slots[5]}] |   |\n";
                board += "------------------------------------\n\n";
                Console.Write(board);
            }

            public int PlayTurn(int fromSlot, Player player)
            {
                int marblesInHand = slots[fromSlot];
                slots[fromSlot] = 0;

                int pointsGained = 0;

                for (int currentPos = (fromSlot + 1) % slots.Length; marblesInHand > 0; currentPos++)
                {
                    // don't put marbles in the enemy store
                    if ((player == Player.Player1 && currentPos == 13) || (player == Player.Player2 && currentPos == 6)) continue;

                    // count points gained
                    if ((player == Player.Player1 && currentPos == 6) || (player == Player.Player2 && currentPos == 13)) pointsGained++;

                    slots[currentPos]++;
                    marblesInHand--;
                }

                return pointsGained;
            }
        }
    }
}