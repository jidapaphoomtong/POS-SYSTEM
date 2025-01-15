using Google.Cloud.Firestore;
using DotNetEnv;
using Google.Cloud.Firestore.V1;
using Newtonsoft.Json;

namespace backend.Services
{
    public class FirestoreDB
    {
        private readonly  FirestoreDb _db;
        public FirestoreDB(FirestoreSettings settings)
        {
            string apiKeyJson = JsonConvert.SerializeObject(
                new {
                    Type = "service_account",
                    project_id = settings.PROJECT_ID,
                    private_key = settings.PRIVATE_KEY.Replace("\\n","\n"),
                    client_email = settings.CLIENT_EMAIL,
                }
            );

            _db = new FirestoreDbBuilder
            {
                ProjectId = settings.PROJECT_ID,
                DatabaseId = settings.DATABASE_ID,
                JsonCredentials = apiKeyJson,
            }.Build();
        }

        public async Task<List<Dictionary<string, object>>> GetCollectionAsync(string collectionName)
        {
            var collectionRef = _db.Collection(collectionName);
            QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();

            List<Dictionary<string, object>> documents = new List<Dictionary<string, object>>();
            foreach (var document in snapshot.Documents)
            {
                documents.Add(document.ToDictionary());
            }

            return documents;
        }

        public CollectionReference Collection(string collectionName)
        {
            return _db.Collection(collectionName);
        }

        internal object CollectionGroup(string v)
        {
            throw new NotImplementedException();
        }
    }
}