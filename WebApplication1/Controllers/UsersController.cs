using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener la lista de los usuarios (Por tenant => filtra en el services)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all users for current tenant");
        var users = await _userService.GetAllUsersAsync(cancellationToken);
        _logger.LogInformation("Successfully retrieved {count} users", users.Count());
        return Ok(users);
    }

    /// <summary>
    /// Obtener un usuario por ID -> Deberia de chequear si es un usuario de su tenant y que solo funcione si es admin en un futuro
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching user with ID {id}", id);
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        return Ok(user);
    }

    /// <summary>
    /// Crear un nuevo usuario
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto createUserDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        _logger.LogInformation("Creating new user with email {Email}", createUserDto.Email);
        var user = await _userService.CreateUserAsync(createUserDto, cancellationToken);
        _logger.LogInformation("Successfully created new user {Id} with username {Username}", user.Id, user.Username);
        
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>
    ///  Controller para actualizar un usuario
    /// </summary>
    /// <param name="id">INT</param>
    /// <param name="updateUserDto">Dto para la actualizacion de usuarios</param>
    /// <returns>UserDto -> Sin password</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserDto updateUserDto, CancellationToken cancellationToken)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);
        
        _logger.LogInformation("Updating user with ID {id}", id);
        var user = await _userService.UpdateUserAsync(id, updateUserDto, cancellationToken);
        _logger.LogInformation("Successfully updated user with ID {Id}", id);
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to delete user with ID {id}", id);
        var deleted = await _userService.DeleteUserAsync(id,cancellationToken);
        
        if(!deleted)
        {
            _logger.LogWarning("Failed to delete user with ID {Id}", id);
            return NotFound(new { message = $"User with the ID: {id} not found." });
        }
        
        _logger.LogInformation("Successfully deleted user with ID {Id}", id);
        return NoContent();
    }

    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> Exists(int id, CancellationToken cancellationToken)
    {
        var exists = await _userService.ExistsUserAsync(id, cancellationToken);
        return Ok(new {exists});
    }
}