using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class FirestoreTest
{
    public static async Task TestDatabase()
    {
        string projectId = "etaxth";

        // สร้าง FirestoreDb instance
        FirestoreDb firestoreDb = FirestoreDb.Create(projectId);

        // เพิ่มเอกสารใน Collection "test"
        Console.WriteLine("Adding test data to Firestore...");
        DocumentReference docRef = firestoreDb.Collection("test").Document("test-doc");
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "message", "Hello from Firestore" },
            { "timestamp", DateTime.UtcNow }
        };
        await docRef.SetAsync(data);
        Console.WriteLine("Document added successfully!");

        // ดึงเอกสารทั้งหมดใน Collection "test"
        Console.WriteLine("Fetching data from Firestore Collection...");
        QuerySnapshot snapshot = await firestoreDb.Collection("test").GetSnapshotAsync();
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            Console.WriteLine($"Document ID: {document.Id}");
            Console.WriteLine($"Data: {string.Join(", ", document.ToDictionary())}");
        }
    }
}