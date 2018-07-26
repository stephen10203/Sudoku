using System;
using Library;

//InFile;
//OutFile;
//IO;

namespace SudokuPrac1Question12
{
    //Sudoku game which draws a possibility board and a play board and will solve
    //the puzzle one search at a time for numbers with only one position they can be placed.
    //When there is more than one move for a number in a block,game will
    //ask for user to make a move
    //To run the game with a solution, open powershell.
    // type in .\SudokuPrac1Question12.exe .\s6 where "s6" can be replaced with any other of the
    //the solution files
    internal class Program
    {
        private enum Pos
        {
            X, Y
        }

        /// <summary>
        /// Main program
        /// </summary>

        private static void Main(string[] args)
        {
            int[,] board = new int[9, 9];
            // Check if a file input is specified
            if (args.Length == 0)
            {
                //some input will be one of the solution files e.g s1
                IO.WriteLine("Nothing was specified for input, rather used \"SudokuPrac1Question12.exe <some input>\"");
                IO.ReadLine();
                return;
            }

            // Load the file
            InFile file = new InFile(args[0]);
            string[] file_lines = new string[21];
            int file_line_count = 0;
            while (!file.EOF())
            {
                file_lines[file_line_count] = file.ReadLine();
                file_line_count++;
            }

            // Interpret starting board: first 9 lines
            for (int i = 0; i < 9; i++)
            {
                var points = file_lines[i].Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < 9; j++)
                {
                    board[j, i] = int.Parse(points[j]);
                }
            }

            // Initiate the board
            CreatePositionPoints();
            CreatePlanningBoard(board);

