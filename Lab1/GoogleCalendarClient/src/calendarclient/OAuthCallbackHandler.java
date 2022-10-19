package calendarclient;

import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpHandler;

import java.io.IOException;
import java.io.OutputStream;
import java.util.HashMap;
import java.util.Map;

public class OAuthCallbackHandler implements HttpHandler
{
    private final CallbackServer callbackServer;

    public OAuthCallbackHandler(CallbackServer callbackServer)
    {
        this.callbackServer = callbackServer;
    }

    @Override
    public void handle(HttpExchange httpExchange) throws IOException
    {
        Map<String, String> queryArgs = queryToMap(httpExchange.getRequestURI().getQuery());

        String oAuthVerifier = queryArgs.get("code");

        if (oAuthVerifier == null)
        {
            writeResponse(httpExchange, 400, "Error. Please restart your application");
        }
        else
        {
            writeResponse(httpExchange, 200, "OK. Please, return to your application");
            callbackServer.setoAuthVerifier(oAuthVerifier);
        }
    }

    public void writeResponse(HttpExchange httpExchange, int returnCode, String response) throws IOException
    {
        httpExchange.sendResponseHeaders(returnCode, response.length());

        OutputStream outputStream = httpExchange.getResponseBody();
        outputStream.write(response.getBytes());
        outputStream.close();
    }

    private Map<String, String> queryToMap(String query)
    {
        Map<String, String> result = new HashMap<>();

        for (String param : query.split("&"))
        {
            String[] pair = param.split("=");
            result.put(pair[0], pair.length > 1 ? pair[1] : "");
        }

        return result;
    }
}