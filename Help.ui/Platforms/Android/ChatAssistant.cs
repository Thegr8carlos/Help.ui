    using System;
    using System.Threading.Tasks;
    using Azure.AI.OpenAI;
    using Azure;
    using System.IO;
    using OpenAI.Chat;
    using Microsoft.VisualBasic;

public class ChatAssistant
{
    private AzureOpenAIClient _client;
    private string _deployment = "helpie_assistant";

    public ChatAssistant()
    {
        string pass = "1y8RkhQHGCXkc7bhTmZzp7dCWwFHvgFCAJxYRhLbncvgzphw3lS8JQQJ99AKACHrzpqXJ3w3AAABACOGIb6h";
        string endpoint = "https://helpie.openai.azure.com/";
        _client = new AzureOpenAIClient(new Uri(endpoint), new System.ClientModel.ApiKeyCredential(pass));

    }

    public Task<string> AskAsync(string userMessage)
    {
        return Task.Run(async () =>
        {
            try
            {
                if (string.IsNullOrEmpty(userMessage))
                {
                    Console.WriteLine("Mensaje erroneo");
                }
                Console.WriteLine("Mensaje recibido");
                Console.WriteLine(userMessage);
                Console.WriteLine("------");
                OpenAI.Chat.ChatClient ChatInterface = _client.GetChatClient(_deployment);
                var result = await ChatInterface.CompleteChatAsync(
                    new SystemChatMessage("Recibiras de entrada la informacion acerca de la pantalla que el usuario esta viendo en su dispositivo, Explicale al usuario lo que esta observando, para que es, que sirve. Es para personas de la tercera edad asi que deja claro que es todo lo que estan observando"),
                    new UserChatMessage(userMessage)
                );
                
                Console.WriteLine("si se LELGO AQUI");
                Console.WriteLine(result.Value.Content[0].Text);
                // Puedes retornar el resultado aquí si lo necesitas
                return result.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return "Error en la respuesta";
            }
        });
    }
}

