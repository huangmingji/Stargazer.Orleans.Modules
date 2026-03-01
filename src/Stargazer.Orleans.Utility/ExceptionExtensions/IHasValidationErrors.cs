namespace Stargazer.Orleans.Utility.ExceptionExtensions
{
	public interface IHasValidationErrors
    {
        List<ValidationError> ValidationErrors { get; set; }
    }
}

