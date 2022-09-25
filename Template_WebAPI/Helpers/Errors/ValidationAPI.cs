namespace Template_WebAPI.Helpers.Errors;

public class ValidationAPI : ResponseAPI
{
    public ValidationAPI() : base(400)
    {

    }

    public IEnumerable<string> Errors { get; set; }
} 
