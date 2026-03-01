namespace Stargazer.Orleans.Utility.Snowflake
{
    public class InvalidSystemClock : Exception
    {      
        public InvalidSystemClock(string message) : base(message) { }
    }
}