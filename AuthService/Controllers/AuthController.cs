using AuthService.Models;
using AuthService.ServiceBus;
using AuthService.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;

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

        if (dto.AcceptTerms == false)
        {
            return BadRequest(ModelState);
        }

        var user = new UserEntity
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = false
        };

        var result = await _userService.CreateUserAsync(user, dto.Password);
        if (result.Succeeded)
        {
            var httpClient = new HttpClient();

            var verificationRequest = new VerificationRequestModel { Email = dto.Email, FirstName = dto.FirstName, LastName = dto.LastName };

            var apiResult = await httpClient.PostAsJsonAsync("https://verificationservice-jdf-e5d8azctc7fnapca.swedencentral-01.azurewebsites.net/", verificationRequest);
            if (!apiResult.IsSuccessStatusCode)
                return BadRequest("Something went wrong, please try again");

            return Ok(new { message = "Registration successful. Please check your email and verify your account.", userId = user.Id });
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
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


    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(ModelState);

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
        return Ok();
    }
}
