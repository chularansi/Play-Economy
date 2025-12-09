using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Common.PostgresDB;
using Play.User.Contracts;
using Play.User.Service.Data;
using static Play.User.Service.Dtos.Dtos;

namespace Play.User.Service.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        //private readonly ApplicationDbContext context;

        private readonly IUnitOfWork unitOfWork;
        private readonly IPublishEndpoint publishEndpoint;

        public UsersController(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint)
        {
            this.unitOfWork = unitOfWork;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        [Authorize(Policy = "read_access")]
        //[Authorize(Roles = "Admin")]
        public async Task<IEnumerable<UserDto>> GetAsync()
        {
            var users = (await this.unitOfWork.PostgresRepository<Entities.User>().GetAllAsync()).Select(user => user.AsDto());

            return users;
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "read_access")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> GetByIdAsync(Guid id)
        {
            var user = await this.unitOfWork.PostgresRepository<Entities.User>().GetAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user.AsDto());
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "write_access")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await this.unitOfWork.PostgresRepository<Entities.User>().GetAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.Gil = updateUserDto.Gil;
            await this.unitOfWork.PostgresRepository<Entities.User>().UpdateAsync(user);

            await this.publishEndpoint.Publish(new UserUpdated(user.Id, user.Email, user.Gil));

            return NoContent();
        }
    }
}
