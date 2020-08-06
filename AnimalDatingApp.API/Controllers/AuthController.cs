using Microsoft.AspNetCore.Mvc;
using AnimalDatingApp.API.Data;
using System.Threading.Tasks;
using AnimalDatingApp.API.Models;
using AnimalDatingApp.API.Dtos;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System;
using AutoMapper;

namespace AnimalDatingApp.API.Controllers
{
    [Route("api/[controller]")]//attribute based routing
    [ApiController] //if this is not used, then we have to create a model state below.
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        private readonly IMapper _mapper;
        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _config = config;
            _repo = repo;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDto userForRegisterDto)
        {

            //validate request

            //    if(!ModelState.IsValid) // don't need this if we're using the API controller above.
            //         return BadRequest(ModelState);

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists");

            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);

            return CreatedAtRoute("GetUser", 
                new { controller = "Users", id = createdUser.Id }, userToReturn);
        }

        [HttpPost("login")]

        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            //throw new Exception("hello");
            var userFromRepo = await _repo.Login(userForLoginDto.Username, userForLoginDto.Password);
 
            if (userFromRepo == null)
                return Unauthorized();

            var claims = new[]
            {
                    new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                    new Claim(ClaimTypes.Name, userFromRepo.Username)

                };

            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var TokenHandler = new JwtSecurityTokenHandler();

            var token = TokenHandler.CreateToken(tokenDescriptor);

            var user = _mapper.Map<UserForListDto>(userFromRepo);


            // var token = new JwtSecurityToken(
            //     issuer: "localhost",
            //     audience: "localhost",
            //     claims: claims,
            //     expires: DateTime.Now.AddDays(1),
            //     signingCredentials: creds
            // );

            return Ok(new
            {
                token = TokenHandler.WriteToken(token),
                user
            });
           }
    }
}