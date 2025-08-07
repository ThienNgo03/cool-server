namespace Journal.Journeys
{
    [ApiController]
    [Route("api/journeys")]
    public class Controller : ControllerBase
    {
        private readonly IMessageBus _messageBus;
        private readonly Get.Interface _getInterface;
        private readonly ILogger<Controller> _logger;

        private readonly JournalDbContext _context; //biến đại diện cho database

        public Controller(ILogger<Controller> logger,
                          JournalDbContext context, 
                          IMessageBus messageBus,
                          Get.Interface getInterface)
        {
            _logger = logger;
            _context = context; // gán database vào biến(_context) đã tạo
            _messageBus = messageBus;
            _getInterface = getInterface;
        }

        ///Abstraction(Get API): gồm 3 bước chính :
        /// <summary>
        /// 1: Lấy dữ liệu từ storage 
        /// 2: xử lý dữ liệu
        /// 3: Trả về dữ liệu cho người dùng
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] Get.Parameters parameters)// phải có dấu ? sau mỗi property, cho phép để trống chúng khi Get, nếu không sẽ lỗi
        {
            var storage = await _getInterface.GetStorage(parameters);
            var processData = await _getInterface.ProcessStorage(storage);
            var result = await _getInterface.CreateResult(processData);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Post.Payload payload)
        {
            var journey = new Databases.Journal.Tables.Journey.Table //tạo một hàng dữ liệu mới
            {
                Id = Guid.NewGuid(),
                Content = payload.Content,
                Location = payload.Location,
                Date = payload.Date
            };
            _context.Journeys.Add(journey); // add hàng dữ liệu mới vào table
            await _context.SaveChangesAsync(); // lưu lại table
            await _messageBus.PublishAsync(new Post.Messager.Message(journey.Id));
            return CreatedAtAction(nameof(Get), journey.Id);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] Delete.Parameters parameters) // bắt buộc phải có id để tìm nên không cần dấu ?
        {
            var journey = await _context.Journeys.FindAsync(parameters.Id);// chờ để ASP.NET tìm và lấy ra data với id được cho, hàm FindAsync chỉ để tìm khóa chính của bảng
            if (journey == null) //nếu data vẫn là null sau khi tìm nghĩa là không có data với id được cho
            {
                return NotFound();
            }
            _context.Journeys.Remove(journey); //xóa data tìm được khỏi table hiện tại
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Delete.Messager.Message(parameters.Id, parameters.DeleteNotes)); // bắn qua handler
            return NoContent(); //201
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Update.Payload payload)
        {
            var journey = await _context.Journeys.FindAsync(payload.Id);
            if (journey == null)
            {
                return NotFound();
            }
            journey.Content = payload.Content; // cập nhật data cũ với data mà người dùng nhập vào
            journey.Date = payload.Date;
            journey.Location = payload.Location;
            _context.Journeys.Update(journey);
            await _context.SaveChangesAsync();
            await _messageBus.PublishAsync(new Update.Messager.Message(payload.Id));
            return NoContent(); //201
        }
    }
}
