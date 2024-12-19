using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Threading.Tasks;

class RealtimeDatabaseTest
{
    public static async Task TestDatabase()
    {
        string databaseUrl = "https://etaxth.firebaseio.com";

        // สร้าง FirebaseClient
        var firebaseClient = new FirebaseClient(databaseUrl);

        // เพิ่มข้อมูลไปยังโหนด "test"
        Console.WriteLine("Adding test data to Realtime Database...");
        var result = await firebaseClient.Child("test").PostAsync(new
        {
            message = "Hello from Realtime Database",
            timestamp = DateTime.UtcNow
        });
        Console.WriteLine($"Data added with key: {result.Key}");

        // ดึงข้อมูลทั้งหมดจากโหนด "test"
        Console.WriteLine("Fetching data from Realtime Database...");
        var data = await firebaseClient.Child("test").OnceAsync<object>();
        foreach (var item in data)
        {
            Console.WriteLine($"Key: {item.Key}, Value: {item.Object}");
        }
    }
}