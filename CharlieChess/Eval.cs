/*
 *	EVAL.C
 *	Tom Kerrigan's Simple Chess Program (TSCP)
 *
 *	Copyright 1997 Tom Kerrigan
 */

/* with fen and null move capabilities - N.Blais 3/5/05 */

namespace CharlieChess
{
    public unsafe partial class Tscp
    {
        private const long DOUBLED_PAWN_PENALTY = 10;
        private const long ISOLATED_PAWN_PENALTY = 35;
        private const long BACKWARDS_PAWN_PENALTY = 8;
        private const long PASSED_PAWN_BONUS = 30;
        private const long ROOK_SEMI_OPEN_FILE_BONUS = 10;
        private const long ROOK_OPEN_FILE_BONUS = 15;
        private const long ROOK_ON_SEVENTH_BONUS = 20;
        private const long IL = LIGHT * 10;
        private const long ID = DARK * 10;

        /* the values of the pieces */
        private long[] piece_value = new long[6] { 100, 300, 310, 500, 900, 0 };

        // The "pcsq" arrays are ptr/square tables. They're values
        // added to the material value of the ptr based on the
        // location of the ptr. */

        private long[] pawn_pcsq = new long[64]
        {
            0,   0,   0,   0,   0,   0,   0,   0,
            10, 13,  18,  25,  25,  18,  14,   12,
            7,  10,  14,  20,  20,  14,  11,   8,
            5,   8,  10,  14,  14,  10,   8,   5,
            3,   5,   7,   8,   8,   7,   5,   3,
            1,   3,   4,  -5,  -5,   4,   3,   1,
            0,   0,   0, -30, -30,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0
        };

        private long[] knight_pcsq = new long[64]
        {
            -10, -10, -10, -10, -10, -10, -10, -10,
            -10,   0,   0,   0,   0,   0,   0, -10,
            -10,   0,   5,   5,   5,   5,   0, -10,
            -10,   0,   5,  10,  10,   5,   0, -10,
            -10,   0,   5,  10,  10,   5,   0, -10,
            -10,   0,   5,   5,   5,   5,   0, -10,
            -10,   0,   0,   0,   0,   0,   0, -10,
            -10, -20, -10, -10, -10, -10, -20, -10
        };

        private long[] bishop_pcsq = new long[64]
        {
            -5,  -10, -20, -25, -25, -20, -10,  -5,
            -10,   5,   0,   0,   0,   0,   5, -10,
            -20,   0,   8,   5,   5,   8,   0, -20,
            -25,   0,   5,  10,  10,   5,   0, -25,
            -25,   0,   5,  10,  10,   5,   0, -25,
            -20,   0,   8,   5,   5,   8,   0, -20,
            -10,   5,   0,   0,   0,   0,   5, -10,
            -5,  -10, -20, -25, -25, -20, -10,  -5
        };

        private long[] king_pcsq = new long[64]
        {
            -40, -40, -40, -40, -40, -40, -40, -40,
            -40, -40, -40, -40, -40, -40, -40, -40,
            -40, -40, -40, -40, -40, -40, -40, -40,
            -40, -40, -40, -40, -40, -40, -40, -40,
            -40, -40, -40, -40, -40, -40, -40, -40,
            -40, -40, -40, -40, -40, -40, -40, -40,
            -20, -20, -20, -20, -20, -20, -20, -20,
              0,  20,  40, -20,   0, -20,  40,  20
        };

        private long[] king_endgame_pcsq = new long[64]
        {
              0,  10,  20,  30,  30,  20,  10,   0,
             10,  20,  30,  40,  40,  30,  20,  10,
             20,  30,  40,  50,  50,  40,  30,  20,
             30,  40,  50,  60,  60,  50,  40,  30,
             30,  40,  50,  60,  60,  50,  40,  30,
             20,  30,  40,  50,  50,  40,  30,  20,
             10,  20,  30,  40,  40,  30,  20,  10,
              0,  10,  20,  30,  30,  20,  10,   0
        };

        // The flip array is used to calculate the ptr/square
        //   values for DARK pieces. The ptr/square value of a
        //   LIGHT pawn is pawn_pcsq[sq] and the value of a DARK
        //   pawn is pawn_pcsq[flip[sq]] */
        private long[] flip = new long[64]
        {
             56,  57,  58,  59,  60,  61,  62,  63,
             48,  49,  50,  51,  52,  53,  54,  55,
             40,  41,  42,  43,  44,  45,  46,  47,
             32,  33,  34,  35,  36,  37,  38,  39,
             24,  25,  26,  27,  28,  29,  30,  31,
             16,  17,  18,  19,  20,  21,  22,  23,
              8,   9,  10,  11,  12,  13,  14,  15,
              0,   1,   2,   3,   4,   5,   6,   7
        };

