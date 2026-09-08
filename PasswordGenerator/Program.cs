using Microsoft.AspNetCore.Identity;

var hasher = new PasswordHasher<object>();
var user = new object();

Console.WriteLine("HASH 111:");
Console.WriteLine(hasher.HashPassword(user, "111"));

Console.WriteLine("HASH 222:");
Console.WriteLine(hasher.HashPassword(user, "222"));

Console.ReadLine();