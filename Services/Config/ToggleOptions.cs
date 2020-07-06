namespace Services
{
    public class ToggleOptions
    {
        /// <summary>
        /// Enable or disable authentication/authorization
        /// </summary>
        public bool AuthEnabled { get; set; }

        /// <summary>
        /// Enable or disable no-sql db 
        /// </summary>
        public bool CosmosEnabled { get; set; }

        public bool RowVersionEnabled { get; set; }
    }
}
