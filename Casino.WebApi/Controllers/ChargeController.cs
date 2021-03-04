﻿using Casino.WebApi.Models;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using Casino.Services;
using Microsoft.AspNet.Identity;

namespace Casino.WebApi.Controllers
{
    [Authorize]
    public class ChargeController : ApiController
    {

        // UNIQUE STRIPE CHARGE CONTROLLER AT BOTTOM
        
        private MakeChargeService _makeChargeService = new MakeChargeService();
        private MakeChargeService CreateMakeChargeServiceForGuid()
        {
            var userId = Guid.Parse(User.Identity.GetUserId());
            var makeChargeService = new MakeChargeService(userId);
            return makeChargeService;
        }

        //Get
        //Get all by logged in Player
        [Authorize(Roles = "User")]
        [Route("api/charges/player")]
        public IHttpActionResult Get()
        {
            MakeChargeService chargeTransactionService = CreateMakeChargeServiceForGuid();
            var chargeTransactions = chargeTransactionService.PlayerGetChargeTransactions();
            return Ok(chargeTransactions);
        }

        //Get by id for logged in player
        [Authorize(Roles = "User")]
        [Route("api/charges/player/{id}")]

        public IHttpActionResult GetById(int id)
        {
            MakeChargeService chargeTransactionService = CreateMakeChargeServiceForGuid();
            var chargeTransactions = chargeTransactionService.GetChargeTransactionById(id);

            return Ok(chargeTransactions);
        }

        //Get all by Admin for Specific player
        [HttpGet]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Route("api/charges/admin/{guidAsString}")]
        public IHttpActionResult Get(string guidAsString)
        {
            Guid guid = Guid.Parse(guidAsString);

            var chargeTransactions = _makeChargeService.AdminGetChargeTransactions(guid);
            return Ok(chargeTransactions);
        }

        //Get all by Admin
        [HttpGet]
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Route("api/charges/admin")]
        public IHttpActionResult GetAll()
        {

            var chargeTransactions = _makeChargeService.AdminGetChargeTransactions();
            return Ok(chargeTransactions);
        }

        [Authorize(Roles = "User")]
        [Route("charge_Async")]
        public async Task<dynamic> Pay(RevisedChargeModel charge)
        {
            return await MakeChargeService.ChargeAsync(charge.CardNumber, charge.Month, charge.Year, charge.Cvc, charge.Zip, charge.Value);
        }

        [Authorize(Roles = "User")]
        [Route("charge_deposit_as_chips")]
        public IHttpActionResult Charge(RevisedChargeModel charge)
        {
            var newCharge = MakeChargeService.Charge(charge.CardNumber, charge.Month, charge.Year, charge.Cvc, charge.Zip, charge.Value);
            if (newCharge)
            {
                MakeChargeService chargeService = CreateMakeChargeServiceForGuid();
                // adds entry to ChargeForChips table AND BankTransaction Table AND Player table's PlayerBalance
                chargeService.CreateChargeforChips(charge);
                return Ok($"charge made: $ {charge.Value/100} charged to your card, and ${charge.Value / 100} added to your player account"); // put in message
            }
            else
                return InternalServerError(); // put in message
        }
    }
}
