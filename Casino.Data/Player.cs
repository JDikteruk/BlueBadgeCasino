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
        [Required]
        public string PlayerEmail { get; set; }
        public string PlayerAddress { get; set; }
        public PlayerState PlayerState { get; set; }

        public string PlayerZipCode { get; set; }

        [Display(Name = "Birthday: Enter in format MMDDYYY (example : 10312021")]
        [Required]
        //public DateTime PlayerDob { get; set; }
        public string PlayerDob { get; set; }

        [Required]
        public DateTimeOffset AccountCreated { get; set; }


        public TierStatus TierStatus { get; set; } = TierStatus.bronze;
        private bool _isActive;
        public bool IsActive
        {
            get
            {
                //if (this.IsActive != false)
                {
                    //bool test;
                    TimeSpan accountCreate = DateTime.Now - AccountCreated;
                    if (accountCreate.TotalDays < 180)
                    {
                        _isActive = true;
                        return true;
                    }
                    _isActive = false;
                    return false;
                }
                //return false;
            }
            set { _isActive = value; } //or _ = value; also works the same.  It returns correctly when called, but the table in SQL DB does not update. 
        }

        public bool HasAccessToHighLevelGame { get; set; }

        public double CurrentBankBalance { get; set; }

        public virtual List<BankTransaction> BankTransactions { get; set; }

        public virtual List<Bet> Bets { get; set; }

        //[Required]
        //public bool EligibleForReward { get; set; }

        public bool AgeVerification { get; set; }

        //public DateTimeOffset CreatedUtc { get; set; }

        //public DateTimeOffset? ModifiedUtc { get; set; }

        
    }

}