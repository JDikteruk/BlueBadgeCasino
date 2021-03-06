using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Casino.Data
{
    public class BankTransaction
    {
        [Key]
        public int BankTransactionId { get; set; }

        [ForeignKey(nameof(Player))]
        public Guid PlayerId { get; set; }
        public virtual Player Player { get; set; }

        [Required]
        public DateTimeOffset DateTimeOfTransaction { get; set; }

        public double BankTransactionAmount { get; set; } //positive for deposit, negative for withdraw


    }
}
