namespace TwitterTools
{
    /// <summary>Представляет данные пользователя, предназначен для хранения ключей и другой технической информации.</summary>
    public class UserData
    {
        private const string oauthConsumerKey = "2BTtxS5WTXc9WvCWR4tAsn22b";
        public string OauthConsumerKey { get => oauthConsumerKey; }
        public const string oauthConsumerSecret = "TnOLA7rILnsdBHLno10MaEjnReTt6vOfK049RO8zLKDZnshGvk";
        private string OauthConsumerSecret { get => oauthConsumerSecret; }
        public readonly string CompositeKey;

        public readonly string OauthAccessToken;
        private readonly string OauthAccessTokenSecret;
        public readonly string UserName;
        public readonly string UserID;

        public string OauthVersion { get; set; }
        public string OauthSignatureMethod { get; set; }

        /// <summary>Используется для получения временных ключей.</summary>
        public UserData()
        {
            OauthVersion = "1.0";
            OauthSignatureMethod = "HMAC-SHA1";

            OauthAccessToken = "";
            OauthAccessTokenSecret = "";

            CompositeKey = Tools.GetCompositeKey(OauthConsumerSecret, OauthAccessTokenSecret);
        }
        /// <summary>Предназначено для хранения временных ключей.</summary>
        /// <param name="accessToken">Временный oauth_token.</param>
        /// <param name="accessTokenSecret">Временный oauth_token_secret.</param>
        public UserData(string accessToken, string accessTokenSecret)
        {
            OauthVersion = "1.0";
            OauthSignatureMethod = "HMAC-SHA1";

            OauthAccessToken = accessToken;
            OauthAccessTokenSecret = accessTokenSecret;
            CompositeKey = Tools.GetCompositeKey(OauthConsumerSecret, OauthAccessTokenSecret);
        }
        /// <summary>Предназначено для хранения ключей авторизованного аккаунта.</summary>
        /// <param name="accessToken">oauth_token</param>
        /// <param name="accessTokenSecret">oauth_token_secret</param>
        /// <param name="userID">user_id</param>
        /// <param name="userName">screen_name</param>
        public UserData(string accessToken, string accessTokenSecret, string userID, string userName)
        {
            OauthVersion = "1.0";
            OauthSignatureMethod = "HMAC-SHA1";

            OauthAccessToken = accessToken;
            OauthAccessTokenSecret = accessTokenSecret;
            CompositeKey = Tools.GetCompositeKey(OauthConsumerSecret, OauthAccessTokenSecret);

            UserName = userName;
            UserID = userID;
        }
    }
}
