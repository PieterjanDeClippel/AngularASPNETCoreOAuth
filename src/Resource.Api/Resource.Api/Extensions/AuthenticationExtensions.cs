using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Resource.Api.Extensions
{
    public static class AuthenticationExtensions
    {
        public static AuthenticationBuilder AddMyAuthServer(this AuthenticationBuilder builder)
            => builder.AddMyAuthServer(MyAuthServerDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddMyAuthServer(this AuthenticationBuilder builder, Action<MyAuthServerOptions> configureOptions)
            => builder.AddMyAuthServer(MyAuthServerDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddMyAuthServer(this AuthenticationBuilder builder, string authenticationScheme, Action<MyAuthServerOptions> configureOptions)
            => builder.AddMyAuthServer(authenticationScheme, MyAuthServerDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddMyAuthServer(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<MyAuthServerOptions> configureOptions)
            => builder.AddOAuth<MyAuthServerOptions, MyAuthServerHandler>(authenticationScheme, displayName, configureOptions);
    }

    public class MyAuthServerOptions : OAuthOptions
    {
        public MyAuthServerOptions()
        {
            CallbackPath = new Microsoft.AspNetCore.Http.PathString("/signin-my-auth-server");
            AuthorizationEndpoint = MyAuthServerDefaults.AuthorizationEndpoint;
            TokenEndpoint = MyAuthServerDefaults.TokenEndpoint;
            ClientId = "CarfacPlusClient";
            ClientSecret = "tada";
            UserInformationEndpoint = MyAuthServerDefaults.UserInformationEndpoint;
            Scope.Add("email");
            Scope.Add("username");
            
            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "userName");
        }

        public override void Validate()
        {
            base.Validate();
        }
    }

    public class MyAuthServerHandler : OAuthHandler<MyAuthServerOptions>
    {
        public MyAuthServerHandler(IOptionsMonitor<MyAuthServerOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            string v = base.BuildChallengeUrl(properties, redirectUri);
            return v;
        }

        protected override async Task<object> CreateEventsAsync()
        {
            var result = await base.CreateEventsAsync();
            return result;
        }

        protected override void GenerateCorrelationId(AuthenticationProperties properties)
        {
            base.GenerateCorrelationId(properties);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var result = await base.HandleAuthenticateAsync();
            return result;
        }

        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            var result = await base.HandleRemoteAuthenticateAsync();
            return result;
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            await base.HandleChallengeAsync(properties);
        }

        protected override async Task<HandleRequestResult> HandleAccessDeniedErrorAsync(AuthenticationProperties properties)
        {
            var result = await base.HandleAccessDeniedErrorAsync(properties);
            return result;
        }

        protected override async Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {
            var result = await base.ExchangeCodeAsync(context);
            return result;
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Options.UserInformationEndpoint)
            };
            message.Headers.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, "Bearer " + tokens.AccessToken);
            using (var response = await Backchannel.SendAsync(message))
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"An error occurred when retrieving Carfac user information ({response.StatusCode}). Please check if the authentication information is correct.");

                using (var payload = JsonDocument.Parse(content))
                {
                    var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, payload.RootElement);
                    context.RunClaimActions();
                    await Events.CreatingTicket(context);
                    return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
                }
            }
        }
    }

    public static class MyAuthServerDefaults
    {
        public const string AuthenticationScheme = "MyAuthServer";
        public static readonly string DisplayName = "MyAuthServer";
        public static readonly string AuthorizationEndpoint = "https://localhost:44348/Account/Login";
        public static readonly string TokenEndpoint = "https://localhost:44348/Account/Token";
        public static readonly string UserInformationEndpoint = "https://localhost:44348/Account/Me";
    }
}
