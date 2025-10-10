using BFF.Databases.Messages;
using Cassandra.Data.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Wolverine;

namespace BFF.Messages
{
    [Route("api/messages")]
    [ApiController]
    public class Controller : ControllerBase
    {
        private readonly Context _context;
        private readonly IHubContext<Hub> _hubContext;
        private readonly IMessageBus _messageBus;
        public Controller(Context context, 
            IHubContext<Hub> hubContext,
            IMessageBus messageBus)
        {
            _context = context;
            _hubContext = hubContext;
            _messageBus = messageBus;
        }
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GET.Parameters parameters)
        {
            CqlQuery<Table> query = _context.Messages;
            if (parameters.Id.HasValue)
                query = query.Where(m => m.Id == parameters.Id.Value);

            if (!string.IsNullOrEmpty(parameters.Content))
                query = query.Where(m => m.Content == parameters.Content);

            if (!string.IsNullOrEmpty(parameters.Receiver))
                query = query.Where(m => m.Receiver == parameters.Receiver);

            if (!string.IsNullOrEmpty(parameters.Sender))
                query = query.Where(m => m.Sender == parameters.Sender);
            var messages = await query.ExecuteAsync();


            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] POST.Payload payload)
        {
            var message = new Databases.Messages.Table
            {
                Content = payload.Content,
                Receiver = payload.Receiver,
                Sender = payload.Sender,
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow
            };
            await _context.Messages.Insert(message).ExecuteAsync();
            await _messageBus.PublishAsync(new Messages.POST.Messager.Message(message.Id));
            await _hubContext.Clients.All.SendAsync("message-created", message.Id);
            return CreatedAtAction(nameof(Get), new { id = message.Id });
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PUT.Payload payload)
        {
            var existingMessage = await _context.Messages.Where(m => m.Id == payload.Id).ExecuteAsync();
            if (!existingMessage.Any())
            {
                return NotFound();
            }
            await _context.Messages
                .Where(i => i.Id == payload.Id)
                .Select(i => new Databases.Messages.Table
                {
                    Content = payload.Content,
                    Receiver = payload.Receiver,
                    Sender = payload.Sender,
                    Timestamp = DateTime.UtcNow,
                })
                .Update()
                .ExecuteAsync();
            await _messageBus.PublishAsync(new Messages.PUT.Messager.Message(payload.Id));
            await _hubContext.Clients.All.SendAsync("message-updated", payload.Id);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] DELETE.Parameters parameters)
        {
            await _context.Messages
                .Where(i => i.Id == parameters.Id)
                .Delete()
                .ExecuteAsync();
            await _messageBus.PublishAsync(new Messages.DELETE.Messager.Message(parameters.Id));
            await _hubContext.Clients.All.SendAsync("message-deleted", parameters.Id);
            return NoContent();
        }
    }
}
