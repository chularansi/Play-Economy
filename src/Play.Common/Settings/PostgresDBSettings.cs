using Microsoft.EntityFrameworkCore;

namespace Play.Common.Settings
{
    public class PostgresDBSettings
    {
        public string Host { get; init; }
        public int Port { get; init; }
        public string Database { get; set; }
        public string PostgresUser { get; set; }
        public string PostgresPassword { get; set; }
        public DbContext AppDbContext { get; set; }

        //$"postgresql://{Host}:{Port}/{Database}?user={PostgresUser}&password={PostgresPassword}"
        public string ConnectionString => $"Host={Host};Port={Port};Username={PostgresUser};Password={PostgresPassword};Database={Database};";
    }
}
