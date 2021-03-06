using Casino.Data;
using Casino.Models;
using Casino.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Web.Http;
namespace Casino.WebApi.Controllers
{
    [Authorize]
    public class PlayerController : ApiController
    {
        private PlayerService CreatePlayerService()
        {
            var userId = Guid.Parse(User.Identity.GetUserId());
            var playerService = new PlayerService(userId);
            return playerService;
        }
        private PlayerService _service = new PlayerService();

        //Player gets own player info
        /// <summary>
        /// Return Player info for logged in Player - restricted to User/Player
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        [Route("api/Player/")]
        public IHttpActionResult GetSelf()
        {
            PlayerService playerService = CreatePlayerService();
            var player = playerService.GetSelf();

            return Ok(player);
        }

        //Admin get all players
        /// <summary>
        /// Get all Players - restricted to SuperAdmin, Admin
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Route("api/Player/admin")]
        public IHttpActionResult GetAllPlayers()
        {

            var players = _service.GetPlayers();
            return Ok(players);

        }

        //Admin gets player by Guid
        /// <summary>
        /// Get a Player by PlayerID/GUID - restricted to SuperAdmin, Admin
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Route("api/Player/admin/guid/{id}")]
        public IHttpActionResult GetById(Guid id)
        {

            var player = _service.GetPlayerById(id);

            return Ok(player);

        }
        //Admin gets players by Tier
        /// <summary>
        /// Get all Players by Tier Level - restricted to SuperAdmin, Admin
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Route("api/Player/admin/tier/{tierStatus}")]
        public IHttpActionResult GetByTierStatus(TierStatus tierStatus)
        {

            var player = _service.GetPlayerByTierStatus(tierStatus);
            return Ok(player);

        }
        //Admin get players with balance
        /// <summary>
        /// Get all Players with an account balance - restricted to SuperAdmin, Admin
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Route("api/Player/admin/balance")]
        public IHttpActionResult GetPlayerHasBalance()
        {

            var player = _service.GetPlayerByHasBalance();
            return Ok(player);

        }
        //Admin Get active players
        /// <summary>
        /// Get all active Players - restricted to SuperAdmin, Admin
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin, SuperAdmin")]
        [Route("api/Player/admin/active")]
        public IHttpActionResult GetActivePlayers()
        {
            var player = _service.GetActivePlayers();
            return Ok(player);
        }
        //User creates player account
        // Commented out - includes ddmmyyyy no slashes fix 
        /// <summary>
        /// Create a new Player account / BIRTHDAY MUST BE ENTERED MM/DD/YYYY WITH SLASHES
        /// </summary>
        /// <returns></returns>

        [Authorize(Roles = "User")]
        [HttpPost]
        [Route("api/makePlayer")]
        public IHttpActionResult Post(PlayerCreate player)  //*BRIAN* looks like it will never get beyond that first bool check with all the "else" returning "ok"
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var service = CreatePlayerService();

            bool query = service.CheckPlayerIdAlreadyExists();

            if (query == true)
                return BadRequest("UserID already in system");
            if (!service.CheckPlayer(player))
                return BadRequest("Date of birth has been entered in the incorrect format.  Please enter Date of Birth in the format of MM/DD/YYYY.");
            if (service.CheckDob(player) == false)  //Is this false or does it need to be revised.  If service.checkplayer = false
                return BadRequest("You are not 21 and can not create a player.");
            if (!service.CreatePlayer(player))
                return InternalServerError();
            else
                return Ok("Your Player Account has been created. Please buy chips to play games!");
        }

        /// <summary>
        /// Player can update contact info (phone number, address)
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        [Route("api/UpdatePlayer/")]
        public IHttpActionResult Put(PlayerEdit player)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var service = CreatePlayerService();
            if (!service.UpdatePlayer(player))
                return InternalServerError();
            return Ok();
        }

        //Player Deletes account(just makes it inactive)
        /// <summary>
        /// Set account to inactive - restricted to User/Player
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        [Route("api/Player/delete")]
        public IHttpActionResult Delete()
        {
            var service = CreatePlayerService();
            if (!service.DeletePlayer())
                return InternalServerError();

            return Ok("Your account is now inactive.  " +
                      "You will recieve a check in the mail within 5 business days for any remaining balance.");


        }
    }
}