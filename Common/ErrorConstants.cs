namespace Common
{
    public static class ErrorConstants
    {
        public static string GradeRoomNotFound = "Room not found";
        public static string CardNotFound = "Card not found";
        public static string UserNotFound = "User not found";
        public static string SchoolNotFound = "School not found";
        public static string DriverNotFound = "Driver not found";
        public static string ParentNotFound = "Parent not found";
        public static string DirectorNotFound = "Director not found";
        public static string BusNotFound = "School bus not found";
        public static string DeviceNotFound = "Device not found";
        public static string BoundNotFound = "Bound not found";
        public static string StudentNotFound = "Student not found";
        public static string SupervisorNotFound = "Supervisor not found";

        public static string UserIsNotADriver = "User is not a driver";
        public static string UserIsNotAParent = "User is not a parent";
        public static string UserIsNotADirector = "User is not a director";
        public static string UserIsNotAStudent = "User is not a student";
        public static string UserIsNotASupervisor = "User is not a supervisor";
        public static string StudentDoesNotHaveACard = "Student does not have a card attached";

        public static string InvalidCoordinates = "Invalid Coordinates";
        public static string UserAlreadyAttached = "User is already attached to a school";
        public static string UserNotAttached = "User is not attached to any school";
        public static string CannotDeleteSchool = "Cannot delete school because it has associated users";

        public static string InvalidUsernameOrPassword = "Invalid username or password";
        public const string InvalidEmail = "Email address is not valid";
        public const string InvalidUrl = "URL is not valid";
        public static string NotFound = "NOT FOUND";

        public static string ConcurrencyMessage = "The record you attempted to edit "
                    + "was modified by another user after you got the original value. The "
                    + "edit operation was canceled and the current values in the database "
                    + "have been displayed. If you still want to edit this record, click "
                    + "the Save button again.";
    }
}
