package calendarclient;

import com.github.scribejava.core.model.*;
import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonParser;

import java.io.*;
import java.net.URISyntaxException;
import java.util.Arrays;
import java.util.ArrayList;
import java.util.concurrent.ExecutionException;

public class CalendarWorker
{
    private final String urlPrefix = "https://www.googleapis.com/calendar/v3";
    private final RequestExecutor requestExecutor;
    private final Gson gson = new Gson();
    private final JsonParser parser = new JsonParser();

    public CalendarWorker() throws IOException, URISyntaxException
    {
        requestExecutor = new RequestExecutor();
    }

    public ArrayList<Calendar> getCalendars() throws IOException, ExecutionException, InterruptedException
    {
        String url = urlPrefix + "/users/me/calendarList";
        OAuthRequest request = new OAuthRequest(Verb.GET, url);

        Response response = requestExecutor.execute(request);

        JsonArray elements = parser.parse(response.getBody()).getAsJsonObject().getAsJsonArray("items");
        return new ArrayList<>(Arrays.asList(gson.fromJson(elements, Calendar[].class)));
    }

    public Boolean addCalendar(Calendar calendar) throws IOException, ExecutionException, InterruptedException
    {
        String url = urlPrefix + "/calendars";
        OAuthRequest request = new OAuthRequest(Verb.POST, url);
        request.setPayload(gson.toJson(calendar));

        Response response = requestExecutor.execute(request);
        calendar.setId(parser.parse(response.getBody()).getAsJsonObject().get("id").getAsString());
        return response.getCode() == 200;
    }

    public Boolean updateCalendar(Calendar calendar) throws IOException, ExecutionException, InterruptedException
    {
        String url = urlPrefix + "/calendars/" + calendar.getId();
        OAuthRequest request = new OAuthRequest(Verb.PUT, url);
        request.setPayload(gson.toJson(calendar));

        Response response = requestExecutor.execute(request);
        return response.getCode() == 200;
    }

    public Boolean removeCalendar(Calendar calendar) throws IOException, ExecutionException, InterruptedException
    {
        String url = urlPrefix + "/calendars/" + calendar.getId();
        OAuthRequest request = new OAuthRequest(Verb.DELETE, url);

        Response response = requestExecutor.execute(request);
        return response.getCode() == 204;
    }
}