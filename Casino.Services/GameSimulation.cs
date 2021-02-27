﻿using Casino.Data;
using Casino.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Casino.Models
{
    public class GameSimulation
    {


        private readonly Random _random = new Random();
        private Game _game = new Game();
        public double PlayGame(double betAmount, int gameId)
        {

            //Game game = GetGameById(gameId);
            //if (betAmount < game.MinBet || betAmount > game.MaxBet)
            //    return 0;
            int x = _random.Next(10);
            if (x <= 3)  //this is 60/40 odds
                return betAmount; //player wins, wins bet amount

            return (-1) * betAmount; //player loses, payout is negative the bet amount
        }

        // helper method to convert gamesim's payout to a bool if payout is +
        //This is being called from BetService
        public bool GameWinOutcome(double payout)
        {
            bool wonGame;
            if (payout > 0) { wonGame = true; } else { wonGame = false; }
            return wonGame;
        }

        
    }
}