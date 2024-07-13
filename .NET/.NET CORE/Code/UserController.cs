using FoundryLaw.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FoundryLaw.Api.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersList()
        {

            return Ok(await _userService.GetUserList());
        }

        [HttpPost]
        public async Task<IActionResult> InsertUser(UserCreateViewModel user)
        {
            var createdby = GetUser().id;
            var result = await _userService.InsertUser(user, createdby);
            return Ok(result);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            return Ok(await _userService.GetUserById(id));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateViewModel user)
        {
            var modifieddby = GetUser().id;
            var result = await _userService.UpdateUser(user, modifieddby);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var modifiedBy = GetUser().id;
            var result = await _userService.DeleteUser(id, modifiedBy);
            return Ok(result);

        }

    }

}