            bool not_complete = true, endGame = false;
            int pos_x = 0, pos_y = 0, pos_choice = 0, value_count = 1;
            int[,] ai_chosen_spots = new int[9, 9];
            //main game loop
            while (not_complete && !endGame)
            {
                Console.Clear();
                UpdateSuggestedMoves(board, ref planning_board);

                for (int x = 0; x < 9; x++)
                {
                    for (int y = 0; y < 9; y++)
                    {
                        ai_chosen_spots[x, y] = 0;
                    }
                }

                DrawPlanningBoard();
                int knownCount = 0;
                for (int x = 0; x < 9; x++)
                {
                    for (int y = 0; y < 9; y++)
                    {
                        if (board[x, y] != 0)
                        {
                            knownCount++;
                        }
                    }
                }
                AIGenerateChoices(ref ai_chosen_spots, board, StringPositions);

                //Predicate<int> pre = delegate (int a) { return a == 0; };
                int predictionCount = 0;
                for (int x = 0; x < 9; x++)
                {
                    for (int y = 0; y < 9; y++)
                    {
                        if (ai_chosen_spots[x, y] != 0)
                        {
                            predictionCount++;
                        }
                    }
                }

                if (predictionCount != 0) // if we found an ai choice
                {
                    DrawBoard(board, ai_chosen_spots);

                    IO.WriteLine(knownCount + " known " + predictionCount + " predicted\n");

                    // check if at least 1 is suggested
                    IO.WriteLine("I did these, so press <enter> once:");
                    IO.ReadLine();

                    /*if (playerinput=1){
                    //AIFillIn();
                          }
                    */
                }
                else
                {
                    DrawBoard(board);
                    GetPlayerInput(ref pos_x, ref pos_y, ref pos_choice, ref endGame);
                    if (IsValidMove(board, pos_x, pos_y, pos_choice))
                    {
                        PlaceMoveAtPos(ref board, pos_x, pos_y, pos_choice);

                        UpdateSuggestedMoves(board, ref planning_board);
                    }
                    else
                    {
                        IO.WriteLine("Move is not valid, please try a new one");
                    }
                }
                // board complete check
                value_count = 0;
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (board[i, j] == 0)
                        {
                            ++value_count;
                        }
                    }
                }
                if (value_count == 0)
                {
                    IO.WriteLine("Game won!");
                    not_complete = false;
                }
            }

            IO.ReadLine();
        }

        /// <summary>
        /// Logical code below
        /// </summary>

        private static void UpdateSuggestedMoves(int[,] board, ref string[] planning_board)
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    for (int i = 1; i <= 9; i++)
                    {
                        if (!IsValidMove(board, x, y, i))
                        {
                            RemoveNumberFromPosition((PositionNames)(i - 1), StringPositions[x, y]);
                        }
                    }
                }
            }
        }

        private static void PlaceMoveAtPos(ref int[,] board, int pos_x, int pos_y, int pos_choice)
        {
            board[pos_x, pos_y] = pos_choice;
        }

        private static void GetPlayerInput(ref int pos_x, ref int pos_y, ref int pos_choice, ref bool endGame)
        {
            IO.WriteLine("Your move - row[0..8] col[0..8] value[1..9] (zero on its own to give up)?");
            string[] input = IO.ReadLine().Split(' ');
            if ((input[0] == "0") && (input.Length == 1))
            {
                endGame = true;
                IO.WriteLine("Game Over");
            }
            else
            {
                while (input.Length != 3)
                {
                    IO.WriteLine("Please try again with an input for row,column and value in form \"1 4 5\"");
                    input = IO.ReadLine().Split(' ');
                }
                pos_y = int.Parse(input[0]);
                pos_x = int.Parse(input[1]);
                pos_choice = int.Parse(input[2]);
            }
        }

        private static bool IsValidMove(int[,] board, int pos_x, int pos_y, int pos_choice)
        {
            if (board[pos_x, pos_y] != 0)
            {
                return false;
            }

            //horizontal,vertical
            for (int i = 0; i < 9; i++)
            {
                //horizontal y = same
                if (board[i, pos_y] == pos_choice)
                { return false; }
                // vertical x stays same
                if (board[pos_x, i] == pos_choice)
                { return false; }
            }
            //block
            int BlockX = pos_x / 3;
            int BlockY = pos_y / 3;
            for (int x = BlockX * 3; x < BlockX * 3 + 3; x++)
            {
                for (int y = BlockY * 3; y < BlockY * 3 + 3; y++)
                {
                    if (board[x, y] == pos_choice)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Drawing code below
        /// </summary>

        public class PositionPoint
        {
            public int X { get; }
            public int Y { get; }

            /// <summary>
            /// The line and position to start adjusting the drawn possible moves board
            /// </summary>
            /// <param name="y">Top-to-bottom differences (e.g. the line vertically "147"),
            ///                 this is what is used for picking planning_board positions</param>
            /// <param name="x">Left-to-right differences (e.g. the "123")</param>
            public PositionPoint(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int[] One { get { return new int[2] { X, Y }; } }
            public int[] Two { get { return new int[2] { X + 1, Y }; } }
            public int[] Three { get { return new int[2] { X + 2, Y }; } }
            public int[] Four { get { return new int[2] { X, Y + 1 }; } }
            public int[] Five { get { return new int[2] { X + 1, Y + 1 }; } }
            public int[] Six { get { return new int[2] { X + 2, Y + 1 }; } }
            public int[] Seven { get { return new int[2] { X, Y + 2 }; } }
            public int[] Eight { get { return new int[2] { X + 1, Y + 2 }; } }
            public int[] Nine { get { return new int[2] { X + 2, Y + 2 }; } }

            internal int[] IfContains(int num)
            {
                switch (num)
                {
                    case 1: return One;
                    case 2: return Two;
                    case 3: return Three;
                    case 4: return Four;
                    case 5: return Five;
                    case 6: return Six;
                    case 7: return Seven;
                    case 8: return Eight;
                    case 9: return Nine;
                }

                return new int[2] { 0, 0 };
            }
        }

        /// <summary>
        /// Positions uses [horizontal, vertical]
        /// </summary>
        public static PositionPoint[,] StringPositions;

        private static void CreatePositionPoints()
        {
            StringPositions = new PositionPoint[9, 9];
            for (int v = 0; v < 9; v++)
            {
                for (int h = 0; h < 9; h++)
                {
                    StringPositions[h, v] = new PositionPoint(4 * (h + 1), 2 + (v * 4));
                }
            }
        }

        private static string[] planning_board;

        private static void CreatePlanningBoard(int[,] board)
        {
            // Starting board is
            planning_board = new string[38];
            planning_board[0] = "     0 : 1 : 2 | 3 : 4 : 5 | 6 : 7 : 8  ";
            planning_board[1] = "   |===:===:===|===:===:===|===:===:===|";
            planning_board[2] = "   |123:123:123|123:123:123|123:123:123|";
            planning_board[3] = " 0 |456:456:456|456:456:456|456:456:456|";
            planning_board[4] = "   |789:789:789|789:789:789|789:789:789|";
            planning_board[5] = "  -|---:---:---|---:---:---|---:---:---|";
            planning_board[6] = "   |123:123:123|123:123:123|123:123:123|";
            planning_board[7] = " 1 |456:456:456|456:456:456|456:456:456|";
            planning_board[8] = "   |789:789:789|789:789:789|789:789:789|";
            planning_board[9] = "  -|---:---:---|---:---:---|---:---:---|";
            planning_board[10] = "   |123:123:123|123:123:123|123:123:123|";
            planning_board[11] = " 2 |456:456:456|456:456:456|456:456:456|";
            planning_board[12] = "   |789:789:789|789:789:789|789:789:789|";
            planning_board[13] = "  =|===:===:===|===:===:===|===:===:===|";
            planning_board[14] = "   |123:123:123|123:123:123|123:123:123|";
            planning_board[15] = " 3 |456:456:456|456:456:456|456:456:456|";
            planning_board[16] = "   |789:789:789|789:789:789|789:789:789|";
            planning_board[17] = "  -|---:---:---|---:---:---|---:---:---|";
            planning_board[18] = "   |123:123:123|123:123:123|123:123:123|";
            planning_board[19] = " 4 |456:456:456|456:456:456|456:456:456|";
            planning_board[20] = "   |789:789:789|789:789:789|789:789:789|";
            planning_board[21] = "  -|---:---:---|---:---:---|---:---:---|";
            planning_board[22] = "   |123:123:123|123:123:123|123:123:123|";
            planning_board[23] = " 5 |456:456:456|456:456:456|456:456:456|";
            planning_board[24] = "   |789:789:789|789:789:789|789:789:789|";
            planning_board[25] = "  =|===:===:===|===:===:===|===:===:===|";
            planning_board[26] = "   |123:123:123|123:123:123|123:123:123|";
            planning_board[27] = " 6 |456:456:456|456:456:456|456:456:456|";
            planning_board[28] = "   |789:789:789|789:789:789|789:789:789|";
            planning_board[29] = "  -|---:---:---|---:---:---|---:---:---|";
            planning_board[30] = "   |123:123:123|123:123:123|123:123:123|";
            planning_board[31] = " 7 |456:456:456|456:456:456|456:456:456|";
            planning_board[32] = "   |789:789:789|789:789:789|789:789:789|";
            planning_board[33] = "  -|---:---:---|---:---:---|---:---:---|";
            planning_board[34] = "   |123:123:123|123:123:123|123:123:123|";
            planning_board[35] = " 8 |456:456:456|456:456:456|456:456:456|";
            planning_board[36] = "   |789:789:789|789:789:789|789:789:789|";
            planning_board[37] = "   |===:===:===|===:===:===|===:===:===|\n";
        }

        private enum PositionNames
        {
            One, Two, Three,
            Four, Five, Six,
            Seven, Eight, Nine
        }

        private static void RemoveNumberFromPosition(PositionNames which, PositionPoint where)
        {
            int pos_x = 0, pos_y = 0;
            switch (which)
            {
                case PositionNames.One:
                case PositionNames.Four:
                case PositionNames.Seven:
                    pos_x = where.One[(int)Pos.X];
                    break;

                case PositionNames.Two:
                case PositionNames.Five:
                case PositionNames.Eight:
                    pos_x = where.Two[(int)Pos.X];
                    break;

                case PositionNames.Three:
                case PositionNames.Six:
                case PositionNames.Nine:
                    pos_x = where.Three[(int)Pos.X];
                    break;
            }
            switch (which)
            {
                case PositionNames.One:
                case PositionNames.Two:
                case PositionNames.Three:
                    pos_y = where.One[(int)Pos.Y];
                    break;

                case PositionNames.Four:
                case PositionNames.Five:
                case PositionNames.Six:
                    pos_y = where.Four[(int)Pos.Y];
                    break;

                case PositionNames.Seven:
                case PositionNames.Eight:
                case PositionNames.Nine:
                    pos_y = where.Seven[(int)Pos.Y];
                    break;
            }

            string chosen_string = planning_board[pos_y];
            var char_version = chosen_string.ToCharArray();
            char_version[pos_x] = ' ';
            chosen_string = new string(char_version);
            planning_board[pos_y] = chosen_string;
        }

        private static void DrawBoard(int[,] board)
        {
            IO.WriteLine();

            string board_string = string.Format("\tO\t1\t2\t3\t4\t5\t6\t7\t8\n\n" +
                                                "O\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\n" +
                                                "1\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\t{17}\n" +
                                                "2\t{18}\t{19}\t{20}\t{21}\t{22}\t{23}\t{24}\t{25}\t{25}\n" +
                                                "3\t{27}\t{28}\t{29}\t{30}\t{31}\t{32}\t{33}\t{34}\t{35}\n" +
                                                "4\t{36}\t{37}\t{38}\t{39}\t{40}\t{41}\t{42}\t{43}\t{44}\n" +
                                                "5\t{45}\t{46}\t{47}\t{48}\t{49}\t{50}\t{51}\t{52}\t{53}\n" +
                                                "6\t{54}\t{55}\t{56}\t{57}\t{58}\t{59}\t{60}\t{61}\t{62}\n" +
                                                "7\t{63}\t{64}\t{65}\t{66}\t{67}\t{68}\t{69}\t{70}\t{71}\n" +
                                                "8\t{72}\t{73}\t{74}\t{75}\t{76}\t{77}\t{78}\t{79}\t{80}\n\n",
                                                board[0, 0], board[1, 0], board[2, 0], board[3, 0], board[4, 0], board[5, 0], board[6, 0], board[7, 0], board[8, 0],
                                                board[0, 1], board[1, 1], board[2, 1], board[3, 1], board[4, 1], board[5, 1], board[6, 1], board[7, 1], board[8, 1],
                                                board[0, 2], board[1, 2], board[2, 2], board[3, 2], board[4, 2], board[5, 2], board[6, 2], board[7, 2], board[8, 2],
                                                board[0, 3], board[1, 3], board[2, 3], board[3, 3], board[4, 3], board[5, 3], board[6, 3], board[7, 3], board[8, 3],
                                                board[0, 4], board[1, 4], board[2, 4], board[3, 4], board[4, 4], board[5, 4], board[6, 4], board[7, 4], board[8, 4],
                                                board[0, 5], board[1, 5], board[2, 5], board[3, 5], board[4, 5], board[5, 5], board[6, 5], board[7, 5], board[8, 5],
                                                board[0, 6], board[1, 6], board[2, 6], board[3, 6], board[4, 6], board[5, 6], board[6, 6], board[7, 6], board[8, 6],
                                                board[0, 7], board[1, 7], board[2, 7], board[3, 7], board[4, 7], board[5, 7], board[6, 7], board[7, 7], board[8, 7],
                                                board[0, 8], board[1, 8], board[2, 8], board[3, 8], board[4, 8], board[5, 8], board[6, 8], board[7, 8], board[8, 8]);
            board_string = board_string.Replace('0', '.');
            IO.WriteLine(board_string);
            IO.WriteLine();
        }

        private static void DrawPlanningBoard()
        {
            for (int i = 0; i < planning_board.Length; i++) IO.WriteLine(planning_board[i]);
        }

        /// <summary>
        /// AI
        /// </summary>

        private static void DrawBoard(int[,] board, int[,] ai_choices)
        {
            string new_board = "\t O\t 1\t 2\t 3\t 4\t 5\t 6\t 7\t 8\t \n\nO\t";
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (ai_choices[x, y] != 0)
                    {
                        new_board += "{" + ai_choices[x, y].ToString() + "}\t";
                    }
                    else
                    {
                        new_board += " " + board[x, y].ToString() + "\t";
                    }
                }
                if (y + 1 != 9) new_board += "\n" + (y + 1).ToString() + "\t";
            }

            new_board = new_board.Replace('0', '.');
            IO.WriteLine(new_board + "\n");
            IO.WriteLine();
        }

        private static void AIGenerateChoices(ref int[,] ai_chosen, int[,] board, PositionPoint[,] stringPositions)
        {
            for (int we_pick = 1; we_pick <= 9; we_pick++)
            {
                for (int y_9x9 = 0; y_9x9 < 3; y_9x9++)
                {
                    for (int x_9x9 = 0; x_9x9 < 3; x_9x9++)
                    {
                        //Update planning board
                        // Now we check for the colum/row/single spot
                        // logic first: single spot aka. we suggest there
                        int first_step_count = 0;
                        int step_1_x = 0, step_1_y = 0;
                        for (int small_x = 0; (small_x < 3) && (first_step_count < 2); small_x++)
                        {
                            for (int small_y = 0; (small_y < 3) && (first_step_count < 2); small_y++)
                            {
                                if (board[x_9x9 * 3 + small_x, y_9x9 * 3 + small_y] == 0)
                                {
                                    // this next line is horrizontal,vertical
                                    var point = StringPositions[x_9x9 * 3 + small_x, y_9x9 * 3 + small_y];
                                    var char_position = point.IfContains(we_pick);
                                    var cha = planning_board[char_position[(int)Pos.Y]][char_position[(int)Pos.X]];
                                    if (we_pick.ToString() == cha.ToString())
                                    {
                                        ++first_step_count;
                                        step_1_x = x_9x9 * 3 + small_x;
                                        step_1_y = y_9x9 * 3 + small_y;
                                    }
                                }
                            }
                        }
                        // we found a single spot available for that number
                        if (first_step_count == 1)
                        {
                            // find where that single spot is, put it into not complete
                            // update the string possible for that tile to remove it all
                            PlaceMoveAtPos(ref board, step_1_x, step_1_y, we_pick);

                            //DrawBoard(board, ai_chosen);
                            ai_chosen[step_1_x, step_1_y] = we_pick;
                            // for x for the row, for y for the column ->
                            //   - RemoveNumberFromPosition()
                        }
                    }
                }
            }
        }
    }
}