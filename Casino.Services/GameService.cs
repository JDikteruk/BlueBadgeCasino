﻿using Casino.Data;
using Casino.Models;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Casino.Services
{
    public class GameService
    {

        public GameService(Guid userId)
        {
            _userId = userId;
        }

        public GameService()
        {

        }

        public bool CreateGame(GameCreate model)
        {
            var entity =
                new Game()
                {
                    GameName = model.GameName,
                    TypeOfGame = model.TypeOfGame,
                    MinBet = model.MinBet,
                    MaxBet = model.MaxBet
                };

            using (var ctx = new ApplicationDbContext())
            {
                ctx.Games.Add(entity);
                return ctx.SaveChanges() == 1;
            }
        }
        public bool UpdateGame(GameUpdate model)
        {
            var entity =
                new Game()
                {
                    GameId = model.GameId,
                    MinBet = model.MinBet,
                    MaxBet = model.MaxBet
                };

            using (var ctx = new ApplicationDbContext())
            {
                ctx.Games.Add(entity);
                return ctx.SaveChanges() == 1;
            }
        }

        public IEnumerable<GameListItem> GetGames()
        {
            using (var ctx = new ApplicationDbContext())
            {
                var query =
                    ctx
                        .Games
                        .Where(e => e.GameId > -1)
                        .Select(
                            e =>
                                new GameListItem
                                {
                                    GameId = e.GameId,
                                    GameName = e.GameName,
                                    TypeOfGame = e.TypeOfGame,
                                    MinBet = e.MinBet,
                                    MaxBet = e.MaxBet
                                }
                        );

                return query.ToArray();
            }
        }

        public IEnumerable<GameListItem> GetGamesPlayer(Guid id) //Limit for players with access to HS games
        {
            bool highStakes = HighStakes(id);

            using (var ctx = new ApplicationDbContext())
            {
                var query =
                    ctx
                        .Games
                        .Where(e => e.IsHighStakes == highStakes)
                        .Select(
                            e =>
                                new GameListItem
                                {
                                    GameId = e.GameId,
                                    GameName = e.GameName,
                                    TypeOfGame = e.TypeOfGame,
                                    MinBet = e.MinBet,
                                    MaxBet = e.MaxBet
                                }
                        );

                return query.ToArray();
            }
        }

        private bool HighStakes(Guid id)
        {
            bool stakes = false;

            using (var ctx = new ApplicationDbContext())
            {
                var query =
                    ctx
                        .Players
                        .Single(e => e.PlayerId == id);


                stakes = query.HasAccessToHighLevelGame;

            }

            return stakes;

        }

        private double PlayerBalance(Guid id)
        {
            double balance = 0;

            using (var ctx = new ApplicationDbContext())
            {
                var query =
                    ctx
                        .Players
                        .Where(e => e.PlayerId == id)
                        .Select(
                            e =>
                                new Player
                                {
                                    CurrentBankBalance = e.CurrentBankBalance
                                }
                        ); ;
                balance = double.Parse(query.ToString());
            }

            return balance;

        }

        public GameListItem GetGameById(int id)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var entity =
                    ctx
                        .Games
                        .Single(e => e.GameId == id);
                return
                    new GameListItem
                    {
                        GameId = entity.GameId,
                        GameName = entity.GameName,
                        TypeOfGame = entity.TypeOfGame,
                        MinBet = entity.MinBet,
                        MaxBet = entity.MaxBet
                    };
            }
        }

        public enum BetType
        {
            basket,
            black,
            column,
            corner,
            double_street,
            dozen,
            even,
            high,
            low,
            no_pass,
            odd,
            pass,
            red,
            snake,
            split,
            straight,
            street,
            trio
        }



        //GamePlay
        public double PlayGame(int id, double betAmt, bool highRoller, BetType bType = BetType.pass)//, betValue = List<int>(0)
        {
            double amount = 0;
            var game = new GameService();

            string gameName = game.GetGameById(id).GameName;
            double payout = 0;

            //game bet limits
            var min = game.GetGameById(id).MinBet;
            var max = game.GetGameById(id).MaxBet;

            if (betAmt<min||betAmt>max) { return 0; }

            else
            {
                var stakes = highRoller;

                switch (gameName.ToLower())
                {
                    case "baccarat":
                        payout = game.Baccarat();
                        break;
                    case "blackjack":
                        payout = game.Blackjack();
                        break;
                    case "craps":
                        //bool Pass or Don't Pass bet type
                        //Not quite there yet
                        bool pass;
                        if (bType == BetType.pass)
                        {
                            pass = true;
                        }
                        else { pass = false; }
                        payout = game.Craps(pass);
                        break;
                    case "roulette":

                        // for Red/Black use 0/1
                        List<int> betValue = new List<int>();

                        payout = game.Roulette(bType, betValue);
                        break;
                    case "keno":
                        //List<int> from "player selection" range = 1-80; up to 20 #'s selected - let's use 10 #'s
                        List<int> playerNums = new List<int>();




                        payout = game.Keno(playerNums);
                        break;
                    case "russian roulette":
                        //need to get userId....

                        double amt = PlayerBalance(_userId);
                        payout = game.RussianRoulette();
                        //Get player bank balance
                        break;
                    default:
                        //switch on gameType

                        var type = GetGameById(id).TypeOfGame;

                        switch (type.ToString().ToLower())
                        {
                            case "dice":
                                payout = game.baseDice();
                                break;
                            case "wheel":
                                payout = game.baseWheel();
                                break;
                            case "random_num":
                                int numPick = r.Next(1, 10); //player selects num
                                payout = game.baseRandNum(numPick);
                                break;
                            default:
                                payout = baseGame();
                                break;
                        }


                        payout = game.baseGame();
                        break;
                }

                if (payout > 0) { amount = payout * betAmt; }
                else if (payout < 0) { }//accountDelete
                else { amount = -1 * betAmt; }

                return amount;
            }
        }

        Random r = new Random();
        private int sum = 0;
        public double payout = 0;
        private readonly Guid _userId;
        private int houseSum = 0;
        private int playerSum = 0;
        public int baseGame()
        {
            int payoutMult = 0;
            // Game Logic
            bool winner;
            Random r = new Random();
            // Odds Range 0 - 99
            // House Odds 0 - 49
            // Player Odds 50 - 99
            int i = r.Next(0, 99);
            if (i < 50)
            {
                winner = true;
            }
            else
            {
                winner = false;
            }

            if (winner)
            {
                payoutMult = 1;
            }
            else { payoutMult = 0; }

            return payoutMult;
        }
        public double baseWheel()
        {
            //52 segments - 1-6 prize level 1-2-4-8-12-24 - payout = 1,2,5,10,20,40
            List<int> wheel = new List<int>();
            List<int> round = new List<int>();
            List<int> prize = new List<int>();

            round.Add(1);
            round.Add(2);
            round.Add(4);
            round.Add(8);
            round.Add(12);
            round.Add(24);

            prize.Add(40);
            prize.Add(20);
            prize.Add(10);
            prize.Add(5);
            prize.Add(2);
            prize.Add(1);

            for (int n = 1; n < 7; n++)
            {
                for (int i = 1; i < round[n] + 1; i++)
                {
                    wheel.Add(prize[i]);
                }
                wheel.Add(0);
            }

            payout = wheel[r.Next(1, 52)];
            return payout;
        }
        public double baseDice()
        {
            List<int> roll = Roll(2);
            int sum = roll.Sum();

            if (sum > 8) { payout = 2; }
            else { payout = 0; }
            return payout;
        }
        public double baseRandNum(int choice)
        {
            int win = r.Next(1, 10);

            if (win == choice) { payout = 1; }
            else { payout = 0; }
            return payout;
        }

        //Game Logic
        //Returns Payout multiplier

        private List<int> Deal(int cardsPerHand)
        {
            List<int> deal = new List<int>();
            int i = 0;
            for (i = 1; i < cardsPerHand + 1; i++)
            {
                r = new Random();
                int v = r.Next(1, 13);
                deal.Add(v);
            }

            return deal;
        }

        private List<int> Hit(List<int> hand)
        {
            Random r = new Random();
            int v = r.Next(1, 13);
            hand.Add(v);

            return hand;
        }

        private List<int> Roll(int numberOfDice)
        {
            int i;
            int dice = numberOfDice;
            List<int> roll = new List<int>();
            for (i = 1; i < dice + 1; i++)
            {
                int diceRoll = r.Next(1, 6);
                roll.Add(diceRoll);
            }
            return roll;
        }

        private int Spin(int chances)
        {
            int spin = r.Next(1, chances);

            return spin;
        }

        private string Winner(int house, int player)
        {
            if (house < player) { return "Player"; }
            else if (player == house) { return "Tie"; }
            else { return "House"; }
        }

        private int EvaluateBaccarat(List<int> hand)
        {
            sum = 0;
            for (int c = 0; c < hand.Count; c++)
            {
                int card = hand[c];
                hand.Remove(card);
                if (card > 9) { card = 0; }
                hand.Add(card);
            }

            sum = hand.Sum();

            return sum;
        }

        public double Baccarat()

        {
            List<int> houseHand = Deal(2);
            List<int> playerHand = Deal(2);
            string win = "";
            //private method to eval hand(s)

            //Value houseHand
            houseSum = EvaluateBaccarat(houseHand) % 10;

            //Value playerHand
            playerSum = EvaluateBaccarat(playerHand) % 10;

            //Game Logic

            //Drawing rule
            if (playerSum >= 8 || houseSum >= 8)
            {

            }

            //Player rule
            else if (playerSum <= 5)
            {
                playerHand = Hit(playerHand);
                playerSum = EvaluateBaccarat(playerHand) % 10;
            }

            //Banker rule
            else if (playerHand.Count() == 2)
            {
                houseHand = Hit(houseHand);
                houseSum = EvaluateBaccarat(houseHand) % 10;
            }

            else if (playerHand.Count() > 2)
            {
                int rule = playerHand[3];

                if (houseSum <= 2)
                {
                    houseHand = Hit(houseHand);

                }
                else if (houseSum == 3 && rule != 8)
                {
                    houseHand = Hit(houseHand);
                }
                else if (houseSum == 4 && Enumerable.Range(2, 7).Contains(rule))
                {
                    houseHand = Hit(houseHand);
                }
                else if (houseSum == 5 && Enumerable.Range(4, 7).Contains(rule))
                {
                    houseHand = Hit(houseHand);
                }
                else if (houseSum == 6 && Enumerable.Range(6, 7).Contains(rule))
                {
                    houseHand = Hit(houseHand);
                }

                houseSum = EvaluateBaccarat(houseHand) % 10;
            }

            //Winner Winner Chicken Dinner
            win = Winner(houseSum, playerSum);

            //Odds
            if (win == "Player") { payout = 1; }
            else if (win == "Tie") { payout = 8; }
            else { payout = 0; }

            return payout;
        }

        private int EvaluateBlackjack(List<int> hand)
        {
            sum = 0;
            for (int c = 0; c < hand.Count; c++)
            {
                int card = hand[c];
                hand.Remove(card);
                if (card > 10) { card = 10; }
                hand.Add(card);
            }

            sum = hand.Sum();

            return sum;
        }

        private int EvaluateAces(List<int> hand)
        {
            sum = 0;


            sum = hand.Sum();
            if (sum < 10 && hand.Contains(1))
            {
                hand.Remove(1);
                hand.Add(11);
                sum = hand.Sum();
            }

            return sum;
        }

        public double Blackjack()
        {
            List<int> houseHand = Deal(2);
            List<int> playerHand = Deal(2);

            playerSum = EvaluateBlackjack(playerHand);
            houseSum = EvaluateBlackjack(houseHand);

            if (houseHand.Contains(1) && houseSum <= 10)
            {
                houseSum = EvaluateAces(houseHand);
            }

            //player = dealer == push (draw) no winner
            if (playerSum == houseSum) { payout = 0; }
            //player > 21 = bust
            else if (playerSum > 21) { payout = 0; }
            //dealer bust = player win
            else if (playerSum < 21 && houseSum > 21) { payout = 1; }
            //player > house = player win
            else if (playerSum <= 21 && playerSum > houseSum) { payout = 1; }
            //player gets 10 + ace = win
            else if (playerHand.Contains(1) && playerHand.Contains(10)) { payout = 1.5; }

            return payout;
        }

        public double Craps(bool Pass) //Pass or Don't Pass bet
        {
            bool pass = Pass;
            int point = 0;
            sum = Roll(2).Sum();
            int round = 1;
            //!st Roll
            if (Pass)
            {
                if (round == 1)
                {
                    switch (sum)
                    {
                        case 2:
                            payout = 0;
                            break;
                        case 3:
                            payout = 0;
                            break;
                        case 7:
                            payout = 1;
                            break;
                        case 11:
                            payout = 1;
                            break;
                        case 12:
                            payout = 0;
                            break;
                        default:
                            point = sum;
                            break;
                    }
                    round += 1;
                }
            }

            while (sum != point || sum != 7)
            {
                sum = Roll(2).Sum();

                if (sum == point) { payout = 1; break; }
                else if (sum == 7) { payout = 0; break; }
            }

            return payout;
        }

        private Dictionary<int, string> RouletteWheel()
        {
            Dictionary<int, string> wheel = new Dictionary<int, string>();
            wheel.Add(0, "Green");
            wheel.Add(28, "Black");
            wheel.Add(9, "Red");
            wheel.Add(26, "Black");
            wheel.Add(30, "Black");
            wheel.Add(11, "Black");
            wheel.Add(7, "Red");
            wheel.Add(20, "Black");
            wheel.Add(32, "Black");
            wheel.Add(17, "Black");
            wheel.Add(5, "Red");
            wheel.Add(22, "Black");
            wheel.Add(34, "Black");
            wheel.Add(15, "Black");
            wheel.Add(3, "Red");
            wheel.Add(24, "Black");
            wheel.Add(36, "Black");
            wheel.Add(13, "Black");
            wheel.Add(1, "Red");
            wheel.Add(37, "Green");
            wheel.Add(27, "Red");
            wheel.Add(10, "Black");
            wheel.Add(25, "Red");
            wheel.Add(29, "Black");
            wheel.Add(12, "Black");
            wheel.Add(8, "Black");
            wheel.Add(19, "Red");
            wheel.Add(31, "Black");
            wheel.Add(18, "Black");
            wheel.Add(6, "Black");
            wheel.Add(21, "Red");
            wheel.Add(33, "Black");
            wheel.Add(16, "Black");
            wheel.Add(4, "Black");
            wheel.Add(23, "Red");
            wheel.Add(35, "Black");
            wheel.Add(14, "Black");
            wheel.Add(2, "Black");

            //00 = 37 for sanity

            return wheel;
        }

        public double RussianRoulette() //Secret High Stakes where loss = player account deletion
        {
            var russian = r.Next(1, 7);

            var load = r.Next(1, 7);

            if (russian == load) { payout = -1; } else { payout = 0; }

            return payout;
        }

        public double Roulette(BetType betType, List<int> betValue) //betValue = player's choice (ie Red, 7, 3rd Street, etc...)
        {
            List<int> targetRange = new List<int>();
            Dictionary<int, string> rouletteWheel = RouletteWheel();

            int winNum = Spin(rouletteWheel.Count());
            string winColor = rouletteWheel[winNum];

            switch (betType.ToString())
            {
                case "straight":
                    if (betValue[0] == winNum) { payout = 35; } else { payout = 0; }
                    break;
                case "split":
                    if (targetRange == betValue) { payout = 17; } else { payout = 0; }
                    break;
                case "street":
                    if (targetRange == betValue) { payout = 11; } else { payout = 0; }
                    break;
                case "corner":
                    if (targetRange == betValue) { payout = 8; } else { payout = 0; }
                    break;
                case "double street":
                    if (targetRange == betValue) { payout = 5; } else { payout = 0; }
                    break;
                case "trio":
                    if (betValue[0] == 0)
                    {
                        targetRange.Add(0);
                        targetRange.Add(1);
                        targetRange.Add(2);
                    }
                    else if (betValue[0] == 1)
                    {
                        targetRange.Add(0);
                        targetRange.Add(2);
                        targetRange.Add(3);
                    }
                    if (targetRange == betValue) { payout = 8; } else { payout = 0; }
                    break;
                case "basket":
                    targetRange.Add(0);
                    targetRange.Add(1);
                    targetRange.Add(2);
                    targetRange.Add(3);

                    if (targetRange == betValue) { payout = 6; } else { payout = 0; }
                    break;
                case "high low":
                    if (betType.ToString() == "high" && betValue[0] == 0) { payout = 1; }
                    else if (betType.ToString() == "low" && betValue[0] == 1) { payout = 1; }
                    else { payout = 0; }
                    break;
                case "color":
                    if (betType.ToString() == "red" && winColor.ToLower() == "red") { payout = 1; }
                    else if (betType.ToString() == "black" && winColor.ToLower() == "black") { payout = 1; }
                    else { payout = 0; }
                    break;
                case "even odd":
                    if (betType.ToString() == "even" && winNum % 2 == 0) { payout = 1; }
                    else if (betType.ToString() == "odd" && winNum % 2 != 0) { payout = 1; }
                    else { payout = 0; }
                    break;
                case "dozen":
                    if (betValue[0] == 1)
                    {
                        targetRange.Add(1);
                        targetRange.Add(2);
                        targetRange.Add(3);
                        targetRange.Add(4);
                        targetRange.Add(5);
                        targetRange.Add(6);
                        targetRange.Add(7);
                        targetRange.Add(8);
                        targetRange.Add(9);
                        targetRange.Add(10);
                        targetRange.Add(11);
                        targetRange.Add(12);
                    }
                    else if (betValue[0] == 2)
                    {
                        targetRange.Add(13);
                        targetRange.Add(14);
                        targetRange.Add(15);
                        targetRange.Add(16);
                        targetRange.Add(17);
                        targetRange.Add(18);
                        targetRange.Add(19);
                        targetRange.Add(20);
                        targetRange.Add(21);
                        targetRange.Add(22);
                        targetRange.Add(23);
                        targetRange.Add(24);
                    }
                    else if (betValue[0] == 3)
                    {
                        targetRange.Add(25);
                        targetRange.Add(26);
                        targetRange.Add(27);
                        targetRange.Add(28);
                        targetRange.Add(29);
                        targetRange.Add(30);
                        targetRange.Add(31);
                        targetRange.Add(32);
                        targetRange.Add(33);
                        targetRange.Add(34);
                        targetRange.Add(35);
                        targetRange.Add(36);
                    }

                    if (targetRange.Contains(winNum)) { payout = 2; } else { payout = 0; }
                    break;
                case "column":
                    if (betValue[0] == 1)
                    {
                        targetRange.Add(1);
                        targetRange.Add(4);
                        targetRange.Add(7);
                        targetRange.Add(10);
                        targetRange.Add(13);
                        targetRange.Add(16);
                        targetRange.Add(19);
                        targetRange.Add(22);
                        targetRange.Add(25);
                        targetRange.Add(28);
                        targetRange.Add(31);
                        targetRange.Add(34);
                    }
                    else if (betValue[0] == 2)
                    {
                        targetRange.Add(2);
                        targetRange.Add(5);
                        targetRange.Add(8);
                        targetRange.Add(11);
                        targetRange.Add(14);
                        targetRange.Add(17);
                        targetRange.Add(20);
                        targetRange.Add(23);
                        targetRange.Add(26);
                        targetRange.Add(29);
                        targetRange.Add(32);
                        targetRange.Add(35);
                    }
                    else if (betValue[0] == 3)
                    {
                        targetRange.Add(3);
                        targetRange.Add(6);
                        targetRange.Add(9);
                        targetRange.Add(12);
                        targetRange.Add(15);
                        targetRange.Add(18);
                        targetRange.Add(21);
                        targetRange.Add(24);
                        targetRange.Add(27);
                        targetRange.Add(30);
                        targetRange.Add(33);
                        targetRange.Add(36);
                    }

                    if (targetRange.Contains(winNum)) { payout = 2; } else { payout = 0; }
                    break;
                case "snake":
                    targetRange.Add(1);
                    targetRange.Add(5);
                    targetRange.Add(9);
                    targetRange.Add(12);
                    targetRange.Add(14);
                    targetRange.Add(16);
                    targetRange.Add(19);
                    targetRange.Add(23);
                    targetRange.Add(27);
                    targetRange.Add(30);
                    targetRange.Add(32);
                    targetRange.Add(34);

                    if (targetRange.Contains(winNum)) { payout = 2; } else { payout = 0; }
                    break;
            }

            return payout;
        }

        public double Keno(List<int> playerNumbers)
        {
            int match = 0;
            List<int> drawingNumbers = new List<int>();


            for (int d = 1; d < 11; d++)
            {
                int drawNum = r.Next(1, 80);
                drawingNumbers.Add(drawNum);
            }

            if (playerNumbers == drawingNumbers) { match = playerNumbers.Count(); }
            else
            {
                foreach (int n in playerNumbers)
                {
                    if (drawingNumbers.Contains(n)) { match += 1; }
                }
            }

            switch (match)
            {
                case 1:
                    payout = 1;
                    break;
                case 2:
                    payout = 2;
                    break;
                case 3:
                    payout = 3;
                    break;
                case 4:
                    payout = 5;
                    break;
                case 5:
                    payout = 10;
                    break;
                case 6:
                    payout = 15;
                    break;
                case 7:
                    payout = 20;
                    break;
                case 8:
                    payout = 30;
                    break;
                case 9:
                    payout = 50;
                    break;
                case 10:
                    payout = 100;
                    break;
                default:
                    payout = 0;
                    break;
            }

            return payout;
        }
    }
}