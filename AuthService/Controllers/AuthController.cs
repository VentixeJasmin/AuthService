using AuthService.Models;
using AuthService.ServiceBus;
using AuthService.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;

namespace AuthService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(UserManager<UserEntity> userManager, UserService userService, SignInManager<UserEntity> signInManager, ServiceBusClient serviceBusClient) : ControllerBase
{
    private readonly UserManager<UserEntity> _userManager = userManager;
    private readonly UserService _userService = userService;
    private readonly SignInManager<UserEntity> _signInManager = signInManager;
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;


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
            EmailConfirmed = false
        };

        var result = await _userService.CreateUserAsync(user, dto.Password);
        if (result.Succeeded)
        {
            await PublishUserCreatedEvent(user.Email);
            return Ok("Registration successful. Please check your email and verify your account.");
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

    private async Task PublishUserCreatedEvent(string email)
    {
        var sender = _serviceBusClient.CreateSender("account-created");
        var eventMessage = new UserRegisteredEvent
        {
            Email = email
        };

        var message = new ServiceBusMessage(JsonSerializer.Serialize(eventMessage));

        await sender.SendMessageAsync(message);
    }
}
