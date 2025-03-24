using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class BuggyController : BaseApiController
    {
        private readonly AppDbContext _context;

        public BuggyController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret(){
            return "text";
        }
        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound(){
            var data =_context.Users.Find(-1);
            if(data==null) return NotFound();

            return Ok(data);
        }
        [HttpGet("server-error")]
        public ActionResult<string> GetServerError(){
            var data=_context.Users.Find(-1);

            return data.ToString();
        }
        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest(){
            return BadRequest();
        } 
    }
}