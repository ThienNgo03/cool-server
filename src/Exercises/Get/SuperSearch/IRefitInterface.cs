using Refit;

namespace Journal.Exercises.Get.SuperSearch;

public interface IRefitInterface
{
    [Post("/exercises/_search")]
    Task<HttpResponseMessage> SearchAsync([Body] StringContent content);
}