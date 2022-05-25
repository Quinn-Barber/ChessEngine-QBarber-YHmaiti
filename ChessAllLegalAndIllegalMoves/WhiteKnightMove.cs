
        // Checks if the white knight move is valid

        public static bool CheckWhiteKnight(ChessBoard board, ChessMove move)
        {
            // killing / emptiness
            if (!KillOrEmpty(move.To.X, move.To.Y, board, ChessColor.Black))
                return false;

            if (Math.Abs(move.To.X - move.From.X) == 2)
            {
                if (Math.Abs(move.To.Y - move.From.Y) != 1)
                    return false;
            }
            else if (Math.Abs(move.To.X - move.From.X) == 1)
            {
                if (Math.Abs(move.To.Y - move.From.Y) != 2)
                    return false;
            }
            else
            {
                return false;
            }


            return true;
        }