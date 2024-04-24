using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Service;
using SchoolAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using SchoolAPI.DAL;
using Kusto.Data.Exceptions;
using Microsoft.Data.SqlClient;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("schoolapi")]
    public class SchoolController : Controller
    {
        private SchoolAPIService schoolAPIService;

        public SchoolController(SchoolAPIService schoolApiService)
        {
            this.schoolAPIService = schoolApiService;
        }

        /// <summary>
        /// Update marks for a student, can be done by teacher only
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code = "201">Marks updated successfully</response>
        /// <response code = "404">Student not found</response>
        /// <response code = "409">Marks already exists</response>
        /// <response code = "500">Internal server error DB or API is down</response>

        [Authorize(Policy = "UserRolePolicy")]
        [HttpPost("marks")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMarksForStudent([FromBody] MarksUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                await this.schoolAPIService.UpdateMarksForStudentAsync(request.StudentName, request.SubjectName, request.Marks);
                return Ok();
            }
            catch (KustoException ex)
            {
                return StatusCode(ex.FailureCode, ex.Message);
            }
            catch (Exception ex)
            {
                switch (ex.Message)
                {
                    case "404 StudentNotFound":
                        return NotFound("Student not found.");
                    case "409 MarksAlreadyExists":
                        return Conflict("Marks already exists for the student for given subject.");
                    default:
                        return StatusCode(500);
                }
            }
        }

        /// <summary>
        /// Get marks of a student subjectwise
        /// </summary>
        /// <param name="studentName"></param>
        /// <returns></returns>
        /// <response code = "200">Success in getting marks and subject</response>
        /// <response code = "404">Student not found</response>
    
        [HttpGet("marks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMarksOfStudent(string studentName)
        {
            try
            {
                List<Subject> subjectMarksListOfStudent = await this.schoolAPIService.GetMarksForStudentAsync(studentName);
                return Ok(subjectMarksListOfStudent);
            }
            catch (KustoException ex)
            {
                return StatusCode(ex.FailureCode, ex.Message + ":KustoException");
            }
            catch (Exception ex)
            {
                switch (ex.Message)
                {
                    case "404 StudentNotFound":
                        return NotFound("Student not found.");
                    default:
                        return StatusCode(500);
                }
            }
        }
    }
}

