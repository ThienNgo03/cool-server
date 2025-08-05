namespace Journal.Users.Delete.Messager
{
    public class Handler
    {
        #region [ Fields ] 

        private readonly JournalDbContext _context;
        #endregion

        #region [ CTors ]

        public Handler(JournalDbContext context)
        {
            _context = context;
        }
        #endregion
        public async Task Handle(Message message)
        {
            if (message.DeleteNotes)
            {
                var notes = await _context.Notes.Where(x => x.UserId == message.Id).ToListAsync();
                if (notes.Any())
                {
                    await _context.Notes.Where(x => x.UserId == message.Id).ExecuteDeleteAsync();
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
