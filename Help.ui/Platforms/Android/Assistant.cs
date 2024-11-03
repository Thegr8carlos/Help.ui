using System;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure;

public class Assistant
{
    private AzureOpenAIClient _client;
    private string _deployment = "helpie_assistant";

    public Assistant()
    {
        string pass = "";
        string endpoint = "https://helpie.openai.azure.com/";
        _client = new AzureOpenAIClient(new Uri(endpoint), new System.ClientModel.ApiKeyCredential(pass));
    }

    
}
