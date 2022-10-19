package calendarclient;

public class Calendar
{
    private String id;
    private String summary;
    private String description;

    public Calendar(String summary, String description)
    {
        this.summary = summary;
        this.description = description;
    }

    public String getId()
    {
        return id;
    }

    public void setId(String id)
    {
        this.id = id;
    }

    public String getSummary()
    {
        return summary;
    }

    public void setSummary(String summary)
    {
        this.summary = summary;
    }

    public String getDescription()
    {
        return description;
    }

    public void setDescription(String description)
    {
        this.description = description;
    }

    @Override
    public String toString()
    {
        return String.format("%s %s", summary, description);
    }
}