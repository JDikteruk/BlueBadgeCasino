﻿using Casino.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;

namespace Casino.Models
{
    public class PlayerDetail
    {
        public Guid PlayerId { get; set; }
        public string PlayerFirstName { get; set; }
        public string PlayerLastName { get; set; }
        public string PlayerPhone { get; set; }
        public string PlayerEmail { get; set; }
        public string PlayerAddress { get; set; }
        //[Display (Name = (PlayerState)PlayerState.ToString())]
        [JsonConverter(typeof(StringEnumConverter))]
        public PlayerState PlayerState { get; set; }

        public string PlayerZipCode { get; set; }

        [Required]
        [Display(Name = "Birthday: Enter in format MMDDYYY (example : 10312021")]
        //public DateTime PlayerDob { get; set; }
        public string PlayerDob { get; set; }
        public DateTimeOffset AccountCreated { get; set; }
        public TierStatus TierStatus { get; set; }
        public bool IsActive { get; set; }
        public bool HasAccessToHighLevelGame { get; set; }
        public double CurrentBankBalance { get; set; }
        //public bool EligibleForReward { get; set; }
        public bool AgeVerification { get; set; }

        //[Display(Name="Created")]
        //public DateTimeOffset CreatedUtc { get; set; }

        //[Display(Name="Modified")]
        //public DateTimeOffset? ModifiedUtc { get; set; }
    }
}
