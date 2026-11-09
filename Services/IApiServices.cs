namespace RaceDay.Web.Services
{
    public interface IApiService
    {
        Task<HttpResponseMessage> GetAsync(string endpoint);
        Task<HttpResponseMessage> PostAsync(string endpoint, object data);
        Task<HttpResponseMessage> PutAsync(string endpoint, object data);
        Task<HttpResponseMessage> DeleteAsync(string endpoint);

        Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent content);
    }
}