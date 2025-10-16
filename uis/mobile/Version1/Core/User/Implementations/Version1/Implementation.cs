using Core.User.All;

namespace Core.User.Implementations.Version1;

public class Implementation : Interface
{

    #region [ Fields ]

    private readonly IRefitInterface refitInterface;
    #endregion

    #region [ CTors ]

    public Implementation(IRefitInterface refitInterface)
    {
        this.refitInterface = refitInterface;
    }

    #endregion
    public async Task<List<Response>> AllAsync(Parameters parameters)
    {
        try
        {
            var refitParameters = new Models.Refit.All.Parameters
            {
                UserId = parameters.UserId,
                UserName = parameters.UserName
            };

            var response = await this.refitInterface.All(refitParameters);

            if (response is null || response.Content is null)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return response.Content.Select(e => new Response
            {
                UserId = e.UserId,
                Title = e.Title,
                SubTitle = e.SubTitle,
                ImageUrl = e.ImageUrl,
                Time = e.Time,
                Status = e.Status
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
