using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.Arm;

namespace AuthService.Controllers; 

[Route("api/[controller]")]
[ApiController]
public class AuthController(UserManager<UserEntity> userManager, UserService userService, SignInManager<UserEntity> signInManager) : ControllerBase
{
    private readonly UserManager<UserEntity> _userManager = userManager; 
    private readonly UserService _userService = userService;
    private readonly SignInManager<UserEntity> _signInManager = signInManager;

    [HttpPost("signup")]
    public async Task<IActionResult> RegisterUser([FromBody] UserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new UserEntity
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            EmailConfirmed = false
        }; 

        //Fetch info from servicebus

        var result = await _userService.CreateUserAsync(user, dto.Password);
        if (result.Succeeded)
        {
            return Created();
        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] SignInDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return BadRequest(dto);
        }

        if (!user.EmailConfirmed)
        {
            ModelState.AddModelError(string.Empty, "Email address not confirmed.");
            return BadRequest(dto);
        }

        var result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, dto.RememberMe, false); 
        return Ok(result);

    }

    [HttpPost("signout")]
    public new async Task<IActionResult> SignOut()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }
}
