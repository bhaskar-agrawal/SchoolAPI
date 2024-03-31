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

