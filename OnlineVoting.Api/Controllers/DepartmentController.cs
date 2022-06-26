using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineVoting.Models.Dtos.Request;
using OnlineVoting.Services.Interfaces;

namespace OnlineVoting.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _deptServie;

        public DepartmentController(IDepartmentService deptServie)
        {
            _deptServie = deptServie;
        }


        [HttpPost("create-department", Name = "Create-Department")]
        public async Task<IActionResult> CreateDepartment([FromQuery] DeptCreateDto model)
        {
            string dept = await _deptServie.CreateDepartment(model);

            return Ok(dept);
        }
    }
}
