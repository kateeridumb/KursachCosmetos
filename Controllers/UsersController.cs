using CosmeticShopAPI.DTOs;
using CosmeticShopAPI.Models;
using CosmeticShopAPI.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
namespace CosmeticShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly CosmeticsShopDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(CosmeticsShopDbContext context, TokenService tokenService, IEmailService emailService, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Ok("Если email существует, инструкции будут отправлены");
            }

            var token = _tokenService.GeneratePasswordResetToken(user.Email, user.Id_User);
            var resetLink = $"https://localhost:7001/Account/ResetPassword?token={WebUtility.UrlEncode(token)}";

            var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .button {{ background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Восстановление пароля - CosmeticShop</h2>
        <p>Здравствуйте, {user.FirstName} {user.LastName}!</p>
        <p>Вы запросили восстановление пароля для вашей учетной записи.</p>
        <p>Для установки нового пароля нажмите на кнопку ниже:</p>
        <p><a href='{resetLink}' class='button'>Восстановить пароль</a></p>
        <p>Если кнопка не работает, скопируйте ссылку в браузер:</p>
        <p><small>{resetLink}</small></p>
        <div class='footer'>
            <p><strong>Ссылка действительна в течение 1 часа.</strong></p>
            <p>Если вы не запрашивали восстановление пароля, проигнорируйте это письмо.</p>
            <p>С уважением,<br>Команда CosmeticShop</p>
        </div>
    </div>
</body>
</html>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, "Восстановление пароля - CosmeticShop", emailBody);
                return Ok("Если email существует, инструкции будут отправлены");
            }
            catch
            {
                return Ok("Если email существует, инструкции будут отправлены");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var (isValid, email, userId) = _tokenService.ValidatePasswordResetToken(request.Token);

            if (!isValid)
                return BadRequest("Неверный или просроченный токен");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id_User == userId && u.Email == email);
            if (user == null)
                return BadRequest("Пользователь не найден");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var sql = "UPDATE Users SET PasswordHash = {0} WHERE Id_User = {1}";
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, passwordHash, userId);

            if (rowsAffected > 0)
            {
                return Ok("Пароль успешно изменен");
            }
            else
            {
                return BadRequest("Ошибка при обновлении пароля");
            }
        }

        [HttpPost("register-employee")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterEmployeeRequest request)
        {
            Console.WriteLine($"[REGISTER_EMPLOYEE] Начало регистрации сотрудника: {request.Email}");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                Console.WriteLine($"[REGISTER_EMPLOYEE] Email уже существует: {request.Email}");
                return BadRequest("Пользователь с таким email уже существует.");
            }

            if (await _context.Users.AnyAsync(u => u.Phone == request.Phone))
            {
                Console.WriteLine($"[REGISTER_EMPLOYEE] Телефон уже существует: {request.Phone}");
                return BadRequest("Пользователь с таким телефоном уже существует.");
            }

            var generatedPassword = GenerateRandomPassword();
            Console.WriteLine($"[REGISTER_EMPLOYEE] Сгенерирован пароль для {request.Email}");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(generatedPassword);

            var sql = @"
        INSERT INTO Users (LastName, FirstName, MiddleName, Email, PasswordHash, Phone, RoleUs, DateRegistered, StatusUs)
        VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})";

            var dateRegistered = DateTime.UtcNow.Date;

            await _context.Database.ExecuteSqlRawAsync(sql,
                request.LastName,
                request.FirstName,
                request.MiddleName,
                request.Email,
                passwordHash,
                request.Phone,
                request.RoleUs,
                dateRegistered,
                "Активен"
            );

            Console.WriteLine($"[REGISTER_EMPLOYEE] Сотрудник добавлен в БД: {request.Email}");

            var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .credentials {{ background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Добро пожаловать в команду CosmeticShop! 🎉</h2>
        <p>Здравствуйте, {request.FirstName} {request.LastName}!</p>
        <p>Вы были зарегистрированы в системе CosmeticShop как {request.RoleUs}.</p>
        
        <div class='credentials'>
            <h3>Ваши данные для входа:</h3>
            <p><strong>Email:</strong> {request.Email}</p>
            <p><strong>Пароль:</strong> {generatedPassword}</p>
        </div>

        <p>Для входа в систему перейдите по ссылке: 
           <a href='https://localhost:7001/Account/Login'>https://localhost:7001/Account/Login</a>
        </p>

        <p><strong>Рекомендуем сменить пароль после первого входа в систему.</strong></p>

        <div class='footer'>
            <p>С уважением,<br>Команда CosmeticShop</p>
        </div>
    </div>
</body>
</html>";

            try
            {
                Console.WriteLine($"[REGISTER_EMPLOYEE] Пытаемся отправить email на: {request.Email}");
                await _emailService.SendEmailAsync(request.Email, "Регистрация в системе CosmeticShop", emailBody);
                Console.WriteLine($"[REGISTER_EMPLOYEE] Email успешно отправлен на: {request.Email}");

                return Ok(new
                {
                    Message = "Сотрудник успешно зарегистрирован",
                    Email = request.Email
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REGISTER_EMPLOYEE] ОШИБКА отправки email: {ex.Message}");
                Console.WriteLine($"[REGISTER_EMPLOYEE] StackTrace: {ex.StackTrace}");

                return Ok(new
                {
                    Message = "Сотрудник зарегистрирован, но не удалось отправить email с паролем",
                    Email = request.Email,
                    Note = "Пожалуйста, сообщите пароль сотруднику другим способом"
                });
            }
        }

        private string GenerateRandomPassword()
        {
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            var random = new Random();
            var password = new char[12];

            password[0] = uppercase[random.Next(uppercase.Length)];
            password[1] = lowercase[random.Next(lowercase.Length)];
            password[2] = digits[random.Next(digits.Length)];
            password[3] = special[random.Next(special.Length)];

            const string allChars = uppercase + lowercase + digits + special;
            for (int i = 4; i < 12; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            return new string(password.OrderBy(x => random.Next()).ToArray());
        }

        [HttpPost("backup")]
        public async Task<IActionResult> CreateBackup()
        {
            try
            {
                string backupFolder = @"C:\Users\PRO\source\repos\CosmeticShopAPI\backups";

                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);

                string fileName = $"CosmeticsShop_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string filePath = Path.Combine(backupFolder, fileName);

                string dbName = _context.Database.GetDbConnection().Database;

                string masterConn = _configuration.GetConnectionString("AdminConnection")
                                  ?? _configuration.GetConnectionString("DefaultConnection");

                using var connection = new SqlConnection(masterConn);
                await connection.OpenAsync();

                string sql = $@"
            BACKUP DATABASE [{dbName}] 
            TO DISK = '{filePath.Replace("'", "''")}'
            WITH FORMAT, 
                 MEDIANAME = 'CosmeticShop_Backups',
                 NAME = 'Full Backup of {dbName}',
                 STATS = 10,
                 CHECKSUM";

                using var command = new SqlCommand(sql, connection);
                command.CommandTimeout = 3600;

                await command.ExecuteNonQueryAsync();

                var fileInfo = new FileInfo(filePath);
                return Ok(new
                {
                    message = "Резервная копия успешно создана",
                    fileName = fileName,
                    fileSize = fileInfo.Length,
                    created = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BACKUP_ERROR] {ex.Message}");
                return StatusCode(500, $"Ошибка при создании резервной копии: {ex.Message}");
            }
        }
        [HttpPost("restore")]
        public async Task<IActionResult> RestoreDatabase(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран");

            try
            {
                string tempDir = "C:\\Users\\PRO\\source\\repos\\CosmeticShopAPI\\backups";
                Directory.CreateDirectory(tempDir);

                string tempFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string tempBackupPath = Path.Combine(tempDir, tempFileName);

                using (var stream = new FileStream(tempBackupPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string dbName = "CosmeticsShopDB"; 

                string masterConn = _configuration.GetConnectionString("AdminConnection")
                                  ?? _configuration.GetConnectionString("DefaultConnection");

                using var connection = new SqlConnection(masterConn);
                await connection.OpenAsync();

                string sql = $@"
            ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            RESTORE DATABASE [{dbName}] FROM DISK = '{tempBackupPath.Replace("'", "''")}' WITH REPLACE;
            ALTER DATABASE [{dbName}] SET MULTI_USER;";

                using var command = new SqlCommand(sql, connection);
                command.CommandTimeout = 300;

                await command.ExecuteNonQueryAsync();

                System.IO.File.Delete(tempBackupPath);

                return Ok("База данных успешно восстановлена");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при восстановлении: {ex.Message}");
            }
        }
    }
}