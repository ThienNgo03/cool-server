using BFF.Databases.Messages;
using BFF.Messages.LoadMessage;
using BFF.Messages.Send;
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
        [HttpGet("load-messages")]
        public async Task<IActionResult> LoadMessages([FromQuery] Parameters parameters)
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

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] Payload payload)
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
            await _messageBus.PublishAsync(new Messages.Send.Messager.Message(message.Id));
            await _hubContext.Clients.All.SendAsync("message-created", message.Id);
            return CreatedAtAction(nameof(LoadMessages), new { id = message.Id });
        }

    }
}
