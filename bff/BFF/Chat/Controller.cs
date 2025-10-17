using BFF.Chat.Messages;
using BFF.Databases.Messages;
using Cassandra.Data.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Wolverine;

namespace BFF.Chat
{
    [Route("api/messages")]
    [Authorize]
    [ApiController]
    public class Controller : ControllerBase
    {
        private readonly Context _context;
        private readonly IHubContext<Hub> _hubContext;
        private readonly IMessageBus _messageBus;
        private readonly IMapper _mapper;
        public Controller(Context context,
            IHubContext<Hub> hubContext,
            IMessageBus messageBus,
            IMapper mapper)
        {
            _context = context;
            _hubContext = hubContext;
            _messageBus = messageBus;
            _mapper = mapper;
        }
        [HttpGet("messages")]
        public async Task<IActionResult> Messages([FromQuery] Parameters parameters)
        {
            CqlQuery<Table> query = _context.Messages;

            var messages = await query.ExecuteAsync();

            if (parameters.PageIndex.HasValue && parameters.PageIndex.HasValue && parameters.PageSize > 0 && parameters.PageIndex >= 0)
                messages = messages.Skip(parameters.PageIndex.Value * parameters.PageSize.Value).Take(parameters.PageSize.Value);
            var responses = messages.Select(x => new Response
            {
                Name = x.Name,
                Message = x.Message,
                SentDate = x.SentDate
            }).ToList();
            _mapper.All.SetAvatar(responses);

            return Ok(responses);
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody]SendMessage.Payload payload)
        {
            var message = new Table
            {
                Message = payload.Message,
                Name = payload.Name,
                Id = Guid.NewGuid(),
                SentDate = DateTime.UtcNow.ToString(),
                Day = DateTime.UtcNow.Day,
                Month = DateTime.UtcNow.Month,
                Year = DateTime.UtcNow.Year
            };
            await _context.Messages.Insert(message).ExecuteAsync();
            await _messageBus.PublishAsync(new Chat.SendMessage.Messager.Message(message.Id));
            await _hubContext.Clients.All.SendAsync("message-sent", message.Id);
            return CreatedAtAction(nameof(Messages), new { id = message.Id });
        }

        [HttpPut("edit-message")]
        public async Task<IActionResult> EditMessage([FromBody] EditMessage.Payload payload)
        {
            var message = await _context.Messages.Where(m => m.Id == payload.MessageId&&m.Month==payload.Month&&m.SentDate==payload.SentDate)
                .FirstOrDefault().ExecuteAsync();
            if (message == null)
                return NotFound();

            await _context.Messages
                .Where(m => m.Id == payload.MessageId && m.Month == payload.Month && m.SentDate == payload.SentDate)
                .Select(x=>new Table 
                { 
                    Message=payload.Message,
                })
                .Update().ExecuteAsync();
            await _messageBus.PublishAsync(new Chat.SendMessage.Messager.Message(message.Id));
            await _hubContext.Clients.All.SendAsync("message-edited", message.Id);
            return NoContent();
        }

        [HttpDelete("delete-message")]
        public async Task<IActionResult> DeleteMessage([FromQuery] DeleteMessage.Parameters parameters)
        {
            var message = await _context.Messages.Where(m => m.Id == parameters.MessageId&&m.Month==parameters.Month&&m.SentDate==parameters.SentDate).FirstOrDefault().ExecuteAsync();
            if (message == null)
                return NotFound();
            await _context.Messages
                .Where(m => m.Id == parameters.MessageId && m.Month == parameters.Month && m.SentDate == parameters.SentDate)
                .Delete().ExecuteAsync();
            await _hubContext.Clients.All.SendAsync("message-deleted", message.Id);
            return NoContent();
        }
    }
}
