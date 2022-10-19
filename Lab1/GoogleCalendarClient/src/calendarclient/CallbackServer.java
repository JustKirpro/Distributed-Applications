package calendarclient;

import com.sun.net.httpserver.HttpServer;

import java.io.IOException;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.util.concurrent.CompletableFuture;

public class CallbackServer
{
    private final HttpServer server;
    private final CompletableFuture<String> oAuthVerifierCF = new CompletableFuture<>();

    public CallbackServer() throws IOException
    {
        server = HttpServer.create(new InetSocketAddress(InetAddress.getLoopbackAddress(), 0), 0);
        server.createContext("/", new OAuthCallbackHandler(this));
        server.setExecutor(null);
    }

    public void start()
    {
        server.start();
    }

    public String getAuthorizationUlr()
    {
        return String.format("http://localhost:%d/", server.getAddress().getPort());
    }

    public CompletableFuture<String> getoAuthVerifierCF()
    {
        return oAuthVerifierCF;
    }

    public void setoAuthVerifier(String value)
    {
        oAuthVerifierCF.complete(value);
        server.stop(0);
    }
}