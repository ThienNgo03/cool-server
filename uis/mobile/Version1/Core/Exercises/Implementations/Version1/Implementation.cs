using Core.Exercises.All;

namespace Core.Exercises.Implementations.Version1;

public class Implementation: Interface
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
        var refitParameters = new Models.Refit.All.Parameters
        {
            UserId = parameters.UserId
        };

        try
        {
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
                Id = e.Id,
                ImageUrl = e.ImageUrl,
                Title = e.Title,
                SubTitle = e.SubTitle,
                Description = e.Description,
                Badge = e.Badge,
                PercentageCompleteString = e.PercentageCompleteString,
                PercentageComplete = e.PercentageComplete,
                BadgeTextColor = e.BadgeTextColor,
                BadgeBackgroundColor = e.BadgeBackgroundColor
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new NotImplementedException();
        }
    }

    public async Task<List<string>> CategoriesAsync()
    {
        try 
        {
            var response = await this.refitInterface.Categories();
            if (response is null || response.Content is null)
            {
                return null;
            }
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return response.Content;
        }
        catch (Exception ex)
        {
            throw new NotImplementedException();
        }

    }
}
