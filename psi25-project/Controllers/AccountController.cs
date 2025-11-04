using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace psi25_project.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
private readonly UserManager<IdentityUser> _userManager;
private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)  
    {  
        _userManager = userManager;  
        _signInManager = signInManager;  
    }  

    [HttpPost("Register")]  
    public async Task<IActionResult> Register([FromBody] RegisterModel model)  
    {  
        if (!ModelState.IsValid)  
            return BadRequest(ModelState);  

        var user = new IdentityUser { UserName = model.Username, Email = model.Email };  
        var result = await _userManager.CreateAsync(user, model.Password);  

        if (result.Succeeded)  
            return Ok(new { Message = "User registered successfully." });  

        return BadRequest(result.Errors);  
    }  

    [HttpPost("Login")]  
    public async Task<IActionResult> Login([FromBody] LoginModel model)  
    {  
        if (!ModelState.IsValid)  
            return BadRequest(ModelState);  

        var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: false, lockoutOnFailure: false);  

        if (result.Succeeded)  
            return Ok(new { Message = "User logged in successfully." });  

        return Unauthorized(new { Message = "Invalid username or password." });  
    }  

    [HttpPost("Logout")]  
    public async Task<IActionResult> Logout()  
    {  
        await _signInManager.SignOutAsync();  
        return Ok(new { Message = "User logged out successfully." });  
    }  
}  

// DTOs for input
public class RegisterModel  
{  
    public string Username { get; set; }  
    public string Email { get; set; }  
    public string Password { get; set; }  
}  

public class LoginModel  
{  
    public string Username { get; set; }  
    public string Password { get; set; }  
}  

}
