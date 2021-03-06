using Casino.Data;
using Casino.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Casino.Services
{
    public class BankTransactionService
    {
        private readonly Guid _playerGuid;

        public BankTransactionService()
        {
        }
        public BankTransactionService(Guid userGuid)
        {
            _playerGuid = userGuid;
        }
        //Create 
        // FOR USE IN CHARGING CREDIT CARD
        public bool CreateBankTransactionCharge(BankTransactionCreate model)
        {
            //maybe return the model and player balance instead of bool
            var entity = new BankTransaction()
            {
                // PlayerId = _playerGuid,
                PlayerId = model.PlayerId,
                BankTransactionAmount = model.BankTransactionAmount,
                DateTimeOfTransaction = DateTimeOffset.Now

            };
            using (var ctx = new ApplicationDbContext())
            {
                ctx.BankTransactions.Add(entity);
                if (ctx.SaveChanges() != 0 && UpdatePlayerBankBalance/*(_playerGuid,*/(model.PlayerId, model.BankTransactionAmount)) // _playerGuid = model.PlayerId
                { return true; }
                return false;
            }

        }

        // Default method (no charge, only affects bankTransaction table and player account)
        public bool CreateBankTransaction(BankTransactionCreate model)
        {

            var entity = new BankTransaction()
            {
                PlayerId = _playerGuid,
                BankTransactionAmount = model.BankTransactionAmount,
                DateTimeOfTransaction = DateTimeOffset.Now

            };
            using (var ctx = new ApplicationDbContext())
            {
                ctx.BankTransactions.Add(entity);
                if (ctx.SaveChanges() != 0 && UpdatePlayerBankBalance(_playerGuid, model.BankTransactionAmount)) // _playerGuid = model.PlayerId
                { return true; }
                return false;
            }

        }
        //Return
        public IEnumerable<BankTransactionListItem> PlayerGetBankTransactions()//similar to PlayerGetBets(int playerId)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var query =
                    ctx
                        .BankTransactions
                        .Where(e => e.PlayerId == _playerGuid)
                                                .Select(
                            e =>
                                new BankTransactionListItem
                                {
                                    PlayerId = e.PlayerId,
                                    DateTimeOfTransaction = e.DateTimeOfTransaction,
                                    BankTransactionAmount = e.BankTransactionAmount,
                                    BankTransactionId = e.BankTransactionId,
                                }
                        );

                return query.ToArray();
            }
        }
        //Admin Get All by Player Guid
        public IEnumerable<BankTransactionListItem> AdminGetBankTransactions(Guid guid)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var query =
                    ctx
                        .BankTransactions
                        .Where(e => e.PlayerId == guid)
                                                .Select(
                            e =>
                                new BankTransactionListItem
                                {
                                    PlayerId = e.PlayerId,
                                    DateTimeOfTransaction = e.DateTimeOfTransaction,
                                    BankTransactionAmount = e.BankTransactionAmount,
                                    BankTransactionId = e.BankTransactionId,
                                }
                        );

                return query.ToArray();
            }
        }
        //Admin GetAll
        public IEnumerable<BankTransactionListItem> AdminGetBankTransactions()
        {
            using (var ctx = new ApplicationDbContext())
            {
                var query =
                    ctx
                        .BankTransactions
                        .Where(e => e.BankTransactionId > -1)
                                                .Select(
                            e =>
                                new BankTransactionListItem
                                {
                                    PlayerId = e.PlayerId,
                                    DateTimeOfTransaction = e.DateTimeOfTransaction,
                                    BankTransactionAmount = e.BankTransactionAmount,
                                    BankTransactionId = e.BankTransactionId,
                                }
                        );

                return query.ToArray();
            }
        }

        public BankTransactionListItem GetBankTransactionById(int id) //if this looks identical to BetListItem we can call that model instead of having 2
        {
            using (var ctx = new ApplicationDbContext())
            {
                var entity =
                    ctx
                        .BankTransactions
                        .Single(e => e.PlayerId == _playerGuid && e.BankTransactionId == id);
                return
                    new BankTransactionListItem

                    {
                        PlayerId = entity.PlayerId,
                        DateTimeOfTransaction = entity.DateTimeOfTransaction,
                        BankTransactionAmount = entity.BankTransactionAmount,
                        BankTransactionId = entity.BankTransactionId,

                    };
            }
        }
        //Delete
        public bool DeleteBankTransaction(int id, double amount)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var entity =
                    ctx
                       .BankTransactions
                       .Single(e => e.BankTransactionId == id && e.BankTransactionAmount == amount);
                ctx.BankTransactions.Remove(entity);
                if (UpdatePlayerBankBalance(entity.PlayerId, (-1) * amount) && ctx.SaveChanges() == 1)
                    return true;
                return false;
            }
        }

        //Helper
        //same method that is in BetService CreateBet
        private bool UpdatePlayerBankBalance(Guid playerId, double amount)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var entity =
                    ctx
                       .Players
                       .Single(e => e.PlayerId == playerId);
                entity.CurrentBankBalance = entity.CurrentBankBalance + amount;
                return (ctx.SaveChanges() == 1);
            }
        }
    }
}
