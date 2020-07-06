namespace Api.Auth
{
    public static class AuthPolicy
    {
        /// <summary>
        /// Only Read Permission
        /// </summary>
        public const string Reader = "Reader"; 
        
        /// <summary>
        /// Create,Read,Update,Delete Permissions
        /// </summary>
        public const string Contributor = "Contributor";

        public const string Test = "Test";
    }
}
