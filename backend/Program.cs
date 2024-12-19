using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Select an option:");
        Console.WriteLine("1. Test Realtime Database");
        Console.WriteLine("2. Test Firestore Database");

        string choice = Console.ReadLine();

        if (choice == "1")
        {
            Console.WriteLine("Testing Realtime Database...");
            await RealtimeDatabaseTest.TestDatabase();
            // after test 19-12-2024 : Firebase error. Please ensure that you have the URL of your Firebase Realtime Database instance configured correctly.
        }
        else if (choice == "2")
        {
            Console.WriteLine("Testing Firestore Database...");
            await FirestoreTest.TestDatabase();
            // after test 19-12-2024 : Cannot find Default Credentials
        }
        else
        {
            Console.WriteLine("Invalid choice. Exiting...");
        }
    }
}