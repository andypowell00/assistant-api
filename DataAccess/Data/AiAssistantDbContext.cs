using Microsoft.EntityFrameworkCore;

namespace Assistant_API.DataAccess.Data;

public class AiAssistantDbContext : DbContext
{
    public AiAssistantDbContext(DbContextOptions<AiAssistantDbContext> options) : base(options) { }

    // Define your DbSets here
}
