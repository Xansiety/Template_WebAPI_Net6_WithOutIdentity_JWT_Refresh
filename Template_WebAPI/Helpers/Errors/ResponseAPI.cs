namespace Template_WebAPI.Helpers.Errors;
public class ResponseAPI
{
    public int StatusCode { get; set; }
    public string Message { get; set; }

    //constructor
    public ResponseAPI(int statusCode, string message = null)
    {
        StatusCode = statusCode;
        Message = message ?? /*operador null coallecing si el elemento de la izquierda es null toma el de la derecha*/ GetDefaultMessage(statusCode);
    }

    private string GetDefaultMessage(int statusCode)
    {
        return statusCode switch
        {
            400 => "Haz realizado una petición incorrecta.",
            401 => "Usuario no autorizado.",
            404 => "El recurso que has intentado solicitar no existe.",
            405 => "Este método HTTP no está permitido en el servidor.",
            500 => "Error en el servidor. No eres tu, soy yo. Comunícate con el area de sistemas.",
            _ => "No identificado"
        };
    }
}
