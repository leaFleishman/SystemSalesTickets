using Npgsql;

var cs = "Host=ep-hidden-silence-b2jmgjgg-pooler.c-6.eu-central-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=הסיסמה_שלך;SSL Mode=Require";

try
{
    await using var connection = new NpgsqlConnection(cs);
    await connection.OpenAsync();

    Console.WriteLine("CONNECTED");
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}