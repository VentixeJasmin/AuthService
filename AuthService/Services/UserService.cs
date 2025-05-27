using AuthService.Data;
using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AuthService.Services;

public class UserService(UserManager<UserEntity> userManager)
{
    private readonly UserManager<UserEntity> _userManager = userManager;

    public async Task<IdentityResult> CreateUserAsync(UserEntity user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<UserEntity> ConfirmUser(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            return user;
        }
        
        return null!; 
    }

    public async Task<IEnumerable<UserEntity>> GetAllUsersAsync()
    {
        return await _userManager.Users.ToListAsync();
    }

    public async Task<UserEntity?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return null;

        return user;
    }

    public async Task<UserEntity?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return null;

        return user;
    }

    public async Task<IdentityResult> UpdateUserAsync(string id, UserDto updatedUser)
    {
        try
        {
            var existingUser = await _userManager.FindByIdAsync(id);

            if (existingUser == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });

           
            existingUser.FirstName = updatedUser.FirstName;
            existingUser.LastName = updatedUser.LastName;

            if (existingUser.Email != updatedUser.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(existingUser, updatedUser.Email);
                if (!setEmailResult.Succeeded)
                    return setEmailResult;
            }


            return await _userManager.UpdateAsync(existingUser);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating user :: {ex.Message}");
            return IdentityResult.Failed(new IdentityError { Description = $"Error updating user: {ex.Message}" });
        }
    }

    public async Task<IdentityResult> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });

        return await _userManager.DeleteAsync(user);

        //^Suggestion by Claude AI to make the controller actions simpler 
    }
}
