package calendarclient;

import com.github.scribejava.apis.GoogleApi20;
import com.github.scribejava.core.builder.ServiceBuilder;
import com.github.scribejava.core.model.OAuth2AccessToken;
import com.github.scribejava.core.model.OAuthRequest;
import com.github.scribejava.core.model.Response;
import com.github.scribejava.core.oauth.OAuth20Service;

import java.awt.*;
import java.io.IOException;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.concurrent.CompletionException;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Future;

public class RequestExecutor
{
    private Future<OAuth2AccessToken> accessTokenF;
    private final OAuth20Service service;

    public RequestExecutor() throws IOException, URISyntaxException
    {
        CallbackServer callbackServer = new CallbackServer();
        callbackServer.start();

        String apiKey = System.getenv("CALENDAR_API_KEY");
        String apiSecret = System.getenv("CALENDAR_API_SECRET");
        String callback = callbackServer.getAuthorizationUlr();
        String scope = System.getenv("CALENDAR_SCOPE");
        GoogleApi20 instance = GoogleApi20.instance();

        service = new ServiceBuilder(apiKey).apiSecret(apiSecret).callback(callback).scope(scope).build(instance);
        authenticate(callbackServer);
    }

    public Response execute(OAuthRequest request) throws IOException, ExecutionException, InterruptedException
    {
        service.signRequest(accessTokenF.get(), request);
        return service.execute(request);
    }

    private void authenticate(CallbackServer callbackServer) throws URISyntaxException, IOException
    {
        accessTokenF = callbackServer.getoAuthVerifierCF().thenApply(oauthVerifier ->
        {
            try
            {
                return service.getAccessToken(oauthVerifier);
            }
            catch (Exception exception)
            {
                throw new CompletionException(exception);
            }
        });

        String authenticationUrl = service.getAuthorizationUrl();
        Desktop.getDesktop().browse(new URI(authenticationUrl));
    }
}