        // pr[x][y] is the rank of the least advanced pawn of col x on file
        //   y - 1. There are "buffer files" on the left and right to avoid special-case
        //   logic later. If there's no pawn on a rank, we pretend the pawn is
        //   impossibly far advanced (0 for LIGHT and 7 for DARK). This makes it easy to
        //   test for pawns on a rank and it simplifies some pawn evaluation code. */
        private long[] pawn_rank = new long[2 * 10];

        private long[] piece_mat = new long[2];  /* the value of a side's pieces */
        private long[] pawn_mat = new long[2];  /* the value of a side's pawns */
        private long[] score = new long[2];  /* each side's scr */

        private long Eval()
        {
            long f;  /* file */

            /* this is the first pass: set up pr, pm, and pam. */
            for (int i = 0; i < 10; ++i)
            {
                pr[IL + i] = 0;
                pr[ID + i] = 7;
            }

            pm[LIGHT] = 0;
            pm[DARK] = 0;
            pam[LIGHT] = 0;
            pam[DARK] = 0;

            for (int i = 0; i < 64; ++i)
            {
                if (col[i] == EMPTY)
                    continue;
                if (ptr[i] == PAWN)
                {
                    pam[col[i]] += piece_value[PAWN];
                    f = (i & 7) + 1;  /* add 1 because of the extra file in the array */
                    if (col[i] == LIGHT)
                    {
                        if (pr[IL + f] < (i >> 3))
                            pr[IL + f] = (i >> 3);
                    }
                    else
                    {
                        if (pr[ID + f] > (i >> 3))
                            pr[ID + f] = (i >> 3);
                    }
                }
                else
                    pm[col[i]] += piece_value[ptr[i]];
            }

            /* this is the second pass: evaluate each ptr */
            scr[LIGHT] = pm[LIGHT] + pam[LIGHT];
            scr[DARK] = pm[DARK] + pam[DARK];
            for (int i = 0; i < 64; ++i)
            {
                if (col[i] == EMPTY)
                    continue;
                if (col[i] == LIGHT)
                {
                    switch (ptr[i])
                    {
                        case PAWN:
                            scr[LIGHT] += EvalLightPawn(i, i >> 3);
                            break;
                        case KNIGHT:
                            scr[LIGHT] += knight_pcsq[i];
                            break;
                        case BISHOP:
                            scr[LIGHT] += bishop_pcsq[i];
                            break;
                        case ROOK:
                            if (pr[IL + ((i & 7) + 1)] == 0)
                            {
                                if (pr[ID + ((i & 7) + 1)] == 7)
                                    scr[LIGHT] += ROOK_OPEN_FILE_BONUS;
                                else
                                    scr[LIGHT] += ROOK_SEMI_OPEN_FILE_BONUS;
                            }

                            if ((i >> 3) == 1)
                                scr[LIGHT] += ROOK_ON_SEVENTH_BONUS;
                            break;

                        case KING:
                            if (pm[DARK] <= 1200)
                                scr[LIGHT] += king_endgame_pcsq[i];
                            else
                                scr[LIGHT] += EvalLightKing(i);
                            break;
                    }
                }
                else
                {
                    switch (ptr[i])
                    {
                        case PAWN:
                            scr[DARK] += EvalDarkPawn(i, i >> 3);
                            break;
                        case KNIGHT:
                            scr[DARK] += knight_pcsq[flip[i]];
                            break;
                        case BISHOP:
                            scr[DARK] += bishop_pcsq[flip[i]];
                            break;
                        case ROOK:
                            if (pr[ID + ((i & 7) + 1)] == 7)
                            {
                                if (pr[IL + ((i & 7) + 1)] == 0)
                                    scr[DARK] += ROOK_OPEN_FILE_BONUS;
                                else
                                    scr[DARK] += ROOK_SEMI_OPEN_FILE_BONUS;
                            }

                            if ((i >> 3) == 6)
                                scr[DARK] += ROOK_ON_SEVENTH_BONUS;
                            break;
                        case KING:
                            if (pm[LIGHT] <= 1200)
                                scr[DARK] += king_endgame_pcsq[flip[i]];
                            else
                                scr[DARK] += EvalDarkKing(i);
                            break;
                    }
                }
            }

            //the scr[] array is set, now return the scr relative
            // to the side to move */
            if (side == LIGHT)
                return scr[LIGHT] - scr[DARK];

            return scr[DARK] - scr[LIGHT];
        }

