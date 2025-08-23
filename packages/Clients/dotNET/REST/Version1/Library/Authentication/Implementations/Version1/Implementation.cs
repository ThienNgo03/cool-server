

using Refit;
using System.Diagnostics;

namespace Library.Authentication.Implementations.Version1;

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

    public async Task RegisterAsync(Register.Payload payload)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            var refitPayload = new Models.Refit.Register.Payload
            {
                AccountName = payload.AccountName,
                AccountEmail = payload.AccountEmail,
                Password = payload.Password,
                ConfirmPassword = payload.ConfirmPassword,
                PhoneNumber = payload.PhoneNumber
            };

            var response = await this.refitInterface.Register(refitPayload);

            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;
        }
        catch (ApiException ex)
        {
            stopwatch.Stop();
        }
    }

    public async Task<Signin.Response?> SignInAsync(Signin.Payload payload)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            var refitPayload = new Models.Refit.SignIn.Payload
            {
                Account = payload.Account,
                Password = payload.Password
            };

            var refitResponse = await this.refitInterface.SignIn(refitPayload);

            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;

            var response = new Signin.Response
            {
                Token = refitResponse.Content?.Token ?? string.Empty
            };

            return response;
        }
        catch (ApiException ex)
        {
            stopwatch.Stop();
            return null;
        }
    }
}
