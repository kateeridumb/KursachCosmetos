using CosmeticShopAPI.DTOs;
using CosmeticShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CosmeticShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly CosmeticsShopDbContext _context;

        public UsersController(CosmeticsShopDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();

            var dtos = users.Select(u => new UserDTO
            {
                IdUser = u.Id_User,
                LastName = u.LastName,
                FirstName = u.FirstName,
                MiddleName = u.MiddleName,
                Email = u.Email,
                Phone = u.Phone ?? "",
                RoleUs = u.RoleUs,
                DateRegistered = u.DateRegistered.ToDateTime(TimeOnly.MinValue),
                StatusUs = u.StatusUs
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id_User == id);

            if (user == null)
                return NotFound();

            var dto = new UserDTO
            {
                IdUser = user.Id_User,
                LastName = user.LastName,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                Email = user.Email,
                Phone = user.Phone ?? "",
                RoleUs = user.RoleUs,
                DateRegistered = user.DateRegistered.ToDateTime(TimeOnly.MinValue),
                StatusUs = user.StatusUs
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(UserDTO dto)
        {
            var existingUser = await _context.Users
        .AnyAsync(u => u.Email == dto.Email);
            if (existingUser)
                return BadRequest("Пользователь с таким email уже существует.");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var sql = @"
        INSERT INTO Users (LastName, FirstName, MiddleName, Email, PasswordHash, Phone, RoleUs, DateRegistered, StatusUs)
        VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})";

            var dateRegistered = DateTime.UtcNow.Date;

            await _context.Database.ExecuteSqlRawAsync(sql,
                dto.LastName,
                dto.FirstName,
                dto.MiddleName,
                dto.Email,
                passwordHash,
                string.IsNullOrEmpty(dto.Phone) ? null : dto.Phone,
                "Клиент", 
                dateRegistered,
                "Активен" 
            );

            var newId = await _context.Users.MaxAsync(u => u.Id_User);

            var userDto = new UserDTO
            {
                IdUser = newId,
                LastName = dto.LastName,
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                Email = dto.Email,
                Phone = dto.Phone,
                RoleUs = "Клиент",
                DateRegistered = dateRegistered,
                StatusUs = "Активен"
            };

            return CreatedAtAction(nameof(GetUser), new { id = userDto.IdUser }, userDto);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserDTO dto)
        {
            string? passwordHash = null;
            if (!string.IsNullOrEmpty(dto.Password))
                passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var emailExists = await _context.Users
     .AnyAsync(u => u.Email == dto.Email && u.Id_User != id);

            if (emailExists)
                return BadRequest("Этот Email уже занят.");

            if (!string.IsNullOrEmpty(dto.Phone))
            {
                var phoneExists = await _context.Users
                    .AnyAsync(u => u.Phone == dto.Phone && u.Id_User != id);

                if (phoneExists)
                    return BadRequest("Этот телефон уже занят.");
            }

            var sql = @"
        UPDATE Users
        SET LastName = {0}, FirstName = {1}, MiddleName = {2}, Email = {3}, 
            PasswordHash = COALESCE({4}, PasswordHash),
            Phone = {5}, RoleUs = {6}, DateRegistered = {7}, StatusUs = {8}
        WHERE Id_User = {9}";

            var rows = await _context.Database.ExecuteSqlRawAsync(sql,
                dto.LastName,
                dto.FirstName,
                dto.MiddleName,
                dto.Email,
                passwordHash,
                string.IsNullOrEmpty(dto.Phone) ? null : dto.Phone,
                dto.RoleUs,
                dto.DateRegistered.Date,
                dto.StatusUs,
                id);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }

    [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var sql = "DELETE FROM Users WHERE Id_User = {0}";
            var rows = await _context.Database.ExecuteSqlRawAsync(sql, id);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Пользователь с таким email уже существует.");

            if (await _context.Users.AnyAsync(u => u.Phone == dto.Phone))
                return BadRequest("Пользователь с таким телефоном уже существует.");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var sql = @"
        INSERT INTO Users (LastName, FirstName, MiddleName, Email, PasswordHash, Phone, RoleUs, DateRegistered, StatusUs)
        VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})";

            var dateRegistered = DateTime.UtcNow.Date;

            await _context.Database.ExecuteSqlRawAsync(sql,
                dto.LastName,
                dto.FirstName,
                dto.MiddleName,
                dto.Email,
                passwordHash,
                dto.Phone,
                "Клиент",   
                dateRegistered,
                "Активен"   
            );

            return Ok("Регистрация успешна!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Username);

            if (user == null)
                return Unauthorized("Неверный логин или пароль.");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
                return Unauthorized("Неверный логин или пароль.");

            if (user.StatusUs == "Заблокирован")
                return Forbid("Пользователь заблокирован.");

            var result = new
            {
                user.Id_User,
                user.FirstName,
                user.LastName,
                user.RoleUs,
                user.Email
            };

            return Ok(result);
        }

    }

}
