﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Casino.Data
{
    public enum PlayerState
    {

        AK, AL, AR, AS, AZ, CA, CO, CT, DC, DE, FL, GA, GU, HI, IA, ID, IL, IN, KS, KY, LA, MA, MD, ME, MI, MN, MO, MP, MS, MT, NC, ND, NE, NH, NJ, NM, NV, NY, OH, OK, OR, PA, PR, RI, SC, SD, TN, TX, UM, UT, VA, VI, VT, WA, WI, WV, WY
    }

    public enum TierStatus
    {
        bronze = 1,
        silver = 2,
        gold = 3
    }

    public class Player
    {
        [Key]
        public Guid PlayerId { get; set; }

        //public Guid PlayerNumber { get; set; }

        [Required]
        public string PlayerFirstName { get; set; }

        [Required]
        public string PlayerLastName { get; set; }

        public string PlayerPhone { get; set; }
        public string PlayerEmail { get; set; }
        public string PlayerAddress { get; set; }

        public PlayerState PlayerState { get; set; }
       
        [Required]
        public DateTime PlayerDoB { get; set; }

        [Required]
        public DateTime AccountCreated { get; set; }

        public TierStatus TierStatus { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public bool HasAccessToHighLevelGame { get; set; }

        [Required]
        public double CurrentBankBalance { get; set; }

        public virtual List<BankTransaction> BankTransactions { get; set; }

        public virtual List<Bet> Bets { get; set; }

        [Required]
        public bool EligibleForReward { get; set; }

        [Required]
        public bool AgeVerification { get; set; }

        [Required]
        public DateTimeOffset CreatedUtc { get; set; }

        public DateTimeOffset? ModifiedUtc { get; set; }

    }

}