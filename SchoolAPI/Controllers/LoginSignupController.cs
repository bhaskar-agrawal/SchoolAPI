﻿using Kusto.Data.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using SchoolAPI.Models;
using SchoolAPI.Service;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("schoolapi/auth/")]
    public class LoginSignupController : Controller
    {
        private SchoolAPIService schoolAPIService;
        private readonly IConfiguration configuration;

        public LoginSignupController(SchoolAPIService schoolApiService, IConfiguration configuration)
        {
            this.schoolAPIService = schoolApiService;
            this.configuration = configuration;
        }

        [HttpPost("signup")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SignUpAsync([FromBody] SignupRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                await schoolAPIService.CreateUserAsync(request.UserName, request.Password, request.Role);
                SignUpResponse response = new SignUpResponse
                {
                    UserName = request.UserName,
                    Role = request.Role
                };
                return Created(string.Empty, response);
                
            }
            catch (KustoException ex)
            {
                return StatusCode(ex.FailureCode, ex.Message + ":KustoException");
            }
            catch (Exception ex)
            {
                 switch (ex.Message)
                {
                    case "409 UserAlreadyExists":
                        return Conflict("UserName already used.");
                    default:
                        return StatusCode(500);
                }
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType (StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                string tokenString = await schoolAPIService.ValidateUserSigninAsync(request.UserName, request.Password);
                if (tokenString == null)
                {
                    return Unauthorized();
                }
                DateTime end = DateTime.Now;
                return Ok(tokenString);
            }
            catch (KustoException ex)
            {
                return StatusCode(ex.FailureCode, ex.Message + ":KustoException");
            }
            catch (Exception ex)
            {
                switch (ex.Message)
                {
                    case "404 UserNotFound":
                        return NotFound();
                    case "401 InvalidPassword":
                        return Unauthorized();
                    default:
                        return StatusCode(500); 
                }
            }
        }
    }
}
