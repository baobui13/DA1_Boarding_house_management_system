#r "nuget: Npgsql, 8.0.3"
using Npgsql;
using System;

string connStr = "Host=localhost;Database=QLT_DB;Username=postgres;Password=postgres";
using var conn = new NpgsqlConnection(connStr);
conn.Open();
using var cmd = new NpgsqlCommand("SELECT \"Id\", \"ModerationStatus\" FROM \"Properties\"", conn);
using var reader = cmd.ExecuteReader();
int count = 0;
while (reader.Read()) {
    Console.WriteLine($"{reader.GetGuid(0)} - {reader.GetString(1)}");
    count++;
}
Console.WriteLine($"Total: {count}");
