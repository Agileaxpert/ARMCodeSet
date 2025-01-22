using ARM_APIs.Interface;
using Google.Apis.Auth.OAuth2;
using System.Net.Http;


public class FirebaseTokenService : IFirebase
{
    public async Task<string> GetAccessTokenAsync()
    {
        //string[] scopes = { "https://www.googleapis.com/auth/firebase.messaging" }; 
        //GoogleCredential googleCredential;
        //string scopes = "https://www.googleapis.com/auth/firebase.messaging";

        var bearertoken = "";
        using (var stream = new FileStream("service_key.json", FileMode.Open, FileAccess.Read))
        {
            var credential = GoogleCredential.FromStream(stream).CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
            bearertoken = credential.UnderlyingCredential.GetAccessTokenForRequestAsync().Result;

        }


        return bearertoken;
    }
}