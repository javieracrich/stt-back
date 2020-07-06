namespace Common
{
    public class Constants
    {
        public static class JwtClaimIdentifiers
        {
            public const string Id = "id",
                                Code = "user_code",
                                Category = "cat",
                                SchoolCode = "school_code",
                                SchoolName = "school_name",
                                Name = "name",
                                ApiAccess = "api_access",
                                UserName = "username";
        }

        public static class TelemetryProperties
        {
            public const string OperationId = "stt-operation-id";
            public const string IpAddress = "ip-address";
            public const string Payload = "payload";
            public const string ClientApp = "stt-app";
        }

        public const string InvalidChars = @"~!#$%^&*()[]{}/;'\""|\<>:;*";

        public const string SchoolCodeHeader = "schoolCode";

    }
}