        private long EvalLightPawn(long sq, long row)
        {
            long r = 0;  /* the value to return */
            long f = (sq & 7) + 1;  /* the pawn's file */

            r += pawn_pcsq[sq];

            /* if there's a pawn behind this one, it's doubled */
            if (pr[IL + f] > row)
                r -= DOUBLED_PAWN_PENALTY;

            /* if there aren't any friendly pawns on either side of
               this one, it's isolated */
            if ((pr[IL + (f - 1)] == 0) &&
                    (pr[IL + (f + 1)] == 0))
                r -= ISOLATED_PAWN_PENALTY;

            /* if it's not isolated, it might be backwards */
            else if ((pr[IL + (f - 1)] < row) && 
                    (pr[IL + (f + 1)] < row))
                r -= BACKWARDS_PAWN_PENALTY;

            /* add a bonus if the pawn is passed */
            if ((pr[ID + (f - 1)] >= row) &&
                (pr[ID + f] >= row) &&
                (pr[ID + (f + 1)] >= row))
                r += (7 - row) * PASSED_PAWN_BONUS;

            return r;
        }

        private long EvalDarkPawn(long sq, long row)
        {
            long r = 0;  /* the value to return */
            long f = (sq & 7) + 1;  /* the pawn's file */

            r += pawn_pcsq[flip[sq]];

            /* if there's a pawn behind this one, it's doubled */
            if (pr[ID + f] < row)
                r -= DOUBLED_PAWN_PENALTY;

            /* if there aren't any friendly pawns on either side of
               this one, it's isolated */
            if ((pr[ID + (f - 1)] == 7) &&
                    (pr[ID + (f + 1)] == 7))
                r -= ISOLATED_PAWN_PENALTY;

            /* if it's not isolated, it might be backwards */
            else if ((pr[ID + (f - 1)] > row) &&
                    (pr[ID + (f + 1)] > row))
                r -= BACKWARDS_PAWN_PENALTY;

            /* add a bonus if the pawn is passed */
            if ((pr[IL + (f - 1)] <= row) &&
                (pr[IL + f] <= row) &&
                (pr[IL + (f + 1)] <= row))
                r += row * PASSED_PAWN_BONUS;

            return r;
        }

        private long EvalLightKing(long sq)
        {
            long r = king_pcsq[sq];  /* the value to return */
            long i;

            // if the king is castled, use a special function to evaluate the
            //   pawns on the appropriate side */
            if ((sq & 7) < 3)
            {
                r += EvalLKP(1);
                r += EvalLKP(2);
                r += EvalLKP(3) / 2;  // problems with pawns on the c & f files
                                      //are not as severe */
            }
            else if ((sq & 7) > 4)
            {
                r += EvalLKP(8);
                r += EvalLKP(7);
                r += EvalLKP(6) / 2;
            }

            // otherwise, just assess a penalty if there are open files near
            // the king */
            else
            {
                for (i = sq & 7; i <= (sq & 7) + 2; ++i)
                    if ((pr[IL + i] == 0) && (pr[ID + i] == 7))
                        r -= 10;
            }

            // scale the king safety value according to the opponent's material;
            //   the premise is that your king safety can only be bad if the
            //   opponent has enough pieces to attack you */
            r *= pm[DARK];
            r /= 3100;

            return r;
        }

        private long EvalLKP(long f)
        {
            long r = 0;

            if (pr[IL + f] == 6) ;  /* pawn hasn't moved */

            else if (pr[IL + f] == 5)
                r -= 10;  /* pawn moved one square */
            else if (pr[IL + f] != 0)
                r -= 20;  /* pawn moved more than one square */
            else
                r -= 25;  /* no pawn on this file */

            if (pr[ID + f] == 7)
                r -= 15;  /* no enemy pawn */
            else if (pr[ID + f] == 5)
                r -= 10;  /* enemy pawn on the 3rd rank */
            else if (pr[ID + f] == 4)
                r -= 5;   /* enemy pawn on the 4th rank */

            return r;
        }

        private long EvalDarkKing(long sq)
        {
            long r = king_pcsq[flip[sq]];
            long i;

            if ((sq & 7) < 3)
            {
                r += EvalDKP(1);
                r += EvalDKP(2);
                r += EvalDKP(3) / 2;
            }
            else if ((sq & 7) > 4)
            {
                r += EvalDKP(8);
                r += EvalDKP(7);
                r += EvalDKP(6) / 2;
            }
            else
            {
                for (i = sq & 7; i <= (sq & 7) + 2; ++i)
                    if ((pr[IL + i] == 0) && (pr[ID + i] == 7))
                        r -= 10;
            }

            r *= pm[LIGHT];
            r /= 3100;
            return r;
        }

        private long EvalDKP(long f)
        {
            long r = 0;

            if (pr[ID + f] == 1) ;
            else if (pr[ID + f] == 2)
                r -= 10;
            else if (pr[ID + f] != 7)
                r -= 20;
            else
                r -= 25;

            if (pr[IL + f] == 0)
                r -= 15;
            else if (pr[IL + f] == 2)
                r -= 10;
            else if (pr[IL + f] == 3)
                r -= 5;

            return r;
        }
    }
}
